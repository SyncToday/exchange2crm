namespace exchange2crm

open Serilog

module Common =
    
    let initLog writer =
        Log.Logger <- LoggerConfiguration()
            .Destructure.FSharpTypes()
            .WriteTo.TextWriter( writer )
            .CreateLogger()
        Log.Information( "Application started" )