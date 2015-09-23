namespace exchange2crm

open Serilog

module Secret =
    
    [<Literal>]
    let BuildCrmServer = "https://nucrmdemo.crm4.dynamics.com/XRMServices/2011/Organization.svc"
    [<Literal>]
    let BuildCrmUser = "alans@nucrmdemo.onmicrosoft.com"
    [<Literal>]
    let BuildCrmPassword = "pass@word1"
