using Astra.Compilation;
using UnityEngine;
using TMPro;

namespace InGame
{
    public class IDE : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;

        public bool doCompile;

        private void OnValidate()
        {
            if (doCompile)
            {
                doCompile = false;

                // Debug.Log(Astra.Expr.Class1.Test());
                // Debug.Log(Astra.Compilation.Compiler.Compile_Astra_to_NASM(inputField.text));

                foreach (Token token in Tokenizer.Tokenize(inputField.text))
                {
                    if (token is Token_Terminator) continue;

                    string word = inputField.text.Substring(token.beginIndex, token.endIndex - token.beginIndex);
                    Debug.Log(token.GetType() + " at " + token.beginIndex + ":" + token.endIndex + " = '" + word + "'");

                    if (token is Token_Identifier iden)
                    {
                        Debug.Log("ident word = '" + iden.name + "'");
                    }
                }
            }
        }
    }
}