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

<img src="example-plot.png" width="640" title="Example Plot">

## TODO

These are the planned next steps to add to the language

1) Allow modules to be used within another module (obviously just makes sense)

    - Will use the `<device>` tag just like when using simulations.

    - Inputs to devices will be hidden. You'll need to add another input to your module's list then set the inputs to the sub-device from the added one

2) Busses/"Arrays" (quality of life)

    - Technically not needed as you could add individual wires for everything, but still useful nonetheless

    - Will be called for simulations like `module.input_bus[index]` just as one familiar with programming would expect

If you have any suggestions, feel free to email me at dylantdmt@gmail.com!
