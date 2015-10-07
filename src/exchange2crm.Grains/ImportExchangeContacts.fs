namespace exchange2crm.Grains

open System
open System.Threading.Tasks
open Orleans
open exchange2crm
open exchange2crm.Interfaces
open Serilog

type ImportExchangeContacts() = 
    inherit Orleans.Grain()

    //at nepouziva saveallcontacts a singlecontact
    let mutable arr = Array.empty

    member this.Import2 () : Task<IContact[]> = 
        Async.StartAsTask <| async {
            let contacts = Exchange.getContacts ()
            Log.Information( "Got {@Contacts} from Exchange", [| contacts |] )
            
            let cs = (
                contacts
                |> Array.map(fun x -> 
                    Xrm.createContact(x)
                    ))                
            
            arr <- cs
            //let all = SaveAllContacts()
            //all.ImportAll2( contacts )

            Exchange.deleteContacts contacts 
            return cs
        }
        

    member this.DoJob ()  =
       this.Import2().Wait()       

    member this.Run () : IContact[] =
        Common.run (this.DoJob, "ImportExchangeContacts" )
        arr

    interface exchange2crm.Interfaces.IImportExchangeContacts with
        member this.Import () = 
            let grainFactory = this.GrainFactory
            upcast (Async.StartAsTask <| async {
                let contacts = Exchange.getContacts ()
                Log.Information( "From Exchange got {@Contacts}", [| contacts |] )
                let all : ISaveAllContacts = upcast SaveAllContacts()
                do! Async.AwaitTaskVoid (all.ImportAll( contacts ))

                Exchange.deleteContacts contacts
            })