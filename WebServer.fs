namespace GiraffeOAuth
open Microsoft.AspNetCore.Hosting
open System.Security.Cryptography.X509Certificates
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.Extensions.Logging
open System.Net
open Microsoft.Extensions.DependencyInjection
open System
open Microsoft.Extensions.Configuration

module WebServer =
    let configureCors (builder : CorsPolicyBuilder) =
        builder.WithOrigins("https://localhost")
               .AllowAnyMethod()
               .AllowAnyHeader()
               |> ignore

    let configureLogging (builder : ILoggingBuilder) =
        let filter (l : LogLevel) = l.Equals LogLevel.Error
        builder.AddFilter(filter).AddConsole().AddDebug() |> ignore            

    let loadCertificate (configuration: IConfiguration)  = 
        let filepath = configuration.GetValue<string> "CertificateFilename"
        let password = configuration.GetValue<string> "CertificatePassword"
        let storename = configuration.GetValue<string> "CertificateStoreName"
        let storelocation = configuration.GetValue<StoreLocation> "CertificateStoreLocation"

        let loadFromStore =
            (
                use store = new X509Store(storename, storelocation)
                store.Open(OpenFlags.ReadOnly)
                let certificate = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", false) // TODO
                certificate.[0]
            )

        let loadFromFile = 
            new X509Certificate2(filepath, password)

        match String.IsNullOrWhiteSpace filepath, String.IsNullOrWhiteSpace password with
        | false, false -> loadFromFile
        | _ -> loadFromStore

    let configureKestrel (options : KestrelServerOptions) = 
        let configuration = options.ApplicationServices.GetRequiredService<IConfiguration>()
        let certificate = loadCertificate configuration

        options.Listen(IPAddress.Loopback, 443, 
            fun (listenOptions) -> listenOptions.UseHttps(certificate) |> ignore
        )     
