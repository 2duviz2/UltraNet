namespace UltraNet.Canvas;

using TMPro;
using UnityEngine;

public class R_Login : MonoBehaviour
{
    public TMP_InputField Input;

    public void Login()
    {
        ContentManager.instance.Login(Input.text);
    }
}
