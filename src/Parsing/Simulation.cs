/*
 * This is just the support classes for DigitalSystem
 * Moved them to another file for readability
 *
 * Specifically holds simulation stuff. The module stuff has been moved to Module.cs
 */
using System;
using System.Collections.Generic;

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
                Assignment newAssign = new Assignment(i, DeviceName, InputId, currentValue);
                newAssign.IsClock = true;
                assignments.Add(newAssign);

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
        public bool IsClock;

        public Assignment(double time, string deviceName, string inputId, bool value) {
            Time = time;
            InputId = inputId;
            Value = value;
            DeviceName = deviceName;
            IsClock = false;
        }

        public int CompareTo(object other) {
            int timeComparison = Time.CompareTo(((Assignment) other).Time);

            // Sort clock created assignments first!
            return 
                timeComparison == 0 ?
                    ((IsClock && !((Assignment) other).IsClock) ?       // Is clock and other isn't
                        -1 :
                        ((!IsClock && ((Assignment) other).IsClock) ?    // Other is and this one isn't
                            1 :
                            timeComparison)) :
                    timeComparison;
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
            BaseModuleCopy = new Module(BaseModuleName,
                                    new Dictionary<string, bool>(),
                                    new Dictionary<string, bool>(), 
                                    new Wire[] {}, 
                                    new Reg[] {}, 
                                    new ModuleDevice[] {});
        }

        public void Initialize(Dictionary<string, Module> moduleTemplates) {
            BaseModuleCopy = new Module(
                                    moduleTemplates[BaseModuleName].Name,
                                    new Dictionary<string, bool>(), 
                                    new Dictionary<string, bool>(),
                                    new Wire[] {},
                                    new Reg[] {},
                                    new ModuleDevice[] {});

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

            List<ModuleDevice> devices = new List<ModuleDevice>();
            foreach(ModuleDevice device in moduleTemplates[BaseModuleName].Devices)
                devices.Add(device);
            
            BaseModuleCopy.Wires = wires.ToArray();
            BaseModuleCopy.Registers = regs.ToArray();
            BaseModuleCopy.Devices = devices.ToArray();

            foreach(string input in InitialInputs.Keys)
                BaseModuleCopy.Inputs[input] = InitialInputs[input];
            
            BaseModuleCopy.InitializeDevices(moduleTemplates);
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

                foreach(string device in Devices.Keys)
                    Devices[device].BaseModuleCopy.Update();

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

                foreach(string device in Devices.Keys)
                    Devices[device].BaseModuleCopy.Update();

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
                
                //Console.WriteLine();
            }

            List<PlotData> data = new List<PlotData>();

            foreach(string name in plotTimes.Keys)
                data.Add(new PlotData(name, plotTimes[name], plotValues[name]));

            return data;
        }
    }
}