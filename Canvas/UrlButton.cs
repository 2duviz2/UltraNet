using UnityEngine;

namespace UltraNet.Canvas
{
    public class UrlButton : MonoBehaviour
    {
        public void OpenUrl(string r)
        {
            Application.OpenURL(r);
        }
    }
}