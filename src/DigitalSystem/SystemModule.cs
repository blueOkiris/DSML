using System;
using System.Collections.Generic;

namespace DSML {
    partial class DigitalSystem {
        private ModuleDevice BuildSubDevice(Token token) {
            string baseModuleName = "";
            Dictionary<string, string> parentToSelfInputs = new Dictionary<string, string>();
            Dictionary<string, string> selfToParentOutputs = new Dictionary<string, string>();

            foreach(Token subToken in token.SubTokens) {
                if(subToken.Type == TokenType.ATTR) {
                    if(subToken.SubTokens[0].Type != TokenType.IDENT || subToken.SubTokens[1].Type != TokenType.STR)
                        throw new Exception("Malformed attribute. System error. Sorry :(");
                    else if(subToken.SubTokens[0].Value == "module")
                        baseModuleName = subToken.SubTokens[1].Value;
                    else if(subToken.SubTokens[0].Value == "inputs") {                        
                        // Get the parent inputs
                        string initials = subToken.SubTokens[1].Value;
                        string[] inputAssigns = initials.Split(',');
                        
                        foreach(string inputAssign in inputAssigns) {
                            string[] pieces = inputAssign.Split('=');
                            parentToSelfInputs.Add(pieces[1], pieces[0]);
                        }
                    } else if(subToken.SubTokens[0].Value == "outputs") {                        
                        // Get the parent inputs
                        string initials = subToken.SubTokens[1].Value;
                        string[] inputAssigns = initials.Split(',');
                        
                        foreach(string inputAssign in inputAssigns) {
                            string[] pieces = inputAssign.Split('=');
                            selfToParentOutputs.Add(pieces[1], pieces[0]);
                        }
                    } else
                        throw new Exception("Unknown device attribute: " + subToken.SubTokens[0].Value);
                } else if(subToken.Type == TokenType.COMMENT)
                    continue;
                else
                    throw new Exception("Unknown token in device!");
            }

            return new ModuleDevice(baseModuleName, parentToSelfInputs, selfToParentOutputs);
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
    }
}