namespace exchange2crm

module Async =
    let AwaitTaskVoid : (System.Threading.Tasks.Task -> Async<unit>) =
        Async.AwaitIAsyncResult >> Async.Ignore