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

    let getAccountById (accountId : string) =
        Log.Information("Searching for account {AccountId}", accountId)

        let ctx = context()
        
        let result = 
            let accountIdG = ref Guid.Empty
            if Guid.TryParse( accountId, accountIdG ) then
                query {
                    for account in ctx.accountSet do
                    where (account.accountid = accountIdG.Value)
                    select account
                } 
                |> Seq.tryHead
            else 
                None
        
        Log.Information(
            "Found account {AccountId} => {@Account}", 
            accountId, 
            result
        )

        result

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
        
        Log.Information(
            "Found contact {ContactId} => {@Contact}", 
            contactId, 
            result
        )

        result

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

    let private toSyncedContact (c: XrmProvider.XrmService.contact ) =   
        let accountName ( accounts : IQueryable<XrmProvider.XrmService.account> )  =
            let account = accounts |> Seq.tryHead 
            match account with
            | Some(a) -> a.name
            | None -> String.Empty

        {
            FirstName   = c.firstname;
            LastName    = c.lastname;
            Company     = accountName( c.``Parent of contact_customer_accounts (account)`` );
            JobTitle    = c.jobtitle;
            Email       = c.emailaddress1;
            PhoneMobile = c.mobilephone;
            PhoneWork   = c.telephone1;
            Notes       = c.description
        }


    let createContact (c : SyncedContact) =
        let ctx = context ()
        let xrmContact = ctx.contactSet.Create()

        Log.Information("Creating contact {@SyncedContact}", c)

        xrmContact.firstname     <- c.FirstName
        xrmContact.lastname      <- c.LastName
        xrmContact.jobtitle      <- c.JobTitle
        xrmContact.emailaddress1 <- c.Email
        xrmContact.mobilephone   <- c.PhoneMobile
        xrmContact.telephone1    <- c.PhoneWork
        xrmContact.description   <- c.Notes

        Log.Information(
            "Updating OrganizationService entity {@Contact}", 
            xrmContact
        )

        xrmContact.Id <- ctx.OrganizationService.Create(xrmContact)

        let account = getAccount c.Company

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

        toSyncedContact( (getContactById xrmContact.Id).Value )