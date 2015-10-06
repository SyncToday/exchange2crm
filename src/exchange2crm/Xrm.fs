namespace exchange2crm

open System
open System.Linq
open Serilog
open FSharp.Data.TypeProviders
open Microsoft.Xrm.Sdk

module Xrm = 
    type private XrmProvider = 
        XrmDataProvider<
            OrganizationServiceUrl=Secret.BuildCrmServer, 
            Username=Secret.BuildCrmUser, 
            Password=Secret.BuildCrmPassword
        >
    
    let private context () = 
        XrmProvider.GetDataContext(
            Settings.CrmServer, 
            Settings.CrmUser, 
            Settings.CrmPassword, 
            domain=""
        )

    let private associateContactToAccount 
        (ctx     : XrmProvider.XrmService)
        (account : XrmProvider.XrmService.account) 
        (contact : XrmProvider.XrmService.contact) =

            let relationship = Relationship("contact_customer_accounts")

            let related = EntityReferenceCollection()
            related.Add(EntityReference(account.LogicalName, account.Id))
            
            Log.Information(
                "Associating contact {ContactId} to account {AccountId}",
                contact.Id,
                account.Id
            )

            ctx.OrganizationService.Associate(
                contact.LogicalName,
                contact.Id,
                relationship,
                related
            )

    let private toSyncedContact (c : XrmProvider.XrmService.contact) =
        let entityReference = 
            c.GetAttributeValue<EntityReference>("parentcustomerid")
            
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
    
    let getContactById (contactId : Guid) =
        Log.Information("Searching for contact {ContactId}", contactId)

        let ctx = context()
        
        let result = 
            query {
                for contact in ctx.contactSet do
                where (contact.contactid = contactId)
                select contact
            } 
            |> Seq.tryHead
        
        match result with
        | Some(r) -> 
            Log.Information(
                "Found contact {ContactId} => {@Contact}", 
                contactId, 
                result
            )
            Some(toSyncedContact r)

        | None -> 
            Log.Information(
                "Contact {ContactId} not found", 
                contactId
            )
            None

    let getAccount (accountName : string) =
        Log.Information("Searching for account {AccountName}", accountName)

        let ctx = context()
        
        let result = 
            query {
                for account in ctx.accountSet do
                where (account.name = accountName)
                select account
            } 
            |> Seq.tryHead
        
        Log.Information(
            "Found account {AccountName} => {@Account}", 
            accountName, 
            result
        )

        result


    let createContact (c : Interfaces.IContact) =
        let ctx = context ()
        let xrmContact = ctx.contactSet.Create()

        Log.Information("Creating contact {@SyncedContact}", c)

        let email = match c.Email.Length > 100 with
            | true -> c.Email.Remove(99);
            | false -> c.Email

        xrmContact.firstname     <- c.FirstName
        xrmContact.lastname      <- c.LastName
        xrmContact.jobtitle      <- c.JobTitle
        xrmContact.emailaddress1 <- email
        xrmContact.mobilephone   <- c.PhoneMobile
        xrmContact.telephone1    <- c.PhoneWork
        xrmContact.description   <- c.Notes

        Log.Information(
            "Updating OrganizationService entity {@Contact}", 
            xrmContact
        )

        xrmContact.Id <- ctx.OrganizationService.Create(xrmContact)

        let account = 
            match c.Company with
            | null    -> None
            | ""      -> None
            | company -> getAccount company

        match account with
        | None -> 
            Log.Information("Account {AccountName} not found.", c.Company)
        | Some(account) ->

            Log.Information(
                "Found account {AccountName}: {@Account}",
                c.Company,
                account
            )

            associateContactToAccount ctx account xrmContact

        (getContactById xrmContact.Id).Value :> Interfaces.IContact