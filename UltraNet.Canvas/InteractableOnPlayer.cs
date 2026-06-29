using UnityEngine;
using UnityEngine.UI;

namespace UltraNet.Canvas;

public class InteractableOnPlayer : MonoBehaviour
{
	private Button button;

	public void Start()
	{
		button = GetComponent<Button>();
		button.interactable = false;
	}

	public void Update()
	{
		button.interactable = MonoSingleton<NewMovement>.Instance != null && MonoSingleton<NewMovement>.Instance.activated;
	}
}
