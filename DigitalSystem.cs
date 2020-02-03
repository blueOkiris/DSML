using System;
using System.Collections.Generic;
using System.Linq;

namespace DSML {
    class DigitalSystem {
        public Dictionary<string, Module> Modules;
        public Dictionary<string, Simulation> Simulations;

        public DigitalSystem() {
            Modules = new Dictionary<string, Module>();
            Simulations = new Dictionary<string, Simulation>();
        }

        private Func<Dictionary<string, bool>, bool> BuildOr(Token token) {
            string a = "", b = "";

            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.ATTR) {
                    if(subToken.SubTokens[0].Type != TokenType.IDENT || subToken.SubTokens[1].Type != TokenType.STR)
                        throw new Exception("Malformed attribute. System error. Sorry :(");
                    else if(subToken.SubTokens[0].Value == "a")
                        a = subToken.SubTokens[1].Value;
                    else if(subToken.SubTokens[0].Value == "b")
                        b = subToken.SubTokens[1].Value;
                    else
                        throw new Exception("Unknown and attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in and!");
            }

            return
                delegate (Dictionary<string, bool> inputs) {
                    return inputs[a] || inputs[b];
                };
        }

        private Func<Dictionary<string, bool>, bool> BuildAnd(Token token) {
            string a = "", b = "";

            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.ATTR) {
                    if(subToken.SubTokens[0].Type != TokenType.IDENT || subToken.SubTokens[1].Type != TokenType.STR)
                        throw new Exception("Malformed attribute. System error. Sorry :(");
                    else if(subToken.SubTokens[0].Value == "a")
                        a = subToken.SubTokens[1].Value;
                    else if(subToken.SubTokens[0].Value == "b")
                        b = subToken.SubTokens[1].Value;
                    else
                        throw new Exception("Unknown and attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in and!");
            }

