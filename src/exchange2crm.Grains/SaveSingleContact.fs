namespace exchange2crm.Grains

#if ORLEANS
open System
open System.Threading.Tasks
open Orleans
open exchange2crm.Interfaces
open exchange2crm

type SaveSingleContact() = 
    inherit Orleans.Grain()

    member this.Import2 (contact : IContact) =
        Xrm.createContact(contact) |> ignore

    interface ISaveSingleContact with
        member this.Import (contact : IContact) =
            Task.Factory.StartNew(
                fun () -> Xrm.createContact(contact) |> ignore
            )
#endif