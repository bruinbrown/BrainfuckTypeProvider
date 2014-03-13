namespace Brainfuck

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open Brainfuck.Interpreter

[<TypeProvider>]
type BrainfuckTypeProvider (cfg:TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()

    let asm = Assembly.GetExecutingAssembly()
    let ns = "Brainfuck.TypeProvider"
    let baseType = typeof<obj>
    let staticParams = [ProvidedStaticParameter("program", typeof<string>, parameterDefaultValue = "")
                        ProvidedStaticParameter("input", typeof<string>, parameterDefaultValue = "")
                        ProvidedStaticParameter("memorySize", typeof<int>, parameterDefaultValue = 512)]

    let runnerType = ProvidedTypeDefinition(asm, ns, "BrainfuckProvider", Some baseType)

    let EvaluateProgramToProperty program =
        program
        |> RunWithoutInput
        |> snd
        |> fun t -> ProvidedProperty("ConsoleOuput",
                                     typeof<string>,
                                     IsStatic = true,
                                     GetterCode = fun args -> <@@ t @@>)

    let CreateErrorProperty () =
        let q = ProvidedProperty("Invalid Program",
                                 typeof<string>,
                                 IsStatic = true,
                                 GetterCode = fun args -> <@@ "Invalid program" @@>)
        q.AddXmlDoc("The interpreter found an error in your program so far. Please correct it.")
        q

    let LazyEvaluate program =
        match CheckValidity program with
        | Valid -> EvaluateProgramToProperty program
        | _ -> CreateErrorProperty ()

    let rec createTypes prog =
        let operators = ["<"; ">"; ",";".";"-";"+";"[";"]"]
        let typs = operators
                   |> List.map (fun t -> let newProg = prog + t
                                         let programProperty = ProvidedProperty("Program", 
                                                                                typeof<string>,
                                                                                IsStatic = true,
                                                                                GetterCode = fun args -> <@@ newProg @@>)
                                         let s = ProvidedTypeDefinition(t, Some typeof<obj>)
                                         s.AddMembersDelayed(fun () -> createTypes newProg)
                                         s.AddMemberDelayed(fun () -> LazyEvaluate newProg)
                                         s.AddMember(programProperty)
                                         s)
        typs

    do runnerType.DefineStaticParameters(
        parameters = staticParams,
        instantiationFunction = (fun typeName parameterValues ->
            match parameterValues with
            | [| :? string as program; :? string as input; :? int as memorySize |]
                -> let memory, output = Run program memorySize input
                   let consoleOutput = ProvidedProperty("ConsoleOutput", 
                                                        typeof<string>,
                                                        IsStatic = true,
                                                        GetterCode = fun args -> <@@ output @@>)

                   let programProperty = ProvidedProperty("Program",
                                                          typeof<string>,
                                                          IsStatic = true,
                                                          GetterCode = fun args -> <@@ program @@>)
                   let ty = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)
                   ty.AddMembersDelayed(fun () -> createTypes program)
                   ty.AddMember(consoleOutput)
                   ty.AddMember(programProperty)
                   ty
            | _ -> failwith "Unexpected number of parameters"))

    do this.AddNamespace(ns, [runnerType])

[<assembly:TypeProviderAssembly>]
do ()
