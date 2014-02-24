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
    let staticParams = [ProvidedStaticParameter("program", typeof<string>)
                        ProvidedStaticParameter("memorySize", typeof<int>, parameterDefaultValue = 512)]

    let runnerType = ProvidedTypeDefinition(asm, ns, "BrainfuckProvider", Some baseType)

    do runnerType.DefineStaticParameters(
        parameters = staticParams,
        instantiationFunction = (fun typeName parameterValues ->
            match parameterValues with
            | [| :? string as program; :? int as memorySize |]
                -> let memory, output = Run program memorySize
                   let consoleOutput = ProvidedProperty("ConsoleOutput", typeof<string>, IsStatic = true, GetterCode = fun args -> <@@ output @@>)
                   let memoryType = ProvidedTypeDefinition("Memory", Some typeof<obj>)
                   let memoryProperties = memory
                                          |> Seq.mapi (fun i t -> ProvidedProperty(i.ToString(), typeof<int>, IsStatic = true,
                                                                                   GetterCode = fun args -> <@@ t @@>))
                                          |> List.ofSeq

                   let ty = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)

                   do memoryType.AddMembers(memoryProperties)
                   do ty.AddMember(consoleOutput)
                   do ty.AddMember(memoryType)
                   ty
            | _ -> failwith "Unexpected number of parameters"))

    do this.AddNamespace(ns, [runnerType])

[<assembly:TypeProviderAssembly>]
do ()
