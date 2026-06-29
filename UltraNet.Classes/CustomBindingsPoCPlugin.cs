using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UltraNet.Classes;

public static class CustomBindingsPoCPlugin
{
	public class InputListener : MonoBehaviour
	{
		private InputListenerInstance inputs;

		public void Start()
		{
			inputs = InputListenerInstance.Instance;
		}

		public void Update()
		{
			if (!(inputs == null) && inputs.SomeFirstAction.WasPerformedThisFrame())
			{
				Plugin.instance.PressKey();
			}
		}
	}

	[HarmonyPatch(typeof(InputActions))]
	public static class InputActionPatches
	{
		[HarmonyPostfix]
		[HarmonyPatch(MethodType.Constructor)]
		public static void InputActions_Constructor_Postfix(InputActions __instance)
		{
			if (__instance.asset.FindActionMap("UltraNet") == null)
			{
				MergeInputActionAssets(__instance);
			}
		}

		private static void MergeInputActionAssets(InputActions ukInputActions)
		{
			InputActionAsset asset = ukInputActions.asset;
			bool wasEnabled = asset.enabled;
			if (wasEnabled)
			{
				asset.Disable();
			}
			asset.AddActionMap(ActionMap);
			if (wasEnabled)
			{
				asset.Enable();
			}
		}
	}

	public class InputListenerInstance : MonoBehaviour
	{
		public static InputListenerInstance Instance;

		private InputActionMap _actionMap;

		public InputAction SomeFirstAction { get; private set; }

		public void Awake()
		{
			Instance = this;
			StartCoroutine(WaitForIt());
		}

		public IEnumerator WaitForIt()
		{
			while (MonoSingleton<InputManager>.Instance == null)
			{
				yield return null;
			}
			_actionMap = MonoSingleton<InputManager>.Instance.InputSource.Actions.asset.FindActionMap("UltraNet");
			SomeFirstAction = _actionMap.FindAction("Toggle panel");
		}
	}

	[HarmonyPatch(typeof(ControlsOptions))]
	public static class ControlsOptionsPatches
	{
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(ControlsOptions), "Rebuild")]
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);
			MethodInfo addMap = AccessTools.Method(typeof(ControlsOptionsPatches), "AddMap");
			FieldInfo mapField = AccessTools.Field(typeof(CustomBindingsPoCPlugin), "ActionMap");
			foreach (CodeInstruction inst in list)
			{
				int num;
				if (inst.opcode == OpCodes.Newarr)
				{
					object operand = inst.operand;
					if (operand is Type t)
					{
						num = ((t == typeof(InputActionMap)) ? 1 : 0);
						goto IL_010f;
					}
				}
				num = 0;
				goto IL_010f;
				IL_010f:
				if (num != 0)
				{
					yield return inst;
					yield return new CodeInstruction(OpCodes.Ldsfld, mapField);
					yield return new CodeInstruction(OpCodes.Call, addMap);
				}
				else
				{
					yield return inst;
				}
			}
		}

		private static InputActionMap[] AddMap(InputActionMap[] original, InputActionMap extra)
		{
			if (original == null)
			{
				return new InputActionMap[1] { extra };
			}
			InputActionMap[] newArr = new InputActionMap[original.Length + 1];
			Array.Copy(original, newArr, original.Length);
			newArr[^1] = extra;
			return newArr;
		}
	}

	private const string InputMapName = "UltraNet";

	private const string SomeFirstActionName = "Toggle panel";

	private static readonly string YourActionMapJson = "{\r\n\t\t\"maps\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"UltraNet\",\r\n\t\t\t\t\"id\": \"6212261a-20ba-4c68-8568-ef2c0f5d770f\",\r\n\t\t\t\t\"actions\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"name\": \"Toggle panel\",\r\n\t\t\t\t\t\t\"type\": \"Button\",\r\n\t\t\t\t\t\t\"id\": \"bc66ba63-9493-499b-8c3d-0cf0992fc6a8\",\r\n\t\t\t\t\t\t\"expectedControlType\": \"Button\",\r\n\t\t\t\t\t\t\"processors\": \"\",\r\n\t\t\t\t\t\t\"interactions\": \"\",\r\n\t\t\t\t\t\t\"initialStateCheck\": false\r\n\t\t\t\t\t}\r\n\t\t\t\t],\r\n\t\t\t\t\"bindings\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"name\": \"\",\r\n\t\t\t\t\t\t\"id\": \"81a50518-0cf2-4bcd-a000-892c93e461e7\",\r\n\t\t\t\t\t\t\"path\": \"<Keyboard>/t\",\r\n\t\t\t\t\t\t\"interactions\": \"\",\r\n\t\t\t\t\t\t\"processors\": \"\",\r\n\t\t\t\t\t\t\"groups\": \"Keyboard & Mouse\",\r\n\t\t\t\t\t\t\"action\": \"Toggle panel\",\r\n\t\t\t\t\t\t\"isComposite\": false,\r\n\t\t\t\t\t\t\"isPartOfComposite\": false\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t]\r\n\t}";

	private static readonly InputActionMap ActionMap = InputActionMap.FromJson(YourActionMapJson)[0];
}
