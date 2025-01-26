using UnityEngine;

namespace InGame
{
    public class Windows : MonoBehaviour
    {
        [SerializeField] private Window gotoWindow, findWindow;

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.G)) gotoWindow.Switch();
                if (Input.GetKeyDown(KeyCode.F)) findWindow.Switch();
            }
        }
    }
}