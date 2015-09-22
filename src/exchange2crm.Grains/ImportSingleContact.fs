namespace exchange2crm.Grains

open System
open System.Threading.Tasks
open Orleans
open exchange2crm.Interfaces
open exchange2crm

type ImportSingleContact() = 
    inherit Orleans.Grain()

    interface IImportSingleContact with
        member this.Import (contact : IContact) =
            Task.Factory.StartNew(
                fun () -> Xrm.createContact(contact)
            )
