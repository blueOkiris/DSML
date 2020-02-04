# DSML

## Description

Digital System Markup Language

An XML-like alternative to Verilog, well kind of. Not NEARLY as feature-filled.

## Running

`dotnet run <simulation | plot | diagram> <filename>`

*simulation* prints a console output of all your simulations defined in your file

*plot* is the same as simulation, but it creates a window that plots a graph of the results instead of writing to console

*diagram* is unimplemented, but will create a schematic of your device.

Example: `dotnet run plot test.dsml` results in:

![Image of example plot]
(https://github.com/blueOkiris/DSML/example-plot.png)
