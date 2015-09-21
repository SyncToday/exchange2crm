namespace exchange2crm.Grains

open System
open System.Threading.Tasks
open Orleans
open exchange2crm.Interfaces
open exchange2crm

type ImportExchangeContactToCRMGrain() = 
    inherit Orleans.Grain()

    interface IImportExchangeContactToCRM with   // Replace grain interface

        // Add implementations of the actual interface methods.
        member this.import ( contact : IContact ) =
            Task.FromResult( upcast Xrm.createContact( contact :?> SyncedContact  ) )