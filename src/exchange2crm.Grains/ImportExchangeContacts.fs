namespace exchange2crm.Grains

open System
open System.Threading.Tasks
open Orleans
open exchange2crm
open exchange2crm.Interfaces
open Serilog
open Microsoft.Exchange.WebServices.Data

type ImportExchangeContacts() = 
    inherit Orleans.Grain()

    let mutable arr = Array.empty

    let toSyncedContactExchange ( item : Item ) =
        let c: Contact = item :?> Contact

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

    let toSyncedContactXrm (c : exchange2crm.Xrm.XrmProvider.XrmService.contact) =
        let entityReference = 
            c.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("parentcustomerid")
            
        let parentCustomerIdName =
            match entityReference with
            | null -> String.Empty
            | er ->  er.Name
                         
        {
            FirstName   = c.firstname;
            LastName    = c.lastname;
            Company     = parentCustomerIdName;
            JobTitle    = c.jobtitle;
            Email       = c.emailaddress1;
            PhoneMobile = c.mobilephone;
            PhoneWork   = c.telephone1;
            Notes       = c.description
            UniqueId    = null
        }

    let toXrmContact c ( xrmContact : exchange2crm.Xrm.XrmProvider.XrmService.contact ) =
        let email = 
            match c.Email.Length > 100 with
            | true -> 
                Log.Information( "Email address {@string} too long. Shortened", c.Email)
                c.Email.Remove(99);
            | false -> c.Email

        xrmContact.firstname     <- c.FirstName
        xrmContact.lastname      <- c.LastName
        xrmContact.jobtitle      <- c.JobTitle
        xrmContact.emailaddress1 <- email
        xrmContact.mobilephone   <- c.PhoneMobile
        xrmContact.telephone1    <- c.PhoneWork
        xrmContact.description   <- c.Notes        

    member this.Import2 () : Task<IContact[]> = 
        Async.StartAsTask <| async {
            let contacts = Exchange.getContacts toSyncedContactExchange
            Log.Information( "Got {@Contacts} from Exchange", [| contacts |] )
            
            let cs = (
                contacts
                |> Array.map(fun x -> 
                     Xrm.createContact (toXrmContact x) x.Company toSyncedContactXrm
                    )          
             )
            
            arr <- cs

            Exchange.deleteContacts ( contacts |> Seq.map ( fun p -> p.UniqueId ) )
            return cs
        }
        

    member this.DoJob ()  =
       this.Import2().Wait()       

    member this.Run () : IContact[] =
        Common.run (this.DoJob, "ImportExchangeContacts" )
        arr

#if ORLEANS
    interface exchange2crm.Interfaces.IImportExchangeContacts with
        member this.Import () = 
            let grainFactory = this.GrainFactory
            upcast (Async.StartAsTask <| async {
                let contacts = Exchange.getContacts ()
                Log.Information( "From Exchange got {@Contacts}", [| contacts |] )
                let all : ISaveAllContacts = upcast SaveAllContacts()
                do! Async.AwaitTaskVoid (all.ImportAll( contacts ))

                Exchange.deleteContacts contacts
            })
#endif