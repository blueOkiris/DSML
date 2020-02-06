using System;
using System.Collections.Generic;
using System.Text;

namespace DSML {
    enum TokenType {
        IDENT,
        STR,
        COMMENT,
        TAG,
        ATTR
    }

    struct Token {
        public string Value;
        public TokenType Type;
        public Token[] SubTokens;

        public Token(string value, TokenType type, Token[] subTokens) {
            Value = value;
            Type = type;
            SubTokens = subTokens;
        }

        public override string ToString() {
            StringBuilder tokStr = new StringBuilder();

            tokStr.Append(Type);
            tokStr.Append(" ");
            tokStr.Append(Value);
            
            if(SubTokens != null) {
                foreach(Token token in SubTokens) {
                    tokStr.Append("\n\t");
                    tokStr.Append(token.ToString().Replace("\n", "\n\t"));
                }
            }

            return tokStr.ToString();
        }
    }

    /*
     * Pseudo-ebnf:
     * <comment>    := '<!--' .* '-->'
     * <ident>      := [A-Za-z]?[A-Za-z0-9_]*
     * <string>     := '"' .* '"'
     * <attribute>  := <ident> '=' <string>
     * <tag>        := '<' <ident> <atrribute>* '>' <ident> '</' <ident> '>'
     */
    class Parser {
        private static void ParseWhitespace(string input, ref int index) {
            while(index < input.Length && char.IsWhiteSpace(input[index]))
                index++;
        }

        private static Token ParseIdentifier(string input, ref int index) {
            StringBuilder identStr = new StringBuilder();

            while(char.IsLetterOrDigit(input[index]) || input[index] == '_' || input[index] == '-') {
                identStr.Append(input[index]);

                index++;
                if(index >= input.Length)
                    throw new Exception("Reached EOF while parsing identifier. Current str: " + identStr.ToString());
            }

            index--;
            return new Token(identStr.ToString(), TokenType.IDENT, null);
        }

        private static Token ParseString(string input, ref int index) {
            StringBuilder str = new StringBuilder();

            index++;
            if(index >= input.Length)
                throw new Exception("Unterminated string. Current str: ");

            while(input[index] != '"') {
                if(input[index] == '\\' && (index + 1 >= input.Length))
                    throw new Exception("Unterminated string. Current str: " + str.ToString());
                else if(input[index] == '\\') {
                    switch(input[index + 1]) {
                        case '"':
                            index++;
                            str.Append('\"');
                            break;

                        case '\\':
                            index++;
                            str.Append('\\');
                            break;

                        case 'r':
                            index++;
                            str.Append('\r');
                            break;

                        case 'n':
                            index++;
                            str.Append('\n');
                            break;

                        case 't':
                            index++;
                            str.Append('\t');
                            break;

                        default:
                            throw new Exception("Unknown escape sequence: \\" + input[index + 1]);
                    }
                } else
                    str.Append(input[index]);

                index++;
                if(index >= input.Length)
                    throw new Exception("Unterminated string. Current str: " + str.ToString());
            }

            return new Token(str.ToString(), TokenType.STR, null);
        }

        private static Token ParseAttribute(string input, ref int index) {
            List<Token> subTokens = new List<Token>();

            subTokens.Add(ParseIdentifier(input, ref index));
            index++;

            // Skip '='
            ParseWhitespace(input, ref index);
            if(index >= input.Length || input[index] != '=')
                throw new Exception("Expected '=' after identifier " + subTokens[0].Value);
            index++;

            ParseWhitespace(input, ref index);
            subTokens.Add(ParseString(input, ref index));
            index++;

            return new Token("attr", TokenType.ATTR, subTokens.ToArray());
        }

        private static Token ParseTag(string input, ref int index) {
            // Skip past '<'
            index++;
            string value = ParseIdentifier(input, ref index).Value;
            index++;

            List<Token> subTokens = new List<Token>();
            while(input[index] != '>') {
                ParseWhitespace(input, ref index);
                subTokens.Add(ParseAttribute(input, ref index));
            }
            index++;

            ParseWhitespace(input, ref index);
            
            if(index >= input.Length)
                throw new Exception("Expected tag closing");
            
            while(!input.Substring(index).StartsWith("</")) {
                if(input.Substring(index).StartsWith("<!--")) {
                    // Comment
                    StringBuilder commentStr = new StringBuilder();
                    index += 4;

                    while(!input.Substring(index).StartsWith("-->")) {
                        commentStr.Append(input[index]);
                        
                        index++;
                        if(index + 2 >= input.Length)
                            throw new Exception("Missing comment closing");
                    }

                    subTokens.Add(new Token(commentStr.ToString(), TokenType.COMMENT, null));
                    index += 3;
                    ParseWhitespace(input, ref index);
                } else if(input[index] != '<') {
                    if(input[index] == '"')
                        subTokens.Add(ParseString(input, ref index));
                    else
                        subTokens.Add(ParseIdentifier(input, ref index));
                    index++;
                } else if(index + 1 >= input.Length)
                    throw new Exception("Expected tag closing");
                else if(input[index + 1] != '/') {
                    subTokens.Add(ParseTag(input, ref index));
                    index++;
                }

                ParseWhitespace(input, ref index);
            }

            ParseWhitespace(input, ref index);
            index += 2;

            ParseWhitespace(input, ref index);
            string closingTag = ParseIdentifier(input, ref index).Value;
            if(closingTag != value)
                throw new Exception("Missing closing tag: " + value);
            index++;
            ParseWhitespace(input, ref index);

            if(index >= input.Length || input[index] != '>')
                throw new Exception("Expected '>' in tag closing");

            return new Token(value, TokenType.TAG, subTokens.ToArray());
        }

        public static Token[] ParseText(string input) {
            List<Token> tokens = new List<Token>();

            for(int i = 0; i < input.Length; i++) {
                if(char.IsWhiteSpace(input[i]))
                    continue;
                    
                if(input.Substring(i).StartsWith("<!--")) {
                    // Comment
                    StringBuilder commentStr = new StringBuilder();
                    i += 4;

                    while(!input.Substring(i).StartsWith("-->")) {
                        commentStr.Append(input[i]);
                        
                        i++;
                        if(i + 2 >= input.Length)
                            throw new Exception("Missing comment closing");
                    }

                    tokens.Add(new Token(commentStr.ToString(), TokenType.COMMENT, null));
                    i += 2;
                } else if(input[i] == '<')
                    tokens.Add(ParseTag(input, ref i));
                else if(char.IsLetter(input[i]))
                    tokens.Add(ParseIdentifier(input, ref i));
                else if(input[i] == '"')
                    tokens.Add(ParseString(input, ref i));
            }

            return tokens.ToArray();
        }
    }
}