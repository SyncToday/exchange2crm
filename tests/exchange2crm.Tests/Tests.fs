module exchange2crm.Tests

open NUnit.Framework
open exchange2crm
open exchange2crm.Interfaces
open exchange2crm.Grains
open System

let randomString (l : int) =
        let rand = System.Random()
        let chars = "ABCDEFGHIJKLMNOPQRSTUVWUXYZabcdefghijklmnopqrstuvwuxyz0123456789"

        let randomChars = [|for i in 0..l-1 -> chars.[rand.Next(chars.Length)]|]
        new System.String(randomChars)

let randomNumber =
        let rand = System.Random()
        let chars = "0123456789"

        let randomChars = [|for i in 0..8 -> chars.[rand.Next(chars.Length)]|]
        new System.String(randomChars)


[<TestFixture>]
type ``Tests``() = 

    let sourceWithoutCompanyRandom = 
        {
            FirstName   = randomString(6);
            LastName    = randomString(10);
            Company     = "Serenity";
            JobTitle    = "Mechanic";
            Email       = "kaylee@serenity.space";
            PhoneMobile = randomNumber;
            PhoneWork   = randomNumber;
            Notes       = randomString(30);
            UniqueId    = null;
        } :> IContact

    let sourceWithCompanyRandom = 
        {
            FirstName   = randomString(6);
            LastName    = randomString(10);
            Company     = "Test";
            JobTitle    = "Mechanic";
            Email       = "kaylee@serenity.space";
            PhoneMobile = randomNumber;
            PhoneWork   = randomNumber;
            Notes       = randomString(30);
            UniqueId    = null;
        } :> IContact

    [<SetUp>]
    let setup =
        Common.initConsoleLog()
        System.Net.ServicePointManager.ServerCertificateValidationCallback <- 
            (fun _ _ _ _ -> true)

    let AssertAreEqual (a : IContact) (b : IContact) =
        Assert.AreEqual(a.FirstName, b.FirstName)
        Assert.AreEqual(a.LastName, b.LastName)
        Assert.AreEqual(a.JobTitle, b.JobTitle)
        Assert.AreEqual(a.Email, b.Email)
        Assert.AreEqual(a.PhoneMobile, b.PhoneMobile)
        Assert.AreEqual(a.PhoneWork, b.PhoneWork)
        //TODO viz Exchange.createContact 
        //Assert.AreEqual(a.Notes, b.Notes)
    
    let mapToExchangeContact (contact:IContact) (app:Microsoft.Exchange.WebServices.Data.Contact) =
        app.CompanyName <- contact.Company
        Exchange.setEmail(app, contact.Email, Microsoft.Exchange.WebServices.Data.EmailAddressKey.EmailAddress1)
        app.GivenName <- contact.FirstName
        app.Surname <- contact.LastName
        app.JobTitle <- contact.JobTitle       
        //TODO how to assign Notes property?
        //app.Notes <- contact.Notes 
        
        Exchange.setPhone( app, contact.PhoneMobile,  Microsoft.Exchange.WebServices.Data.PhoneNumberKey.MobilePhone )
        Exchange.setPhone( app, contact.PhoneWork,  Microsoft.Exchange.WebServices.Data.PhoneNumberKey.BusinessPhone )          

    [<Test>]
    member public x.``Create new random contact in the Exchange server`` () =
        let c = sourceWithoutCompanyRandom
        let result = Exchange.createContact (mapToExchangeContact c)  
        Assert.IsFalse( result.Id.UniqueId.Equals(String.Empty))        
        let importExchange = new ImportExchangeContactsGrain();
        let r =importExchange.Run()
                
        r
        |>Array.iter (fun y ->
            match y.FirstName.Equals(c.FirstName) with
                |true -> AssertAreEqual c y
                |false -> Assert.IsTrue(1=1)
        )

    [<Test>]
    member public x.``there is always a Test account in the CRM`` () =
        let result = Xrm.getAccount "Test"  
        Assert.IsTrue( result.IsSome )

    [<Test>]
    member public x.``there are no contacts in the Exchange server at the beginning`` () =
        let importExchangeContacts = ImportExchangeContactsGrain()
        let result = Exchange.getContacts (exchange2crm.Grains.ImportExchangeContacts.toSyncedContactExchange) |> Seq.toList
        Assert.IsTrue( result.IsEmpty )

    [<Test>]
    member public x.``creating a random contact without company succeeds`` () =
        let x : SyncedContact = downcast sourceWithoutCompanyRandom
        let result = Xrm.createContact (Xrm.CreateParameters.FillXrmContact(ImportExchangeContacts.toXrmContact x)) x.Company ImportExchangeContacts.toSyncedContactXrm
        Assert.IsTrue(result.IsSome)
        AssertAreEqual result.Value sourceWithoutCompanyRandom
        Assert.AreEqual( result.Value.Company, String.Empty )

    [<Test>]
    member public x.``creating a random contact with existing account succeeds`` () =
        let x : SyncedContact = downcast sourceWithCompanyRandom
        let result = Xrm.createContact (Xrm.CreateParameters.FillXrmContact(ImportExchangeContacts.toXrmContact x)) x.Company ImportExchangeContacts.toSyncedContactXrm
        Assert.IsTrue(result.IsSome)
        AssertAreEqual result.Value sourceWithCompanyRandom
        Assert.AreEqual( result.Value.Company, sourceWithCompanyRandom.Company )
