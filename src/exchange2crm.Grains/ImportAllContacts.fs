namespace exchange2crm.Grains

open System
open System.Threading.Tasks
open Orleans
open exchange2crm
open exchange2crm.Interfaces

type ImportAllContacts() = 
    inherit Orleans.Grain()

    interface exchange2crm.Interfaces.IImportAllContacts with
        member this.ImportAll (contacts : IContact seq) =
            Task.Factory.StartNew(
                fun () -> contacts |> Seq.map Xrm.createContact
            )
