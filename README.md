# DSML

## Description

Digital System Markup Language

An XML-based hardware description language, well kind of.

Not NEARLY as feature-filled as something like Verilog, just my attempt.

Example module:

```
<module name="test" inputs="a,b,clk,rst" outputs="c,d,e">
    <wire name="c">
        <!-- c = (a & b) | b -->
        <and a="a" b="b"></and>
        <or a="c" b="b"></or>
    </wire>
    <reg name="d" clock="clk" reset="rst" rising="true" active-low="false" default="gnd">
        <!-- d = c (will only activate on clock pulse) -->
        c
    </reg>
</module>
```

## Running

`dotnet run <simulation | plot | diagram> <filename>`

*simulation* prints a console output of all your simulations defined in your file

*plot* is the same as simulation, but it creates a window that plots a graph of the results instead of writing to console

*diagram* is unimplemented, but will create a schematic of your device.

Example: `dotnet run plot test.dsml` results in:

<img src="example-plot.png" width="640" title="Example Plot">

## TODO

These are the planned next steps to add to the language add another input to your module's list then set the inputs to the sub-device from the added one

1) More logic gates (necessary!!!)

    - Only have and and or, not even not. Will add more

2) Allow modules to be used within another module (obviously just makes sense)

    - Will use the `<device>` tag just like when using simulations.

    - Inputs to devices will be hidden. You'll need to

3) Busses/"Arrays" (quality of life)

    - Technically not needed as you could add individual wires for everything, but still useful nonetheless

    - Will be called for simulations like `module.input_bus[index]` just as one familiar with programming would expect

If you have any suggestions, feel free to email me at dylantdmt@gmail.com!
