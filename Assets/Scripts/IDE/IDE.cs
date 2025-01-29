using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Astra.Compilation;
using UnityEngine;
using TMPro;
using Debug = UnityEngine.Debug;

namespace InGame
{
    public class IDE : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_InputField coloredField;
        [SerializeField] private TMP_InputField nasmField;

        public bool doCompile;

        [SerializeField] private TextAsset colorSchemeJson;

        private ColorScheme colorScheme;

        private HashSet<Type> punctuationTypes = new HashSet<Type>()
        {
            typeof(Token_Comma),
            typeof(Token_Colon),
            typeof(Token_BracketOpen),
            typeof(Token_BracketClose),
            typeof(Token_SquareBracketOpen),
            typeof(Token_SquareBracketClose),
            typeof(Token_BlockOpen),
            typeof(Token_BlockClose),
        };

        private void OnValidate()
        {
            if (doCompile)
            {
                doCompile = false;

                Tokenize();
                Compile();
            }
        }

        private void Tokenize()
        {
            Stopwatch w = Stopwatch.StartNew();
            List<Token> tokens = Tokenizer.Tokenize(inputField.text);
            Debug.Log($"Tokenized in {w.ElapsedMilliseconds} ms");

            ReadColorScheme();

            string colored = inputField.text;

            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                Token token = tokens[i];
                // if (token is Token_Terminator) continue;

                string word = inputField.text.Substring(token.beginIndex, token.endIndex - token.beginIndex);

                string colorHex = GetColor(token);

                colored = colored.Remove(token.beginIndex, word.Length);
                colored = colored.Insert(token.beginIndex, $"<color={colorHex}>{word}</color>");

                Debug.Log(token.GetType() + " '" + word + "'");
            }

            coloredField.text = colored;
        }

        private void Compile()
        {
            string nasm = Compiler.Compile_Astra_to_NASM(inputField.text);
            nasmField.text = nasm;
        }

        private string GetColor(Token token)
        {
            string key = GetKey(token);
            if (key == null) return "#f22";

            foreach (ColorCase colorCase in colorScheme.cases)
            {
                if (colorCase.types.Contains(key))
                {
                    return colorCase.color;
                }
            }

            return "#0f0";
        }

        private string GetKey(Token token)
        {
            if (token is Token_Return) return "controlFlowKeyword";
            if (token is Token_Visibility || token is Token_Class) return "keyword";
            if (token is Token_Operator) return "operator";
            if (token is Token_Constant) return "number";
            if (punctuationTypes.Contains(token.GetType())) return "punctuation";


            if (token is Token_Identifier) return "variableName";

            return null;
        }

        private void ReadColorScheme()
        {
            colorScheme = JsonUtility.FromJson<ColorScheme>(colorSchemeJson.text);
        }
    }

    public class ColorScheme
    {
        public ColorCase[] cases;
    }

    [System.Serializable]
    public class ColorCase
    {
        public string color;
        public string[] types;
    }
}