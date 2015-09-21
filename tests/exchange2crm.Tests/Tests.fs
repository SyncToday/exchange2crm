module exchange2crm.Tests

do
    Common.initConsoleLog()

open exchange2crm
open NUnit.Framework

[<Test>]
let ``hello returns 42`` () =
  let result = Library.hello 42
  printfn "%i" result
  Assert.AreEqual(42,result)

[<Test>]
let ``there are always Test`` () =
  let result = Library.getCompany "Test"  
  Assert.IsTrue( result.IsSome )

