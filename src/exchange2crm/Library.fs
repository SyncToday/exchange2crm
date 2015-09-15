namespace exchange2crm

open System
open System.Linq
open FSharp.Data.TypeProviders

/// Documentation for my library
///
/// ## Example
///
///     let h = Library.hello 1
///     printfn "%d" h
///
module Library = 
  
  let xrm = XrmDataProvider<"http://nucrm/nudev/XRMServices/2011/Organization.svc", Username="", Password="">.GetDataContext(server, username, password, "")


  /// Returns 42
  ///
  /// ## Parameters
  ///  - `num` - whatever
  let hello num = 42


  let getCompany name =
    name