namespace exchange2crm
open Microsoft.Exchange.WebServices.Data

module Exchange =

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
        }

    let getContacts =
        let _service = new ExchangeService()
        _service.EnableScpLookup <- true 
        
        _service.Credentials <- 
            new WebCredentials(
                Settings.ExchangeUserEmail, 
                Settings.ExchangePassword
            )

        _service.AutodiscoverUrl(Settings.ExchangeUserEmail, (fun _ -> true) )

        let folder = Folder.Bind(_service, WellKnownFolderName.Contacts)
        let view = new ItemView(1000)
        let folderItems = folder.FindItems(view)
        folderItems 
        |> Seq.ofType<Contact> 
        |> Seq.map toSyncedContact
        |> Seq.toArray

