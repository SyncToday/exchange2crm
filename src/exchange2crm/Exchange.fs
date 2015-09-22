namespace exchange2crm
open exchange2crm.Interfaces
open Microsoft.Exchange.WebServices.Data
open Serilog

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
