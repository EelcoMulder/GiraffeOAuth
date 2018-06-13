namespace GiraffeOAuth

open Giraffe
open GiraffeViewEngine

type Message =
    {
        Text : string
    }

module Views =
    let layout (content: XmlNode list) =
        html [] [
            head [] [
                title []  [ encodedText "Giraffe on Asp.Net Core with OAuth" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
            ]
            body [] content
        ]

    let partial () =
        h1 [] [ encodedText "Hello Giraffe" ]

    let index (model : Message) =
        [
            partial()
            p [] [ encodedText model.Text ]
        ] |> layout

    let loginView = 
        [
            partial()
            p[] [ a [_href "https://localhost/secured"] [encodedText "GoTo /secured"] ]
        ] |> layout