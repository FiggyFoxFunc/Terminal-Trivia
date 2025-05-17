// Terminal Trivia.
// Presents the user with trivia questions to answer.
// Uses the Open Trivia DB API.
open System.Net.Http
open System.Text.Json.Nodes
open System.Text.Json
open System.Text.RegularExpressions


type Trivia = {
    difficulty: string
    category: string
    question: string
    correct_answer: string
    incorrect_answers: string array
} 

let get_result =
    async {
        use client = new HttpClient()
        let! response = client.GetAsync("https://opentdb.com/api.php?amount=1") |> Async.AwaitTask
        response.EnsureSuccessStatusCode () |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return JsonNode.Parse content


    }

let response_is_success (c: int) = c = 0


let ask_question (trivia: Trivia) = 
    trivia.question
    |> System.Web.HttpUtility.HtmlDecode
    |> System.Console.WriteLine 

    let answers = 
        List.ofArray trivia.incorrect_answers 
        |> (List.insertAt 0 trivia.correct_answer)
        |> List.indexed
    
    for idx, answer in answers do
        printfn "%i: %s" idx answer

//TODO: Finish parsing JSON and then proceed to take and process user input.
[<EntryPoint>]
let main args =
    let triviaJson = get_result |> Async.RunSynchronously
    if triviaJson.Item "response_code" |> int |> response_is_success then
        let (inner: JsonNode) = (triviaJson.Item "results").[0]
        let trivia =  JsonSerializer.Deserialize(inner, JsonSerializerOptions.Default)
        ask_question trivia
        0
    else
        1