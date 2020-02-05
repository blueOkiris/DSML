using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DSML {
    public class ExternalFuncGlobal {
        public Dictionary<string, bool> Inputs;

        public ExternalFuncGlobal(Dictionary<string, bool> inputs) {
            Inputs = new Dictionary<string, bool>();

            foreach(string input in inputs.Keys)
                Inputs.Add(input, inputs[input]);
        }
    }

    class DigitalSystem {
        public Dictionary<string, Module> Modules;
        public Dictionary<string, Simulation> Simulations;

        public DigitalSystem() {
            Modules = new Dictionary<string, Module>();
            Simulations = new Dictionary<string, Simulation>();
        }

        private Func<Dictionary<string, bool>, bool> BuildXor(Token token) {
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
                        throw new Exception("Unknown xor attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in xor!");
            }

            return
                delegate (Dictionary<string, bool> inputs) {
                    return inputs[a] ^ inputs[b];
                };
        }

        private Func<Dictionary<string, bool>, bool> BuildNot(Token token) {
            string a = "";

            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.ATTR) {
                    if(subToken.SubTokens[0].Type != TokenType.IDENT || subToken.SubTokens[1].Type != TokenType.STR)
                        throw new Exception("Malformed attribute. System error. Sorry :(");
                    else if(subToken.SubTokens[0].Value == "a")
                        a = subToken.SubTokens[1].Value;
                    else
                        throw new Exception("Unknown not attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in not!");
            }

            return
                delegate (Dictionary<string, bool> inputs) {
                    return !inputs[a];
                };
        }

        private Func<Dictionary<string, bool>, bool> BuildNand(Token token) {
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
                        throw new Exception("Unknown nand attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in nand!");
            }

            return
                delegate (Dictionary<string, bool> inputs) {
                    return !(inputs[a] && inputs[b]);
                };
        }

        private Func<Dictionary<string, bool>, bool> BuildNor(Token token) {
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
                        throw new Exception("Unknown nor attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in nor!");
            }

            return
                delegate (Dictionary<string, bool> inputs) {
                    return !(inputs[a] || inputs[b]);
                };
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
                        throw new Exception("Unknown or attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in or!");
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

        private Func<Dictionary<string, bool>, bool> BuildFuncFromFile(Token token) {
            string fileName = "";
            
            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.ATTR) {
                    if(subToken.SubTokens[0].Type != TokenType.IDENT || subToken.SubTokens[1].Type != TokenType.STR)
                        throw new Exception("Malformed attribute. System error. Sorry :(");
                    else if(subToken.SubTokens[0].Value == "src")
                        fileName = subToken.SubTokens[1].Value;
                    else
                        throw new Exception("Unknown and attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in file!");
            }

            return 
                delegate (Dictionary<string, bool> inputs) {
                    string funcStr = File.ReadAllText(fileName);

                    Script<bool> script = CSharpScript.Create<bool>(
                                                    funcStr, 
                                                    ScriptOptions.Default.WithImports("System.Collections.Generic"), 
                                                    typeof(ExternalFuncGlobal));
                    script.Compile();

                    ExternalFuncGlobal globals = new ExternalFuncGlobal(inputs);
                    bool result = script.RunAsync(globals).Result.ReturnValue;

                    return result;
                };
        }

        private Func<Dictionary<string, bool>, bool> BuildFuncFromStr(Token token) {
            string codeStr = "";

            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.COMMENT)
                    continue;
                else if(subToken.Type == TokenType.STR) {
                    codeStr = subToken.Value;
                } else
                    throw new Exception("Unknown token in file!");
            }

            return
                delegate (Dictionary<string, bool> inputs) {
                    Script<bool> script = CSharpScript.Create<bool>(
                                                    codeStr,
                                                    ScriptOptions.Default.WithImports("System.Collections.Generic"), 
                                                    typeof(ExternalFuncGlobal));
                    script.Compile();

                    ExternalFuncGlobal globals = new ExternalFuncGlobal(inputs);
                    bool result = script.RunAsync(globals).Result.ReturnValue;

                    return result;
                };
        }

        private Reg BuildRegister(Token token) {
            string name = "", clockName = "", resetName = "";
            bool positiveLevel = false, activeLow = false, def = false;
            List<Func<Dictionary<string, bool>, bool>> driven = new List<Func<Dictionary<string, bool>, bool>>();
            Dictionary<string, bool> drivers = new Dictionary<string, bool>();

            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.ATTR) {
                    if(subToken.SubTokens[0].Type != TokenType.IDENT || subToken.SubTokens[1].Type != TokenType.STR)
                        throw new Exception("Malformed attribute. System error. Sorry :(");
                    else if(subToken.SubTokens[0].Value == "name")
                        name = subToken.SubTokens[1].Value;
                    else if(subToken.SubTokens[0].Value == "clock")
                        clockName = subToken.SubTokens[1].Value;
                    else if(subToken.SubTokens[0].Value == "reset")
                        resetName = subToken.SubTokens[1].Value;
                    else if(subToken.SubTokens[0].Value == "positive-level") {
                        if(subToken.SubTokens[1].Value != "true" && subToken.SubTokens[1].Value != "false")
                            throw new Exception("Expected true or false for rising");
                        positiveLevel = subToken.SubTokens[1].Value == "true";
                    } else if(subToken.SubTokens[0].Value == "active-low") {
                        if(subToken.SubTokens[1].Value != "true" && subToken.SubTokens[1].Value != "false")
                            throw new Exception("Expected true or false for activeLow");
                        activeLow = subToken.SubTokens[1].Value == "true";
                    } else if(subToken.SubTokens[0].Value == "default") {
                        if(subToken.SubTokens[1].Value != "vcc" && subToken.SubTokens[1].Value != "gnd")
                            throw new Exception("Expected vcc or gnd for default");
                        def = subToken.SubTokens[1].Value == "vcc";
                    } else
                        throw new Exception("Unknown reg attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.TAG) {
                    if(subToken.Value == "and") {
                        driven.Add(BuildAnd(subToken));
                    } else if(subToken.Value == "or") {
                        driven.Add(BuildOr(subToken));
                    } else if(subToken.Value == "nand") {
                        driven.Add(BuildNand(subToken));
                    } else if(subToken.Value == "nor") {
                        driven.Add(BuildNor(subToken));
                    } else if(subToken.Value == "xor") {
                        driven.Add(BuildXor(subToken));
                    } else if(subToken.Value == "not") {
                        driven.Add(BuildNot(subToken));
                    } else if(subToken.Value == "file") {
                        driven.Add(BuildFuncFromFile(subToken));
                    } else if(subToken.Value == "code") {
                        driven.Add(BuildFuncFromStr(subToken));
                    } else
                        throw new Exception("Unepected '" + subToken.Value + "' tag in wire");
                } else if(subToken.Type == TokenType.IDENT) {
                    driven.Add(
                        delegate (Dictionary<string, bool> inputs) {
                            return inputs[subToken.Value];
                        }
                    );
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in reg!");
            }

            return new Reg(name, clockName, resetName, positiveLevel, activeLow, def, driven.ToArray(), drivers);
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
                    } else if(subToken.Value == "nand") {
                        driven.Add(BuildNand(subToken));
                    } else if(subToken.Value == "nor") {
                        driven.Add(BuildNor(subToken));
                    } else if(subToken.Value == "xor") {
                        driven.Add(BuildXor(subToken));
                    } else if(subToken.Value == "not") {
                        driven.Add(BuildNot(subToken));
                    } else if(subToken.Value == "file") {
                        driven.Add(BuildFuncFromFile(subToken));
                    } else if(subToken.Value == "code") {
                        driven.Add(BuildFuncFromStr(subToken));
                    } else
                        throw new Exception("Unepected '" + subToken.Value + "' tag in wire");
                } else if(subToken.Type == TokenType.IDENT) {
                    driven.Add(
                        delegate (Dictionary<string, bool> inputs) {
                            return inputs[subToken.Value];
                        }
                    );
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
            List<Reg> registers = new List<Reg>();

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
                    } else if(subToken.Value == "reg") {
                        registers.Add(BuildRegister(subToken));
                    } else
                        throw new Exception("Unepected '" + subToken.Value + "' tag in module");
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in module!");
            }

            return new Module(name, inputs, outputs, wires.ToArray(), registers.ToArray());
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

        private Clock BuildClock(Token token) {
            double startTime = 0, frequency = 0;
            string deviceName = "", inputId = "";
            bool initialValue = false;

            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.ATTR) {
                    if(subToken.SubTokens[0].Type != TokenType.IDENT || subToken.SubTokens[1].Type != TokenType.STR)
                        throw new Exception("Malformed attribute. System error. Sorry :(");
                    else if(subToken.SubTokens[0].Value == "start") {
                        string timeStr = subToken.SubTokens[1].Value;
                        
                        if(timeStr.EndsWith("ms"))
                            startTime = double.Parse(timeStr.Substring(0, timeStr.Length - 2)) * 1e-3;
                        else if(timeStr.EndsWith("s"))
                            startTime = double.Parse(timeStr.Substring(0, timeStr.Length - 2));
                        else if(timeStr.EndsWith("us"))
                            startTime = double.Parse(timeStr.Substring(0, timeStr.Length - 2)) * 1e-6;
                        else if(timeStr.EndsWith("ns"))
                            startTime = double.Parse(timeStr.Substring(0, timeStr.Length - 2)) * 1e-9;
                        else if(timeStr.EndsWith("ps"))
                            startTime = double.Parse(timeStr.Substring(0, timeStr.Length - 2)) * 1e-12;
                        else
                            throw new Exception("Unknown time value: " + timeStr);
                    } else if(subToken.SubTokens[0].Value == "frequency") {
                        string freqStr = subToken.SubTokens[1].Value;
                        
                        if(freqStr.EndsWith("Hz"))
                            frequency = double.Parse(freqStr.Substring(0, freqStr.Length - 2));
                        else if(freqStr.EndsWith("MHz"))
                            frequency = double.Parse(freqStr.Substring(0, freqStr.Length - 2)) * 1e6;
                        else if(freqStr.EndsWith("GHz"))
                            frequency = double.Parse(freqStr.Substring(0, freqStr.Length - 2)) * 1e9;
                        else
                            throw new Exception("Unknown frequency value: " + freqStr);
                    } else if(subToken.SubTokens[0].Value == "device")
                        deviceName = subToken.SubTokens[1].Value;
                    else if(subToken.SubTokens[0].Value == "id")
                        inputId = subToken.SubTokens[1].Value;
                    else
                        throw new Exception("Unknown assignment attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.IDENT) {
                    if(subToken.Value != "gnd" && subToken.Value != "vcc")
                        throw new Exception("Unknown assignment value: " + subToken.Value);

                    initialValue = subToken.Value == "vcc";
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in assignment!");
            }

            return new Clock(frequency, startTime, deviceName, inputId, initialValue);
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
            List<Clock> clocks = new List<Clock>();
            Dictionary<string, SimOutput> outputs = new Dictionary<string, SimOutput>();
            double maxTime = double.MinValue;

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
                    } else if(subToken.Value == "assign") {
                        Assignment newAssignment = BuildAssignment(subToken);
                        if(newAssignment.Time > maxTime)
                            maxTime = newAssignment.Time;
                        assignments.Add(newAssignment);
                    } else if(subToken.Value == "output") {
                        SimOutput output = BuildOutput(subToken);
                        outputs.Add(output.DeviceName + "." + output.InputId, output);
                    } else if(subToken.Value == "clock")
                        clocks.Add(BuildClock(subToken));
                    else
                        throw new Exception("Unepected '" + subToken.Value + "' tag in simulation");
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in simulation!");
            }

            Simulation newSim = new Simulation(name, devices, assignments, outputs);
            foreach(Clock clock in clocks) {
                Assignment[] clockAssigns = clock.ToAssignments(maxTime * 1.1);
                
                foreach(Assignment assignment in clockAssigns)
                    newSim.Assignments.Add(assignment);
            }

            return newSim;
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
}