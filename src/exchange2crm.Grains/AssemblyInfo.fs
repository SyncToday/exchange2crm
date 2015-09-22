namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("exchange2crm.Grains")>]
[<assembly: AssemblyProductAttribute("exchange2crm")>]
[<assembly: AssemblyDescriptionAttribute("Import Office 365 Exchange Contact to Microsoft Dynamics CRM")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
