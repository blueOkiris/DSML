/*
 * This is just the support classes for DigitalSystem
 * Moved them to another file for readability
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSML {
    struct Clock {
        public double Frequency, StartTime;
        public string DeviceName;
        public string InputId;
        public bool InitialValue;

        public Clock(double frequency, double startTime, string deviceName, string inputId, bool initialValue) {
            Frequency = frequency;
            StartTime = startTime;
            DeviceName = deviceName;
            InputId = inputId;
            InitialValue = initialValue;
        }

        public Assignment[] ToAssignments(double maxTime) {
            List<Assignment> assignments = new List<Assignment>();

            double i = StartTime;
            bool currentValue = InitialValue;
            while(i < maxTime) {
                assignments.Add(new Assignment(i, DeviceName, InputId, currentValue));

                currentValue = !currentValue;
                i += 1/(Frequency);
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
                    Console.WriteLine(
                        "At t = " + t + " seconds, "
                      + output + " = " + Devices[Outputs[output].DeviceName].BaseModuleCopy.Outputs[Outputs[output].InputId]);
                }
            }
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
            for(int i = 0; i < Wires.Length; i++) {
                foreach(Wire wire in Wires) {
                    if(!Wires[i].Drivers.ContainsKey(wire.Name))
                        Wires[i].Drivers.Add(wire.Name, false);
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

                foreach(string output in Outputs.Keys.ToList()) {
                    if(Wires[i].Name == output)
                        Outputs[output] = Wires[i].Output();
                }
            }
        }
    }

    abstract class IO { public string Name; public abstract bool Output(); }

    class Wire : IO {
        public Func<Dictionary<string, bool>, bool>[] Driven;
        public Dictionary<string, bool> Drivers;

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
        public Func<Dictionary<string, bool>, bool>[] Driven;
        public Dictionary<string, bool> Drivers;
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