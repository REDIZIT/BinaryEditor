using System.Text;
using TMPro;
using UnityEngine;

namespace InGame
{
    public class RowController : MonoBehaviour
    {
        [SerializeField] private PixelText text;
        [SerializeField] private bool isCharMode;

        private byte[] temp = new byte[1];

        public void Refresh(byte b)
        {
            if (isCharMode)
            {
                if (b == 0)
                {
                    text.text = "";
                }
                else if (b <= 31 || b >= 127)
                {
                    text.text = ".";
                }
                else
                {
                    temp[0] = b;
                    char c = Encoding.ASCII.GetChars(temp)[0];
                    text.text = c.ToString();
                }
            }
            else
            {
                text.text = b.ToString("x2");
            }

            // text.color = b == 0 ? Color.gray : Color.white;
        }

        public void Clear()
        {
            text.text = "";
        }
    }
}