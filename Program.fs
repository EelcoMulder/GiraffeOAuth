namespace GiraffeOAuth

open Giraffe
open System
open System.IO
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open Handlers
open WebServer
open Microsoft.Extensions.Configuration

module App =

    let configureApp (app : IApplicationBuilder) =
        let env = app.ApplicationServices.GetService<IHostingEnvironment>()
        (match env.IsDevelopment() with
        | true  -> app.UseDeveloperExceptionPage()
        | false -> app.UseGiraffeErrorHandler errorHandler)
            .UseCors(configureCors)
            .UseStaticFiles()
            .UseAuthentication()
            .UseGiraffe(webApp)   
        
    let configureServices (services : IServiceCollection) =
        services.AddAuthentication(fun options ->
                options.DefaultAuthenticateScheme <- CookieAuthenticationDefaults.AuthenticationScheme
                options.DefaultSignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
                options.DefaultChallengeScheme <- "GitHub"
            )
            .AddCookie()
            .AddGitHub(fun options -> 
                options.ClientId <- "<Your ClientId>"
                options.ClientSecret <- "<Your ClientSecret>"
                options.CallbackPath <- new PathString("/signin-github") 

                options.AuthorizationEndpoint <- "https://github.com/login/oauth/authorize"
                options.TokenEndpoint <- "https://github.com/login/oauth/access_token"
                options.UserInformationEndpoint <- "https://api.github.com/user"  
            ) 
            .AddFacebook(fun options ->
                options.AppId <- "<Your AppId>"
                options.AppSecret <- "<Your AppSecret>"
            )
            |> ignore
        services.AddCors()    |> ignore
        services.AddGiraffe() |> ignore

    let configureConfig (w: WebHostBuilderContext) (c: IConfigurationBuilder) =
        c.AddJsonFile("appsettings.json") |> ignore
        ()
        
    [<EntryPoint>]
    let main _ =
        let contentRoot = Directory.GetCurrentDirectory()
        let webRoot     = Path.Combine(contentRoot, "WebRoot")
        WebHostBuilder()
            .UseKestrel(configureKestrel)
            .UseContentRoot(contentRoot)
            .UseIISIntegration()
            .UseWebRoot(webRoot)
            .ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureConfig)
            .Configure(Action<IApplicationBuilder> configureApp)
            .ConfigureServices(configureServices)
            .ConfigureLogging(configureLogging)
            .Build()
            .Run()
        0
