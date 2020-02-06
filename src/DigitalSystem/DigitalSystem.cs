using System;
using System.Collections.Generic;

namespace DSML {
    public class ExternalFuncGlobal {
        public Dictionary<string, bool> Inputs;

        public ExternalFuncGlobal(Dictionary<string, bool> inputs) {
            Inputs = new Dictionary<string, bool>();

            foreach(string input in inputs.Keys)
                Inputs.Add(input, inputs[input]);
        }
    }

    partial class DigitalSystem {
        public Dictionary<string, Module> Modules;
        public Dictionary<string, Simulation> Simulations;

        public DigitalSystem() {
            Modules = new Dictionary<string, Module>();
            Simulations = new Dictionary<string, Simulation>();
        }

        private Module BuildModule(Token token) {
            string name = "";
            Dictionary<string, bool> inputs = new Dictionary<string, bool>();
            Dictionary<string, bool> outputs = new Dictionary<string, bool>();
            List<Wire> wires = new List<Wire>();
            List<Reg> registers = new List<Reg>();
            List<ModuleDevice> devices = new List<ModuleDevice>();

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
                    } else if(subToken.Value == "device") {
                        devices.Add(BuildSubDevice(subToken));
                    } else
                        throw new Exception("Unepected '" + subToken.Value + "' tag in module");
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in module!");
            }

            return new Module(name, inputs, outputs, wires.ToArray(), registers.ToArray(), devices.ToArray());
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