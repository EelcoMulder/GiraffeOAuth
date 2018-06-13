namespace GiraffeOAuth

open Giraffe
open Microsoft.AspNetCore.Http
open System.Security.Claims
open System
open Microsoft.Extensions.Logging

module Handlers = 
    let indexHandler (next : HttpFunc) (ctx: HttpContext) =
        let nameClaim = ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Name)

        if ctx.User.Identity.IsAuthenticated then 
            let model     = { Text = nameClaim.Value }
            let view      = Views.index model    
            htmlView view next ctx
        else
            let view      = Views.loginView
            htmlView view next ctx



    let errorHandler (ex : Exception) (logger : ILogger) =
        logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
        clearResponse >=> setStatusCode 500 >=> text ex.Message        

    type App =  HttpFunc -> HttpContext -> HttpFuncResult
    let mustBeLoggedIn : HttpHandler =
        requiresAuthentication (challenge "GitHub") 
    let webApp : App = 
        choose [
            GET >=> 
                choose [
                    route "/" >=>  indexHandler
                ]
            mustBeLoggedIn >=>
                GET >=>
                    choose [
                        route "/secured" >=> indexHandler
                    ]    
            setStatusCode 404 >=> text "Not Found" ]