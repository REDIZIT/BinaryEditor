using TMPro;
using UnityEngine;

namespace InGame
{
    public class InputFieldValidator : MonoBehaviour
    {
        [SerializeField] private Sprite focusSprite, invalidSprite, defaultSprite;
        
        [Require] private TMP_InputField field;
        
        public void MarkValid(bool isValid)
        {
            var sprites = field.spriteState;
            sprites.selectedSprite = isValid ? focusSprite : invalidSprite;
            sprites.pressedSprite = sprites.selectedSprite;
            sprites.highlightedSprite = isValid ? defaultSprite : invalidSprite;
            field.spriteState = sprites;
            
            field.image.sprite = sprites.highlightedSprite;
        }
    }
}