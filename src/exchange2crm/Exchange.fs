namespace exchange2crm
open exchange2crm.Interfaces
open Microsoft.Exchange.WebServices.Data
open Serilog
open System 

module Exchange =

    let private getService () =
        let service = new ExchangeService()
        service.EnableScpLookup <- true 
        
        service.Credentials <- 
            new WebCredentials(
                Settings.ExchangeUserEmail, 
                Settings.ExchangePassword
            )

        service.AutodiscoverUrl(Settings.ExchangeUserEmail, (fun _ -> true))
        service

    let private getContactsFolder service =
        Folder.Bind(service, WellKnownFolderName.Contacts)

    let private toSyncedContact (c: Contact) =
        
        let email key = 
            if c.EmailAddresses.Contains(key) then
                c.EmailAddresses.[key].Address
            else
                null
        
        let phone key =
            if c.PhoneNumbers.Contains(key) then
                c.PhoneNumbers.[key]
            else
                null
        
        {
            FirstName   = c.GivenName;
            LastName    = c.Surname;
            Company     = c.CompanyName;
            JobTitle    = c.JobTitle;
            Email       = email EmailAddressKey.EmailAddress1
            PhoneMobile = phone PhoneNumberKey.MobilePhone
            PhoneWork   = phone PhoneNumberKey.BusinessPhone
            Notes       = c.Notes
            UniqueId    = c.Id.UniqueId
        }

    let private setEmail( r : Contact, emailAddress : string, key : EmailAddressKey ) =     
        let mutable oldEmailAddress : EmailAddress = null
        if r.EmailAddresses.TryGetValue(key, &oldEmailAddress ) then
            r.EmailAddresses.[key] <- null
        if not (String.IsNullOrWhiteSpace(emailAddress)) then
            r.EmailAddresses.[key] <- EmailAddress(emailAddress)

    let private setPhone( r : Contact, number : string, key : PhoneNumberKey ) =     
        let mutable oldNumber : string = null
        if r.PhoneNumbers.TryGetValue(key, &oldNumber ) then
            r.PhoneNumbers.[key] <- null
        if not ( String.IsNullOrWhiteSpace(number) ) then
            r.PhoneNumbers.[key] <- number

    let createContact (contact : IContact) =         
        let service = getService ()        
        let folder = getContactsFolder service
        let app = new Contact(service)  

        app.CompanyName <- contact.Company
        setEmail(app, contact.Email, EmailAddressKey.EmailAddress1)
        app.GivenName <- contact.FirstName
        app.Surname <- contact.LastName
        app.JobTitle <- contact.JobTitle        
        
        setPhone( app, contact.PhoneMobile,  PhoneNumberKey.MobilePhone )
        setPhone( app, contact.PhoneWork,  PhoneNumberKey.BusinessPhone )        
        app.Save(folder.Id)

        app.Id
        

    let getContacts () =
        let service = getService ()
        let folder = getContactsFolder service
        let view = new ItemView(1000)
        let folderItems = folder.FindItems(view)
        folderItems 
        |> Seq.ofType<Contact> 
        |> Seq.map toSyncedContact
        |> Seq.cast<Interfaces.IContact>
        |> Seq.toArray

    let deleteContacts (contacts : IContact seq) =
        let service = getService ()
        contacts 
        |> Seq.map  (fun c  -> ItemId(c.UniqueId))
        |> Seq.map  (fun id -> Contact.Bind(service, id))
        |> Seq.iter (fun xc -> 

            Log.Information(
                "Moving contact {ContactId} to deleted items.", 
                xc.Id.UniqueId
            )

            xc.Delete(DeleteMode.MoveToDeletedItems)
        )
