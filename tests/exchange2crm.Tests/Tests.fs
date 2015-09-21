module exchange2crm.Tests

open NUnit.Framework
open exchange2crm

[<TestFixture>]
type ``Tests``() = 

    [<SetUp>]
    let setup =
      Common.initConsoleLog()

    [<Test>]
    member public x.``hello returns 42`` () =
      let result = Library.hello 42
      printfn "%i" result
      Assert.AreEqual(42,result)

    [<Test>]
    member public x.``there is always a Test company`` () =
      let result = Library.getCompany "Test"  
      Assert.IsTrue( result.IsSome )

