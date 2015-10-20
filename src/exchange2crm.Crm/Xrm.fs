namespace exchange2crm

open System
open System.Linq
open Serilog
open FSharp.Data.TypeProviders
open Microsoft.Xrm.Sdk

module Xrm = 
    type public XrmProvider = 
        XrmDataProvider<
            OrganizationServiceUrl=Secret.BuildCrmServer, 
            Username=Secret.BuildCrmUser, 
            Password=Secret.BuildCrmPassword
        >

    type CreateParameters =
    | Dict of System.Collections.Generic.Dictionary<string,string>
    | FillXrmContact of (XrmProvider.XrmService.contact -> unit)
    
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
    
    let getContactById (contactId : Guid) toSyncedContact =
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

    let createContact (fillParameters:CreateParameters) company mapBack =
        let ctx = context ()
        let xrmContact = ctx.contactSet.Create()
        match fillParameters with
        | FillXrmContact mapToXrmContact -> mapToXrmContact xrmContact        

        Log.Information("Creating contact {@SyncedContact}", xrmContact)

        xrmContact.emailaddress1 <-
            match xrmContact.emailaddress1.Length > 100 with
            | true -> 
                Log.Information( "Email address {@string} too long. Shortened", xrmContact.emailaddress1)
                xrmContact.emailaddress1.Remove(99);
            | false -> xrmContact.emailaddress1

        Log.Information(
            "Updating OrganizationService entity {@Contact}", 
            xrmContact
        )

        xrmContact.Id <- ctx.OrganizationService.Create(xrmContact)

        let account = 
            match company with
            | null    -> None
            | ""      -> None
            | company -> getAccount company

        match account with
        | None -> 
            Log.Information("Account {AccountName} not found.", company)
        | Some(account) ->

            Log.Information(
                "Found account {AccountName}: {@Account}",
                company,
                account
            )

            associateContactToAccount ctx account xrmContact

        getContactById xrmContact.Id mapBack