            return
                delegate (Dictionary<string, bool> inputs) {
                    return inputs[a] && inputs[b];
                };
        }

        private Wire BuildWire(Token token) {
            string name = "";
            List<Func<Dictionary<string, bool>, bool>> driven = new List<Func<Dictionary<string, bool>, bool>>();
            Dictionary<string, bool> drivers = new Dictionary<string, bool>();

            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.ATTR) {
                    if(subToken.SubTokens[0].Type != TokenType.IDENT || subToken.SubTokens[1].Type != TokenType.STR)
                        throw new Exception("Malformed attribute. System error. Sorry :(");
                    else if(subToken.SubTokens[0].Value == "name")
                        name = subToken.SubTokens[1].Value;
                    else
                        throw new Exception("Unknown wire attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.TAG) {
                    if(subToken.Value == "and") {
                        driven.Add(BuildAnd(subToken));
                    } else if(subToken.Value == "or") {
                        driven.Add(BuildOr(subToken));
                    } else
                        throw new Exception("Unepected '" + subToken.Value + "' tag in wire");
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in wire!");
            }

            return new Wire(name, driven.ToArray(), drivers);
        }

        private Module BuildModule(Token token) {
            string name = "";
            Dictionary<string, bool> inputs = new Dictionary<string, bool>();
            Dictionary<string, bool> outputs = new Dictionary<string, bool>();
            List<Wire> wires = new List<Wire>();

            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.ATTR) {
                    if(subToken.SubTokens[0].Type != TokenType.IDENT || subToken.SubTokens[1].Type != TokenType.STR)
                        throw new Exception("Malformed attribute. System error. Sorry :(");
                    else if(subToken.SubTokens[0].Value == "name")
                        name = subToken.SubTokens[1].Value;
                    else if(subToken.SubTokens[0].Value == "inputs") {
                        string[] inputNames = subToken.SubTokens[1].Value.Split(',');
                        foreach(string inputName in inputNames)
                            inputs.Add(inputName, false);
                    } else if(subToken.SubTokens[0].Value == "outputs") {
                        string[] outputNames = subToken.SubTokens[1].Value.Split(',');
                        foreach(string outputName in outputNames)
                            outputs.Add(outputName, false);
                    } else
                        throw new Exception("Unknown module attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.TAG) {
                    if(subToken.Value == "wire") {
                        wires.Add(BuildWire(subToken));
                    } else
                        throw new Exception("Unepected '" + subToken.Value + "' tag in module");
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in module!");
            }

            return new Module(name, inputs, outputs, wires.ToArray());
        }

        private Device BuildDevice(Token token) {
            string name = "";
            string baseModuleName = "";
            Dictionary<string, bool> inputs = new Dictionary<string, bool>();

            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.ATTR) {
                    if(subToken.SubTokens[0].Type != TokenType.IDENT || subToken.SubTokens[1].Type != TokenType.STR)
                        throw new Exception("Malformed attribute. System error. Sorry :(");
                    else if(subToken.SubTokens[0].Value == "name")
                        name = subToken.SubTokens[1].Value;
                    else if(subToken.SubTokens[0].Value == "module")
                        baseModuleName = subToken.SubTokens[1].Value;
                    else if(subToken.SubTokens[0].Value == "initialize") {
                        string initials = subToken.SubTokens[1].Value;
                        string[] inputAssigns = initials.Split(',');
                        
                        foreach(string inputAssign in inputAssigns) {
                            string[] pieces = inputAssign.Split('=');

                            if(pieces[1] != "gnd" && pieces[1] != "vcc")
                                throw new Exception("Unknown initial value: " + pieces[1]);

                            inputs.Add(pieces[0], pieces[1] == "vcc");
                        }
                    } else
                        throw new Exception("Unknown device attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in device!");
            }

            return new Device(name, baseModuleName, inputs);
        }

        private Assignment BuildAssignment(Token token) {
            double time = 0;
            string deviceName = "", inputId = "";
            bool value = false;

            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.ATTR) {
                    if(subToken.SubTokens[0].Type != TokenType.IDENT || subToken.SubTokens[1].Type != TokenType.STR)
                        throw new Exception("Malformed attribute. System error. Sorry :(");
                    else if(subToken.SubTokens[0].Value == "time") {
                        string timeStr = subToken.SubTokens[1].Value;
                        
                        if(timeStr.EndsWith("ms"))
                            time = double.Parse(timeStr.Substring(0, timeStr.Length - 2)) * 1e-3;
                        else if(timeStr.EndsWith("s"))
                            time = double.Parse(timeStr.Substring(0, timeStr.Length - 2));
                        else if(timeStr.EndsWith("us"))
                            time = double.Parse(timeStr.Substring(0, timeStr.Length - 2)) * 1e-6;
                        else if(timeStr.EndsWith("ns"))
                            time = double.Parse(timeStr.Substring(0, timeStr.Length - 2)) * 1e-9;
                        else if(timeStr.EndsWith("ps"))
                            time = double.Parse(timeStr.Substring(0, timeStr.Length - 2)) * 1e-12;
                        else
                            throw new Exception("Unknown time value: " + timeStr);
                    } else if(subToken.SubTokens[0].Value == "device")
                        deviceName = subToken.SubTokens[1].Value;
                    else if(subToken.SubTokens[0].Value == "id")
                        inputId = subToken.SubTokens[1].Value;
                    else
                        throw new Exception("Unknown assignment attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.IDENT) {
                    if(subToken.Value != "gnd" && subToken.Value != "vcc")
                        throw new Exception("Unknown assignment value: " + subToken.Value);

                    value = subToken.Value == "vcc";
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in assignment!");
            }

            return new Assignment(time, deviceName, inputId, value);
        }

        private SimOutput BuildOutput(Token token) {
            string deviceName = "", inputId = "";

            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.ATTR) {
                    if(subToken.SubTokens[0].Type != TokenType.IDENT || subToken.SubTokens[1].Type != TokenType.STR)
                        throw new Exception("Malformed attribute. System error. Sorry :(");
                    else if(subToken.SubTokens[0].Value == "device")
                        deviceName = subToken.SubTokens[1].Value;
                    else if(subToken.SubTokens[0].Value == "id")
                        inputId = subToken.SubTokens[1].Value;
                    else
                        throw new Exception("Unknown output attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in output!");
            }

            return new SimOutput(deviceName, inputId);
        }

        private Simulation BuildSimulation(Token token) {
            string name = "";
            Dictionary<string, Device> devices = new Dictionary<string, Device>();
            List<Assignment> assignments = new List<Assignment>();
            Dictionary<string, SimOutput> outputs = new Dictionary<string, SimOutput>();

            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.ATTR) {
                    if(subToken.SubTokens[0].Type != TokenType.IDENT || subToken.SubTokens[1].Type != TokenType.STR)
                        throw new Exception("Malformed attribute. System error. Sorry :(");
                    else if(subToken.SubTokens[0].Value == "name")
                        name = subToken.SubTokens[1].Value;
                    else
                        throw new Exception("Unknown simulation attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.TAG) {
                    if(subToken.Value == "device") {
                        Device newDevice = BuildDevice(subToken);
                        devices.Add(newDevice.Name, newDevice);
                    } else if(subToken.Value == "assign")
                        assignments.Add(BuildAssignment(subToken));
                    else if(subToken.Value == "output") {
                        SimOutput output = BuildOutput(subToken);
                        outputs.Add(output.DeviceName + "." + output.InputId, output);
                    } else
                        throw new Exception("Unepected '" + subToken.Value + "' tag in simulation");
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in simulation!");
            }

            return new Simulation(name, devices, assignments, outputs);
        }

        public void Initialize(Token[] tokens) {
            foreach(Token token in tokens) {
                if(token.Type == TokenType.TAG) {
                    if(token.Value == "module") {
                        Module mod = BuildModule(token);
                        Modules.Add(mod.Name, mod);
                    }else if(token.Value == "simulation") {
                        Simulation sim = BuildSimulation(token);
                        Simulations.Add(sim.Name, sim);
                    } else
                        throw new Exception("Unknown top level tag: '" + token.Value + "'");
                } else
                    throw new Exception("Expected tag in top level design");
            }

            // Now that everything's set up, apply the modules to the simulations so they'll work
            foreach(string sim in Simulations.Keys) {
                Simulations[sim].SetModules(Modules);
                Simulations[sim].InitializeDevices();
            }
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
            BaseModuleCopy = new Module(BaseModuleName, new Dictionary<string, bool>(), new Dictionary<string, bool>(), new Wire[] {});
        }

        public void Initialize(Dictionary<string, Module> moduleTemplates) {
            BaseModuleCopy = new Module(
                                    moduleTemplates[BaseModuleName].Name,
                                    new Dictionary<string, bool>(), 
                                    new Dictionary<string, bool>(),
                                    new Wire[] {});

            foreach(string input in moduleTemplates[BaseModuleName].Inputs.Keys)
                BaseModuleCopy.Inputs.Add(input, moduleTemplates[BaseModuleName].Inputs[input]);
            foreach(string output in moduleTemplates[BaseModuleName].Outputs.Keys)
                BaseModuleCopy.Outputs.Add(output, moduleTemplates[BaseModuleName].Outputs[output]);

            List<Wire> wires = new List<Wire>();
            foreach(Wire wire in moduleTemplates[BaseModuleName].Wires)
                wires.Add(wire);
            
            BaseModuleCopy.Wires = wires.ToArray();

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

        public Module(string name, Dictionary<string, bool> inputs, Dictionary<string, bool> outputs, Wire[] wires) {
            Name = name;
            Inputs = inputs;
            Outputs = outputs;
            Wires = wires;
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
        
        public static bool Vcc() => true;
        public static bool Gnd() => false;

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
        private bool Current, Default;
        private bool Rising;

        public Reg(string name, bool def, bool rising) {
            Name = name;
            Current = def;
            Default = def;
            Rising = rising;
        }

        public override bool Output() => Current;

        public void Update(bool input, bool clk, bool reset) {
            if(reset)
                Current = Default;
            else if(Rising && clk)
                Current = input;
        }
    }
}