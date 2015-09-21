module exchange2crm.Tests

open NUnit.Framework
open exchange2crm

[<TestFixture>]
type ``Tests``() = 

    [<SetUp>]
    let setup =
        Common.initConsoleLog()
        System.Net.ServicePointManager.ServerCertificateValidationCallback <- 
            (fun _ _ _ _ -> true)

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
        Xrm.createContact(
            {
                FirstName   = "Kaylee";
                LastName    = "Frye";
                Company     = "Serenity";
                JobTitle    = "Mechanic";
                Email       = "kaylee@serenity.space";
                PhoneMobile = null;
                PhoneWork   = null;
                Notes       = null;
            }
        )

    [<Test>]
    member public x.``creating a contact with existing account succeeds`` () =
        Xrm.createContact(
            {
                FirstName   = "Kaylee";
                LastName    = "Frye";
                Company     = "Test";
                JobTitle    = "Mechanic";
                Email       = "kaylee@serenity.space";
                PhoneMobile = null;
                PhoneWork   = null;
                Notes       = null;
            }
        )