using UnityEngine;

namespace InGame
{
    public class Windows : MonoBehaviour
    {
        [SerializeField] private Window gotoWindow;

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G))
            {
                gotoWindow.Switch();
            }
        }
    }
}