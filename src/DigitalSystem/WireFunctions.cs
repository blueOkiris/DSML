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
    }
}