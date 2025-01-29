using System;
using System.Collections.Generic;
using System.Linq;

namespace Astra.Compilation
{
    public static class Tokenizer
    {
        public static Dictionary<string, Type> tokenTypeBySingleWord = new Dictionary<string, Type>()
        {
            { "if", typeof(Token_If) },
            { "else", typeof(Token_Else) },
            { "while", typeof(Token_While) },
            { "for", typeof(Token_For) },
            { "fn", typeof(Token_Fn) },
            { "return", typeof(Token_Return) },
            { "class", typeof(Token_Class) },
            { "new", typeof(Token_New) },
        };
        public static Dictionary<string, Type> tokenTypeBySingleChar = new Dictionary<string, Type>()
        {
            { "(", typeof(Token_BracketOpen) },
            { ")", typeof(Token_BracketClose) },
            { "=", typeof(Token_Assign) },
            { "{", typeof(Token_BlockOpen) },
            { "}", typeof(Token_BlockClose) },
            { ";", typeof(Token_Terminator) },
            { ":", typeof(Token_Colon) },
            { ",", typeof(Token_Comma) },
            { ".", typeof(Token_Dot) },
            { "[", typeof(Token_SquareBracketOpen) },
            { "]", typeof(Token_SquareBracketClose) },
        };

        public static List<Token> Tokenize(string rawCode)
        {
            List<Token> tokens = new List<Token>();

            bool isCollecting_Constant = false;

            int beginIndex = 0;
            int currentIndex = 0;
            string word = "";

            Action<Token> appendToken = (Token token) =>
            {
                token.beginIndex = beginIndex;
                token.endIndex = beginIndex + word.Length;

                beginIndex = token.endIndex;

                tokens.Add(token);
                word = "";
            };


            while (currentIndex < rawCode.Length)
            {
                char currentChar = rawCode[currentIndex];
                currentIndex++;

                // Skip tabs
                if (currentChar == '\t')
                {
                    beginIndex++;
                    continue;
                }


                // Start tokenize constant value (int, float, double)
                if (char.IsDigit(currentChar) && word == "")
                {
                    isCollecting_Constant = true;
                }
                if (char.IsDigit(currentChar) && isCollecting_Constant)
                {
                    word += currentChar;
                    continue;
                }
                else
                {
                    if (isCollecting_Constant)
                    {
                        appendToken(new Token_Constant()
                        {
                            value = word
                        });
                    }
                    isCollecting_Constant = false;
                }


                // On word end reach (new line or space)
                bool isTerminatorReached = currentChar == '\r' || currentChar == '\n' || currentChar == ' ';

                // If current char is ':', '(' or another single char, then it means, that we reached word end
                bool isSingleCharReached = TryTokenizeSingleChar(currentChar.ToString(), out Token singleCharToken);


                if (isTerminatorReached == false && isSingleCharReached == false)
                {
                    word += currentChar;
                }

                if (isTerminatorReached || isSingleCharReached)
                {
                    if (word.Length > 0)
                    {
                        if (Token_Visibility.TryMatch(word, out var vis))
                        {
                            appendToken(vis);
                        }
                        else if (Token_Identifier.IsMatch(word))
                        {
                            appendToken(new Token_Identifier()
                            {
                                name = word
                            });
                        }
                        else
                        {
                            Token wholeWordToken = TryTokenize(word, true);
                            if (wholeWordToken != null)
                            {
                                appendToken(wholeWordToken);
                                continue;
                            }
                        }
                    }

                    if (currentChar != ' ')
                    {
                        TerminateLine(tokens, appendToken);
                    }

                    if (isSingleCharReached)
                    {
                        word += currentChar;
                        appendToken(singleCharToken);
                    }

                    word = "";
                    beginIndex = currentIndex;
                }
            }

            return tokens;
        }

        private static Token TryTokenize(string word, bool isWholeWord)
        {
            if (Token_Equality.TryMatch(word, out var eq)) return eq;

            if (Token_AddSub.TryMatch(word, out var term)) return term;
            if (Token_Factor.TryMatch(word, out var fact)) return fact;
            if (Token_Unary.TryMatch(word, out var un)) return un;

            if (isWholeWord)
            {
                if (Token_Comprassion.TryMatch(word, out var cmp)) return cmp;
            }

            if (word == "=" && isWholeWord == false)
            {
                // '=' is not single token, '=' may be '=='
            }
            else
            {
                if (tokenTypeBySingleWord.TryGetValue(word, out Type tokenType))
                {
                    return (Token)Activator.CreateInstance(tokenType);
                }
            }



            return null;
        }

        private static bool TryTokenizeSingleChar(string word, out Token token)
        {
            token = null;
            if (word.Length > 1) return false;

            if (tokenTypeBySingleChar.TryGetValue(word, out Type tokenType))
            {
                token = (Token)Activator.CreateInstance(tokenType);
                return true;
            }

            return false;
        }

        private static void TerminateLine(List<Token> tokens, Action<Token> appendToken)
        {
            if (tokens.Last() is Token_Terminator == false)
            {
                appendToken(new Token_Terminator());
            }
        }
    }
}