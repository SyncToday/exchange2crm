namespace exchange2crm.Grains

open System
open System.Threading.Tasks
open Orleans
open exchange2crm
open exchange2crm.Interfaces

type SaveAllContacts() = 
    inherit Orleans.Grain()
    
    let saveSingleContact (gf : IGrainFactory) (contact : IContact) =
        gf.GetGrain<ISaveSingleContact>(Guid.NewGuid()).Import(contact)

    let saveSingleContact2 (gf : IGrainFactory) (contact : IContact) =
        let saveSingleContact = SaveSingleContact()
        saveSingleContact.Import2(contact)

    member this.ImportAll2 (contacts : IContact seq) =
        contacts 
        |> Seq.iter (saveSingleContact2 this.GrainFactory)

    interface exchange2crm.Interfaces.ISaveAllContacts with
        member this.ImportAll (contacts : IContact seq) =
            Task.WhenAll(
                contacts 
                |> Seq.map (saveSingleContact this.GrainFactory)
                |> Seq.toArray
            )

