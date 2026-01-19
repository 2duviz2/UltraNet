using UnityEngine;
using UnityEngine.UI;

namespace UltraNet.Canvas
{
    public class ResetScrollbar : MonoBehaviour
    {
        public float value = 1;

        void Start()
        {
            ResetPos(value);
        }

        public void ResetPos(float value)
        {
            GetComponent<Scrollbar>().value = value;
        }
    }
}
