namespace exchange2crm

module Seq = 
    let ofType<'a> (source : System.Collections.IEnumerable) : seq<'a> =
       seq {
          for item in source do
             match item with
             | :? 'a as x -> yield x
             | _ -> ()
       }