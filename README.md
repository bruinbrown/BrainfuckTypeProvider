BrainfuckTypeProvider
=====================

Compiler-ception.

## What is it?
It's a type provider which executes a given brainfuck program and returns the console output of that program.

## How do I use it?
```
Brainfuck.TypeProvider.BrainfuckProvider<"+++++++++++++++++++++++++++++++++++++.">.ConsoleOutput
```  
This will take in a BF program as a string and then return the ouput straight away.  
  
Sometimes you also want your programs to have input, to do this, pass the input string as a static parameter.  
```
Brainfuck.TypeProvider.BrainfuckProvider<",+.", "a">.ConsoleOutput
```  
You can also pass in a chosen size for the number of memory elements available to the interpreter, if none is provided, then it defaults to 512.  
  
Programs can also be written using the F# type system such that you can start to write a program in the string static parameter and then expand upon it further.  
```
Brainfuck.TypeProvider.BrainfuckProvider<"+++++++++++++++++++++++++++++++++++++.">.``+``.``.``.ConsoleOutput
```
You can also access the program so far from this by replacing the ```ConsoleOuput``` with ```Program```.  
