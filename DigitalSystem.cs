using System;
using System.Collections.Generic;

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
                       // TODO
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
                    } else if(subToken.Value == "clock") {
                        // TODO
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
}