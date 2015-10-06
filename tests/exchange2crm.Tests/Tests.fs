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

    let sourceWithoutCompany = 
        {
            FirstName   = "Kaylee";
            LastName    = "Frye";
            Company     = "Serenity";
            JobTitle    = "Mechanic";
            Email       = "kaylee@serenity.space";
            PhoneMobile = String.Empty;
            PhoneWork   = String.Empty;
            Notes       = String.Empty;
            UniqueId    = null;
        } :> IContact

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


    let sourceWithCompany = 
        {
            FirstName   = "Kaylee";
            LastName    = "Frye";
            Company     = "Test";
            JobTitle    = "Mechanic";
            Email       = "kaylee@serenity.space";
            PhoneMobile = String.Empty;
            PhoneWork   = String.Empty;
            Notes       = String.Empty;
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
        Assert.AreEqual(a.Notes, b.Notes)
    
    [<Test>]
    member public x.``Create new random contact in the Exchange server`` () =
        let c = sourceWithoutCompanyRandom
        let result = Exchange.createContact(c)        
        Assert.IsFalse( result.UniqueId.Equals(String.Empty))        
        let importExchange = new ImportExchangeContacts();
        importExchange.Run()

    [<Test>]
    member public x.``there is always a Test account in the CRM`` () =
        let result = Xrm.getAccount "Test"  
        Assert.IsTrue( result.IsSome )



    [<Test>]
    member public x.``there are always contacts in the Exchange server`` () =
        let result = Exchange.getContacts () |> Seq.toList
        Assert.IsFalse( result.IsEmpty )

//    [<Test>]
//    member public x.``creating a contact succeeds`` () =
//        let result = Xrm.createContact( sourceWithoutCompany )
//        AssertAreEqual result sourceWithoutCompany
//        Assert.AreEqual( result.Company, String.Empty )
//
//    [<Test>]
//    member public x.``creating a contact with existing account succeeds`` () =
//        let result = Xrm.createContact( sourceWithCompany )
//        AssertAreEqual result sourceWithCompany
//        Assert.AreEqual( result.Company, sourceWithCompany.Company )

    [<Test>]
    member public x.``creating a random contact without company succeeds`` () =
        let result = Xrm.createContact( sourceWithoutCompanyRandom )
        AssertAreEqual result sourceWithoutCompanyRandom
        Assert.AreEqual( result.Company, String.Empty )

    [<Test>]
    member public x.``creating a random contact with existing account succeeds`` () =
        let result = Xrm.createContact( sourceWithCompanyRandom )
        AssertAreEqual result sourceWithCompanyRandom
        Assert.AreEqual( result.Company, sourceWithCompanyRandom.Company )
