namespace exchange2crm.Grains

open System
open System.Threading.Tasks
open Orleans
open exchange2crm.Interfaces
open exchange2crm

type SaveSingleContact() = 
    inherit Orleans.Grain()

    interface ISaveSingleContact with
        member this.Import (contact : IContact) =
            Task.Factory.StartNew(
                fun () -> Xrm.createContact(contact) |> ignore
            )
