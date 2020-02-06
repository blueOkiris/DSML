/*
 * Structures for creating a module from tags
 * 
 * Includes wires, registers, and sub devices (based on a class from simulation)
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace DSML {
    class ModuleDevice : Device {
        public Dictionary<string, string> ParentToSelfInputs;

        public ModuleDevice(string name, string baseModuleName, Dictionary<string, bool> initialInputs, Dictionary<string, string> parentToSelfInputs)
                : base(name, baseModuleName, initialInputs) {
            ParentToSelfInputs = parentToSelfInputs;
        }
    }

    class Module {
        public string Name;
        public Dictionary<string, bool> Inputs;
        public Dictionary<string, bool> Outputs;

        public Wire[] Wires;
        public Reg[] Registers;
        public ModuleDevice[] Devices;

        public Module(string name, Dictionary<string, bool> inputs, Dictionary<string, bool> outputs, Wire[] wires, Reg[] registers, ModuleDevice[] devices) {
            Name = name;
            Inputs = inputs;
            Outputs = outputs;
            Wires = wires;
            Registers = registers;
            Devices = devices;
        }

        // Registers could have a wire, an input, or an output as its input
        // Give ALL registers access to EVERYTHING
        // The similar function below is the same, but for wires
        private void AddRegisterAccess() {
            for(int i = 0; i < Registers.Length; i++) {
                foreach(Wire wire in Wires) {
                    if(!Registers[i].Drivers.ContainsKey(wire.Name))
                        Registers[i].Drivers.Add(wire.Name, false);
                }

                foreach(Reg reg in Registers) {
                    if(!Registers[i].Drivers.ContainsKey(reg.Name))
                        Registers[i].Drivers.Add(reg.Name, false);
                }

                foreach(string input in Inputs.Keys) {
                    if(!Registers[i].Drivers.ContainsKey(input))
                        Registers[i].Drivers.Add(input, false);
                }

                foreach(string output in Outputs.Keys) {
                    if(!Registers[i].Drivers.ContainsKey(output))
                        Registers[i].Drivers.Add(output, false);
                }
            }
        }

        private void AddWireAccess() {
            for(int i = 0; i < Wires.Length; i++) {
                foreach(Wire wire in Wires) {
                    if(!Wires[i].Drivers.ContainsKey(wire.Name))
                        Wires[i].Drivers.Add(wire.Name, false);
                }

                foreach(Reg reg in Registers) {
                    if(!Wires[i].Drivers.ContainsKey(reg.Name))
                        Wires[i].Drivers.Add(reg.Name, false);
                }

                foreach(string input in Inputs.Keys) {
                    if(!Wires[i].Drivers.ContainsKey(input))
                        Wires[i].Drivers.Add(input, false);
                }

                foreach(string output in Outputs.Keys) {
                    if(!Wires[i].Drivers.ContainsKey(output))
                        Wires[i].Drivers.Add(output, false);
                }
            }
        }

        private void UpdateRegisters() {
            AddRegisterAccess();

            for(int i = 0; i < Registers.Length; i++) {
                // Update the inputs of the device
                foreach(string input in Inputs.Keys) {
                    if(input == Registers[i].Name)
                        throw new Exception("Cannot assign to input '" + input + "'");
                    
                    if(Registers[i].Drivers.ContainsKey(input))
                        Registers[i].Drivers[input] = Inputs[input];
                }

                foreach(string output in Outputs.Keys) {
                    if(Registers[i].Drivers.ContainsKey(output))
                        Registers[i].Drivers[output] = Outputs[output];
                }

                // This is okay bc the clk will be run first
                Registers[i].Update();

                // Update the outputs of the device
                foreach(string output in Outputs.Keys.ToList()) {
                    if(Registers[i].Name == output)
                        Outputs[output] = Registers[i].Output();
                }
            }
        }

        private void UpdateWires() {
            AddWireAccess();

            for(int i = 0; i < Wires.Length; i++) {
                // Update the inputs of the device
                foreach(string input in Inputs.Keys) {
                    if(input == Wires[i].Name)
                        throw new Exception("Cannot assign to input '" + input + "'");
                    
                    if(Wires[i].Drivers.ContainsKey(input))
                        Wires[i].Drivers[input] = Inputs[input];
                }

                foreach(string output in Outputs.Keys) {
                    if(Wires[i].Drivers.ContainsKey(output))
                        Wires[i].Drivers[output] = Outputs[output];
                }

                // Update our own outputs
                foreach(string output in Outputs.Keys.ToList()) {
                    if(Wires[i].Name == output)
                        Outputs[output] = Wires[i].Output();
                }
            }
        }

        private void UpdateDevices() {
            // The sub-devices already know what inputs they want.
            // There's a map from this class's inputs to its base module's input
            // Therefore, we just need to search all wires, inputs, and outputs for those values
            for(int i = 0; i < Devices.Length; i++) {
                // Update the inputs of the device
                foreach(string input in Inputs.Keys) {
                    if(Devices[i].ParentToSelfInputs.ContainsKey(input))
                        Devices[i].BaseModuleCopy.Inputs[Devices[i].ParentToSelfInputs[input]] = Inputs[input];
                }

                foreach(string output in Outputs.Keys) {
                    if(Devices[i].ParentToSelfInputs.ContainsKey(output))
                        Devices[i].BaseModuleCopy.Inputs[Devices[i].ParentToSelfInputs[output]] = Outputs[output];
                }

                Devices[i].BaseModuleCopy.Update();

                // Update our own outputs from the device
                foreach(string output in Outputs.Keys.ToList()) {
                    if(Devices[i].BaseModuleCopy.Outputs.ContainsKey(output))
                        Outputs[output] = Devices[i].BaseModuleCopy.Outputs[output];
                }
            }
        }

        public void Update() {
            for(int i = 0; i < 4; i++) {
                // Wires
                UpdateWires();

                // Registers
                UpdateRegisters();

                // Sub Devices
                UpdateDevices();
            }
        }
    }

    abstract class IO {
        public Func<Dictionary<string, bool>, bool>[] Driven;
        public string Name;
        public Dictionary<string, bool> Drivers;
        
        public abstract bool Output();
    }

    class Wire : IO {
        public Wire(string name, Func<Dictionary<string, bool>, bool>[] driven, Dictionary<string, bool> drivers) {
            Name = name;
            Driven = driven;
            
            Drivers = drivers;
            Drivers.Add(Name, false);
        }

        public override bool Output() {
            for(int i = 0; i < Driven.Length; i++)
                Drivers[Name] = Driven[i](Drivers);

            return Drivers[Name];
        }
    }

    class Reg : IO {
        public bool PositiveLevel, ActiveLow;
        public bool Default;
        public string ResetName, ClockName;

        public Reg(string name, string clockName, string resetName, bool positiveLevel, bool activeLow, bool def, Func<Dictionary<string, bool>, bool>[] driven, Dictionary<string, bool> drivers) {
            Name = name;
            ClockName = clockName;
            ResetName = resetName;
            Driven = driven;
            PositiveLevel = positiveLevel;
            ActiveLow = activeLow;
            
            Drivers = drivers;
            Drivers.Add(Name, def);
            Drivers.Add(ClockName, false);
            Drivers.Add(ResetName, false);

            Default = def;
        }

        public override bool Output() => Drivers[Name];

        public void Update() {
            if((ActiveLow && !Drivers[ResetName]) || (!ActiveLow && Drivers[ResetName]))
                Drivers[Name] = default;
            else if((PositiveLevel && Drivers[ClockName]) || (!PositiveLevel && Drivers[ClockName])) {
                for(int i = 0; i < Driven.Length; i++)
                    Drivers[Name] = Driven[i](Drivers);
            }
        }
    }
}