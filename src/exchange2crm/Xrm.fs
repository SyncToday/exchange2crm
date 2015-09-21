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