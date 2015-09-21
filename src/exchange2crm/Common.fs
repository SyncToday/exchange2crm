namespace exchange2crm

open Serilog

module Common =
    
    let initConsoleLog () =
        Log.Logger <- LoggerConfiguration()
            .Destructure.FSharpTypes()
            .WriteTo.Console()
            .CreateLogger()
        Log.Information( "Application started" )

    let initLog writer =
        Log.Logger <- LoggerConfiguration()
            .Destructure.FSharpTypes()
            .WriteTo.TextWriter( writer )
            .CreateLogger()
        Log.Information( "Application started" )
