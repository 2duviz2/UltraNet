using UnityEngine;
using UnityEngine.UI;

namespace UltraEditor.Classes.Canvas
{
    internal class ResetScrollbar : MonoBehaviour
    {
        public float value = 1;

        void Start()
        {
            ResetPos();
        }

        void ResetPos()
        {
            GetComponent<Scrollbar>().value = value;
        }
    }
}
