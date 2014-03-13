namespace Brainfuck

type ProgramState =
    | Valid
    | TooManyLeft
    | TooManyRight

module Interpreter =
    let FindJump dir (program:char []) cidx =
        let s = System.Collections.Generic.Stack<int>()
        let rec NextChar index =
            match program.[index], dir with
            | '[', 1
            | ']', -1 -> s.Push(index)
                         NextChar (index+dir)
            | ']', 1
            | '[', -1 -> if s.Count = 0 then (index+1)
                         else
                             s.Pop() |> ignore
                             NextChar (index+dir)
            | _ -> NextChar (index+dir)
        NextChar (cidx+dir)
    
    let FindForwardJump = FindJump 1
    
    let FindBackwardJump = FindJump -1

    let rec Execute (program:string) ip dp (data:int []) output (input:string) inputIndex =
        if ip = program.Length then data,output
        else
            match program.[ip] with
            | '>' -> Execute program (ip+1) (dp+1) data output input inputIndex
            | '<' -> Execute program (ip+1) (dp-1) data output input inputIndex
            | ',' -> let k = input.[inputIndex] |> int
                     data.[dp] <- k
                     Execute program (ip+1) dp data output input (inputIndex+1)
            | '.' -> let o =  output + sprintf "%c" (data.[dp] |> char)
                     Execute program (ip+1) dp data o input inputIndex
            | '+' -> do data.[dp] <- data.[dp] + 1
                     Execute program (ip+1) dp data output input inputIndex
            | '-' -> do data.[dp] <- data.[dp] - 1
                     Execute program (ip+1) dp data output input inputIndex
            | '[' -> let idx = if data.[dp] = 0 then (FindForwardJump (program.ToCharArray()) ip) else (ip+1)
                     Execute program idx dp data output input inputIndex
            | ']' -> let idx = if data.[dp] <> 0 then (FindBackwardJump (program.ToCharArray()) ip) else (ip+1)
                     Execute program idx dp data output input inputIndex
            | _ -> Execute program (ip+1) dp data output input inputIndex
    
    let Run program memorySize input = Execute program 0 0 (Array.zeroCreate memorySize) "" input 0
    
    let RunWithoutInput program = Run program 32 ""

    let CheckValidity program =
        let s = System.Collections.Generic.Stack<_>()
        let mutable state = Valid
        for c in program do
            if c = '[' then s.Push()
            else if c = ']' then if s.Count = 0 then state<-TooManyRight else s.Pop () |> ignore
            else ()
        match state with
        | TooManyRight -> TooManyRight
        | _ -> if s.Count = 0 then Valid else TooManyLeft