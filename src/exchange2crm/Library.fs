namespace exchange2crm

open System
open System.Linq
open FSharp.Data.TypeProviders
open FSharp.Configuration
open Serilog
open Microsoft.Exchange.WebServices.Data

/// Documentation for my library
///
/// ## Example
///
///     let h = Library.hello 1
///     printfn "%d" h
///
module Library = 
  
  type Settings = AppSettings<"app.config">

  let xrm = XrmDataProvider<OrganizationServiceUrl=Secret.BuildCrmServer, Username=Secret.BuildCrmUser, Password=Secret.BuildCrmPassword>.GetDataContext(Settings.CrmServer, Settings.CrmUser, Settings.CrmPassword, "")


  /// Returns 42
  ///
  /// ## Parameters
  ///  - `num` - whatever
  let hello num = 42


  let getCompany name =
    let companyName : string = name
    Log.Debug( "getCompany {Name}", companyName )
    let result = 
        query {
            for account in xrm.accountSet do
            where ( 
                ( account.name = companyName )
            ) 
            select account
        } 
        |> Seq.tryHead
    Log.Debug( "getCompany {Name} => {@Result}", companyName, result )
    result

  let getContacts  =
    let _service = new ExchangeService()
    _service.EnableScpLookup <- true 
    _service.Credentials <- new WebCredentials(Settings.ExchangeUserEmail, Settings.ExchangePassword)
    _service.AutodiscoverUrl(Settings.ExchangeUserEmail, (fun _ -> true) )
    let folder = Folder.Bind(_service, WellKnownFolderName.Contacts)
    let view = new ItemView(1000)
    let folderItems = folder.FindItems(view)
    folderItems.AsEnumerable() |> Seq.cast<Item>

