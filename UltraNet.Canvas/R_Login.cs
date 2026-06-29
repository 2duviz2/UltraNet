using TMPro;
using UnityEngine;

namespace UltraNet.Canvas;

public class R_Login : MonoBehaviour
{
	public TMP_InputField Input;

	public void Login()
	{
		ContentManager.instance.Login(Input.text);
	}
}
