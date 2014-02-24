namespace Brainfuck

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

    let rec Execute (program:string) ip dp (data:int []) output =
        if ip = program.Length then data,output
        else
            match program.[ip] with
            | '>' -> Execute program (ip+1) (dp+1) data output
            | '<' -> Execute program (ip+1) (dp-1) data output
            | ',' -> let k = System.Console.ReadKey().KeyChar |> int
                     data.[dp] <- k
                     Execute program (ip+1) dp data output
            | '.' -> let o =  output + sprintf "%c" (data.[dp] |> char)
                     Execute program (ip+1) dp data o
            | '+' -> do data.[dp] <- data.[dp] + 1
                     Execute program (ip+1) dp data output
            | '-' -> do data.[dp] <- data.[dp] - 1
                     Execute program (ip+1) dp data output
            | '[' -> let idx = if data.[dp] = 0 then (FindForwardJump (program.ToCharArray()) ip) else (ip+1)
                     Execute program idx dp data output
            | ']' -> let idx = if data.[dp] <> 0 then (FindBackwardJump (program.ToCharArray()) ip) else (ip+1)
                     Execute program idx dp data output
            | _ -> Execute program (ip+1) dp data output
    
    let Run program memorySize = Execute program 0 0 (Array.zeroCreate memorySize) ""
