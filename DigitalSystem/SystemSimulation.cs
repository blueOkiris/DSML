using System.Collections.Generic;
using System;


namespace DSML {
    partial class DigitalSystem {
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
    }
}