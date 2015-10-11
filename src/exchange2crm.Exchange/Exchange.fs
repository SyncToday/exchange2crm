namespace exchange2crm
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

    let public setEmail( r : Contact, emailAddress : string, key : EmailAddressKey ) =     
        let mutable oldEmailAddress : EmailAddress = null
        if r.EmailAddresses.TryGetValue(key, &oldEmailAddress ) then
            r.EmailAddresses.[key] <- null
        if not (String.IsNullOrWhiteSpace(emailAddress)) then
            r.EmailAddresses.[key] <- EmailAddress(emailAddress)

    let public setPhone( r : Contact, number : string, key : PhoneNumberKey ) =     
        let mutable oldNumber : string = null
        if r.PhoneNumbers.TryGetValue(key, &oldNumber ) then
            r.PhoneNumbers.[key] <- null
        if not ( String.IsNullOrWhiteSpace(number) ) then
            r.PhoneNumbers.[key] <- number

    let createContact mapToExchangeContact =         
        let service = getService ()        
        let folder = getContactsFolder service
        let app = new Contact(service)  

        mapToExchangeContact app        
        app.Save(folder.Id)

        app
        

    let getContacts toSyncedContact =
        let service = getService ()
        let folder = getContactsFolder service
        let view = new ItemView(1000)
        let folderItems = folder.FindItems(view)
        folderItems 
        |> Seq.map toSyncedContact
        |> Seq.toArray

    let deleteContacts contactUniqueIds =
        let service = getService ()
        contactUniqueIds 
        |> Seq.map  (fun id -> Contact.Bind(service, ItemId(id)))
        |> Seq.iter (fun xc -> 

            Log.Information(
                "Moving contact {ContactId} to deleted items.", 
                xc.Id.UniqueId
            )

            xc.Delete(DeleteMode.MoveToDeletedItems)
        )
