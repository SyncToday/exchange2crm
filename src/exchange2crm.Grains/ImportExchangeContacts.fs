namespace exchange2crm.Grains

open System
open System.Threading.Tasks
open Orleans
open exchange2crm
open exchange2crm.Interfaces

type ImportExchangeContacts() = 
    inherit Orleans.Grain()

    interface exchange2crm.Interfaces.IImportExchangeContacts with
        member this.Import () = 
            let grainFactory = this.GrainFactory
            upcast (Async.StartAsTask <| async {
                let contacts = Exchange.getContacts ()
                let all = grainFactory.GetGrain<ISaveAllContacts>(0L)

                do! Async.AwaitTaskVoid (all.ImportAll(contacts))

                Exchange.deleteContacts contacts
            })