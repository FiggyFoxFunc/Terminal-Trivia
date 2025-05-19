// Terminal Trivia.
// Presents the user with trivia questions to answer.
// Uses the Open Trivia DB API.
open System.Net.Http
open System.Web
open System.Text.Json.Nodes
open System.Text.Json


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

let answers trivia = 
    List.ofArray trivia.incorrect_answers 
    |> (List.insertAt 0 trivia.correct_answer)
    |> List.randomShuffle

let print_question trivia =
    trivia.question
    |> HttpUtility.HtmlDecode
    |> System.Console.WriteLine

let print_answers (answers: string list) =
    let rec iter idx = 
        if idx < 0 then
            ()
        else
            answers[idx]
            |> HttpUtility.HtmlDecode
            |> printfn "%i: %s" idx
            iter (idx - 1)
    (List.length answers) - 1
    |> iter

let get_input (trivia: Trivia) =  
    System.Console.Write "> "
    System.Console.ReadLine() |> int

let is_correct trivia (answers: string list) index =
    if answers[index] = HttpUtility.HtmlDecode trivia.correct_answer then
        System.Console.WriteLine "You got the right answer!"
    else
        printfn "The correct answer was:\n%s" (HttpUtility.HtmlDecode trivia.correct_answer)

[<EntryPoint>]
let main args =
    let triviaJson = get_result |> Async.RunSynchronously
    if triviaJson.Item "response_code" |> int |> response_is_success then
        let (inner: JsonNode) = (triviaJson.Item "results").[0]
        let trivia =  JsonSerializer.Deserialize(inner, JsonSerializerOptions.Default)
        let answers = answers trivia

        print_question trivia
        print_answers answers
        get_input trivia
        |> is_correct trivia answers

        0
    else
        System.Console.WriteLine "Unable to get trivia question."
        1