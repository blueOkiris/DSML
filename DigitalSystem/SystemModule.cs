using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DSML {
    partial class DigitalSystem {
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
    }
}