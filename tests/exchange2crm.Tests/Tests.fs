module exchange2crm.Tests

open NUnit.Framework
open exchange2crm

[<TestFixture>]
type ``Tests``() = 

    [<SetUp>]
    let setup =
      Common.initConsoleLog()
      System.Net.ServicePointManager.ServerCertificateValidationCallback <- (fun _ _ _ _ -> true)

    [<Test>]
    member public x.``hello returns 42`` () =
      let result = Library.hello 42
      printfn "%i" result
      Assert.AreEqual(42,result)

    [<Test>]
    member public x.``there is always a Test company in the CRM`` () =
      let result = Library.getCompany "Test"  
      Assert.IsTrue( result.IsSome )

    [<Test>]
    member public x.``there are always contacts in the Exchange server`` () =
      let result = Library.getContacts |> Seq.toList
      Assert.IsFalse( result.IsEmpty )

