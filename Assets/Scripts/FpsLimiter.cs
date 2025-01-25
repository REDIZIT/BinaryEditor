using UnityEngine;

namespace InGame
{
    public class FpsLimiter : MonoBehaviour
    {
        private void Start()
        {
            Application.targetFrameRate = 60;
        }
    }
}