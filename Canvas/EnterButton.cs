using UnityEngine;
using UnityEngine.UI;

namespace UltraNet.Canvas
{
    public class EnterButton : MonoBehaviour
    {
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                GetComponent<Button>().onClick.Invoke();
            }
        }
    }
}
