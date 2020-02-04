/*
 * This is just the support classes for DigitalSystem
 * Moved them to another file for readability
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSML {
    struct Clock {
        public double Frequency, StartTime, Period;
        public string DeviceName;
        public string InputId;
        public bool InitialValue;

        public Clock(double frequency, double startTime, string deviceName, string inputId, bool initialValue) {
            Frequency = frequency;
            StartTime = startTime;
            DeviceName = deviceName;
            InputId = inputId;
            InitialValue = initialValue;

            Period = 1 / Frequency;
        }

        public Assignment[] ToAssignments(double maxTime) {
            List<Assignment> assignments = new List<Assignment>();

            double i = StartTime;
            bool currentValue = InitialValue;
            while(i < maxTime) {
                assignments.Add(new Assignment(i, DeviceName, InputId, currentValue));

                currentValue = !currentValue;
                i += Period;
            }

            return assignments.ToArray();
        }
    }

    class Assignment : IComparable {
        public double Time;
        public string InputId;
        public bool Value;
        public string DeviceName;

        public Assignment(double time, string deviceName, string inputId, bool value) {
            Time = time;
            InputId = inputId;
            Value = value;
            DeviceName = deviceName;
        }

        public int CompareTo(object other) {
            return Time.CompareTo(((Assignment) other).Time);
        }
    }

    class SimOutput {
        public string DeviceName;
        public string InputId;

        public SimOutput(string deviceName, string inputId) {
            DeviceName = deviceName;
            InputId = inputId;
        }
    }

    class Device {
        public string Name;
        public string BaseModuleName;
        public Module BaseModuleCopy;
        public Dictionary<string, bool> InitialInputs;

        // Module is a class so it can be passed by value
        public Device(string name, string baseModuleName, Dictionary<string, bool> initialInputs) {
            Name = name;
            BaseModuleName = baseModuleName;
            InitialInputs = initialInputs;

            // Blank module
            BaseModuleCopy = new Module(BaseModuleName, new Dictionary<string, bool>(), new Dictionary<string, bool>(), new Wire[] {}, new Reg[] {});
        }

        public void Initialize(Dictionary<string, Module> moduleTemplates) {
            BaseModuleCopy = new Module(
                                    moduleTemplates[BaseModuleName].Name,
                                    new Dictionary<string, bool>(), 
                                    new Dictionary<string, bool>(),
                                    new Wire[] {},
                                    new Reg[] {});

            foreach(string input in moduleTemplates[BaseModuleName].Inputs.Keys)
                BaseModuleCopy.Inputs.Add(input, moduleTemplates[BaseModuleName].Inputs[input]);
            foreach(string output in moduleTemplates[BaseModuleName].Outputs.Keys)
                BaseModuleCopy.Outputs.Add(output, moduleTemplates[BaseModuleName].Outputs[output]);

            List<Wire> wires = new List<Wire>();
            foreach(Wire wire in moduleTemplates[BaseModuleName].Wires)
                wires.Add(wire);
            
            List<Reg> regs = new List<Reg>();
            foreach(Reg reg in moduleTemplates[BaseModuleName].Registers)
                regs.Add(reg);
            
            BaseModuleCopy.Wires = wires.ToArray();
            BaseModuleCopy.Registers = regs.ToArray();

            foreach(string input in InitialInputs.Keys)
                BaseModuleCopy.Inputs[input] = InitialInputs[input];
        }
    }

    class Simulation {
        public string Name;
        public Dictionary<string, Device> Devices;
        public List<Assignment> Assignments;
        public Dictionary<string, SimOutput> Outputs;

        public Dictionary<string, Module> ModuleTemplates;

        public Simulation(string name, Dictionary<string, Device> devices, List<Assignment> assignments, Dictionary<string, SimOutput> outputs) {
            Devices = devices;
            Assignments = assignments;
            Outputs = outputs;
            Name = name;

            ModuleTemplates = new Dictionary<string, Module>();
        }

        public void SetModules(Dictionary<string, Module> moduleTemplates) {
            foreach(string module in moduleTemplates.Keys)
                ModuleTemplates.Add(module, moduleTemplates[module]);
        }

        public void InitializeDevices() {
            foreach(string device in Devices.Keys)
                Devices[device].Initialize(ModuleTemplates);
        }

        public void Simulate() {
            Assignments.Sort();

            Dictionary<double, List<Assignment>> timedAssignments = new Dictionary<double, List<Assignment>>();
            for(int i = 0; i < Assignments.Count; i++) {
                if(!timedAssignments.ContainsKey(Assignments[i].Time)) {
                    timedAssignments.Add(Assignments[i].Time, new List<Assignment>());
                    timedAssignments[Assignments[i].Time].Add(Assignments[i]);
                } else
                    timedAssignments[Assignments[i].Time].Add(Assignments[i]);
            }

            foreach(double t in timedAssignments.Keys) {
                for(int j = 0; j < timedAssignments[t].Count; j++) {
                    Devices[(timedAssignments[t])[j].DeviceName].BaseModuleCopy.Inputs[(timedAssignments[t])[j].InputId] = (timedAssignments[t])[j].Value;
                    Devices[(timedAssignments[t])[j].DeviceName].BaseModuleCopy.Update();
                }

                foreach(string output in Outputs.Keys) {
                    if(Devices[Outputs[output].DeviceName].BaseModuleCopy.Outputs.ContainsKey(Outputs[output].InputId))
                        Console.WriteLine(
                            "At t = " + t + " seconds, "
                        + output + " = " + Devices[Outputs[output].DeviceName].BaseModuleCopy.Outputs[Outputs[output].InputId]);
                    else if(Devices[Outputs[output].DeviceName].BaseModuleCopy.Inputs.ContainsKey(Outputs[output].InputId))
                        Console.WriteLine(
                            "At t = " + t + " seconds, "
                        + output + " = " + Devices[Outputs[output].DeviceName].BaseModuleCopy.Inputs[Outputs[output].InputId]);
                    else
                        throw new Exception("Output " + Outputs[output].InputId + " does not exist");
                }
                
                Console.WriteLine();
            }
        }

        public List<PlotData> CreatePlot() {
            // Basically Simulate, but store data in "plot datas" instead of stuff
            Dictionary<string, List<double>> plotValues = new Dictionary<string, List<double>>();
            Dictionary<string, List<double>> plotTimes = new Dictionary<string, List<double>>();

            Assignments.Sort();

            Dictionary<double, List<Assignment>> timedAssignments = new Dictionary<double, List<Assignment>>();
            for(int i = 0; i < Assignments.Count; i++) {
                if(!timedAssignments.ContainsKey(Assignments[i].Time)) {
                    timedAssignments.Add(Assignments[i].Time, new List<Assignment>());
                    timedAssignments[Assignments[i].Time].Add(Assignments[i]);
                } else
                    timedAssignments[Assignments[i].Time].Add(Assignments[i]);
            }

            foreach(double t in timedAssignments.Keys) {
                for(int j = 0; j < timedAssignments[t].Count; j++) {
                    Devices[(timedAssignments[t])[j].DeviceName].BaseModuleCopy.Inputs[(timedAssignments[t])[j].InputId] = (timedAssignments[t])[j].Value;
                    Devices[(timedAssignments[t])[j].DeviceName].BaseModuleCopy.Update();
                }

                foreach(string output in Outputs.Keys) {
                    if(Devices[Outputs[output].DeviceName].BaseModuleCopy.Outputs.ContainsKey(Outputs[output].InputId)) {
                        /*Console.WriteLine(
                            "At t = " + t + " seconds, "
                        + output + " = " + Devices[Outputs[output].DeviceName].BaseModuleCopy.Outputs[Outputs[output].InputId]);*/
                        if(!plotTimes.ContainsKey(output)) {
                            plotTimes.Add(output, new List<double>());
                            plotValues.Add(output, new List<double>());

                            plotTimes[output].Add(0);
                            plotValues[output].Add(0);
                        } else {
                            plotTimes[output].Add(t);
                            plotValues[output].Add(plotValues[output][plotValues[output].Count - 1]);
                        }

                        plotTimes[output].Add(t);
                        plotValues[output].Add(
                            Devices[Outputs[output].DeviceName].BaseModuleCopy.Outputs[Outputs[output].InputId] ? 5 : 0
                        );
                    } else if(Devices[Outputs[output].DeviceName].BaseModuleCopy.Inputs.ContainsKey(Outputs[output].InputId)) {
                        /*Console.WriteLine(
                            "At t = " + t + " seconds, "
                        + output + " = " + Devices[Outputs[output].DeviceName].BaseModuleCopy.Inputs[Outputs[output].InputId]);*/
                        if(!plotTimes.ContainsKey(output)) {
                            plotTimes.Add(output, new List<double>());
                            plotValues.Add(output, new List<double>());

                            plotTimes[output].Add(0);
                            plotValues[output].Add(0);
                        } else {
                            plotTimes[output].Add(t);
                            plotValues[output].Add(plotValues[output][plotValues[output].Count - 1]);
                        }

                        plotTimes[output].Add(t);
                        plotValues[output].Add(
                            Devices[Outputs[output].DeviceName].BaseModuleCopy.Inputs[Outputs[output].InputId] ? 5 : 0
                        );
                    } else
                        throw new Exception("Output " + Outputs[output].InputId + " does not exist");
                }
                
                Console.WriteLine();
            }

            List<PlotData> data = new List<PlotData>();

            foreach(string name in plotTimes.Keys)
                data.Add(new PlotData(name, plotTimes[name], plotValues[name]));

            return data;
        }
    }

    class Module {
        public string Name;
        public Dictionary<string, bool> Inputs;
        public Dictionary<string, bool> Outputs;

        public Wire[] Wires;
        public Reg[] Registers;

        public Module(string name, Dictionary<string, bool> inputs, Dictionary<string, bool> outputs, Wire[] wires, Reg[] registers) {
            Name = name;
            Inputs = inputs;
            Outputs = outputs;
            Wires = wires;
            Registers = registers;
        }

        public void Update() {
            // Registers
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

            for(int i = 0; i < Registers.Length; i++) {
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

                Registers[i].Update();

                foreach(string output in Outputs.Keys.ToList()) {
                    if(Registers[i].Name == output)
                        Outputs[output] = Registers[i].Output();
                }
            }

            // Wires
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

            for(int i = 0; i < Wires.Length; i++) {
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

                foreach(string output in Outputs.Keys.ToList()) {
                    if(Wires[i].Name == output)
                        Outputs[output] = Wires[i].Output();
                }
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
        public bool RisingEdge, ActiveLow;
        public bool Default;
        public string ResetName, ClockName;

        public Reg(string name, string clockName, string resetName, bool risingEdge, bool activeLow, bool def, Func<Dictionary<string, bool>, bool>[] driven, Dictionary<string, bool> drivers) {
            Name = name;
            ClockName = clockName;
            ResetName = resetName;
            Driven = driven;
            RisingEdge = risingEdge;
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
            else if((RisingEdge && Drivers[ClockName]) || (!RisingEdge && Drivers[ClockName])) {
                for(int i = 0; i < Driven.Length; i++)
                    Drivers[Name] = Driven[i](Drivers);
            }
        }
    }
}