module exchange2crm.Tests

open NUnit.Framework
open exchange2crm
open System

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
        }

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
        }

    [<SetUp>]
    let setup =
        Common.initConsoleLog()
        System.Net.ServicePointManager.ServerCertificateValidationCallback <- 
            (fun _ _ _ _ -> true)

    let AssertAreSame a b =
        Assert.AreEqual( a.FirstName, b.FirstName )
        Assert.AreEqual( a.LastName, b.LastName )
        Assert.AreEqual( a.JobTitle, b.JobTitle )
        Assert.AreEqual( a.Email, b.Email )
        Assert.AreEqual( a.PhoneMobile, b.PhoneMobile )
        Assert.AreEqual( a.PhoneWork, b.PhoneWork )
        Assert.AreEqual( a.Notes, b.Notes )


    [<Test>]
    member public x.``there is always a Test account in the CRM`` () =
        let result = Xrm.getAccount "Test"  
        Assert.IsTrue( result.IsSome )



    [<Test>]
    member public x.``there are always contacts in the Exchange server`` () =
        let result = Exchange.getContacts |> Seq.toList
        Assert.IsFalse( result.IsEmpty )

    [<Test>]
    member public x.``creating a contact succeeds`` () =
        let result = Xrm.createContact( sourceWithoutCompany )
        AssertAreSame result sourceWithoutCompany
        Assert.AreEqual( result.Company, String.Empty )

    [<Test>]
    member public x.``creating a contact with existing account succeeds`` () =
        let result = Xrm.createContact( sourceWithCompany )
        AssertAreSame result sourceWithCompany
        Assert.AreEqual( result.Company, sourceWithCompany.Company )
