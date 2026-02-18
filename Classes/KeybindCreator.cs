namespace UltraNet.Classes;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class CustomBindingsPoCPlugin
{
    // Keys & actions settings
    private const string InputMapName = "UltraNet";
    private const string SomeFirstActionName = "Toggle panel";

    #region Action map JSON
    private static readonly string YourActionMapJson = $@"{{
		""maps"": [
			{{
				""name"": ""{InputMapName}"",
				""id"": ""6212261a-20ba-4c68-8568-ef2c0f5d770f"",
				""actions"": [
					{{
						""name"": ""{SomeFirstActionName}"",
						""type"": ""Button"",
						""id"": ""bc66ba63-9493-499b-8c3d-0cf0992fc6a8"",
						""expectedControlType"": ""Button"",
						""processors"": """",
						""interactions"": """",
						""initialStateCheck"": false
					}}
				],
				""bindings"": [
					{{
						""name"": """",
						""id"": ""81a50518-0cf2-4bcd-a000-892c93e461e7"",
						""path"": ""<Keyboard>/t"",
						""interactions"": """",
						""processors"": """",
						""groups"": ""Keyboard & Mouse"",
						""action"": ""{SomeFirstActionName}"",
						""isComposite"": false,
						""isPartOfComposite"": false
					}}
				]
			}}
		]
	}}";
    #endregion

    private static readonly InputActionMap ActionMap = InputActionMap.FromJson(YourActionMapJson)[0];

    public class InputListener : MonoBehaviour
    {
        private Inputs inputs;

        public void Start()
        {
            inputs = Inputs.Instance;
        }

        public void Update()
        {
            if (inputs == null)
                return;

            if (inputs.SomeFirstAction.WasPerformedThisFrame())
                Plugin.instance.PressKey();
        }
    }

    [HarmonyPatch(typeof(InputActions))]
    public static class InputActionPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor)]
        public static void InputActions_Constructor_Postfix(InputActions __instance)
        {
            if (__instance.asset.FindActionMap(InputMapName) != null)
                return;

            MergeInputActionAssets(__instance);
        }

        private static void MergeInputActionAssets(InputActions ukInputActions)
        {
            var jukeboxActionAssets = Resources.FindObjectsOfTypeAll(typeof(InputActionAsset));
            if (jukeboxActionAssets.Length == 0)
            {
                Debug.LogError("Couldn't load InputActionAsset");
                return;
            }

            ukInputActions.asset.AddActionMap(ActionMap);
        }
    }

    [ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
    public class Inputs : MonoSingleton<Inputs>
    {
        private InputActionMap _actionMap;

        public new void Awake()
        {
            base.Awake();
            _actionMap = InputManager.Instance.InputSource.Actions.asset.FindActionMap(InputMapName);

            SomeFirstAction = _actionMap.FindAction(SomeFirstActionName);
        }

        public InputAction SomeFirstAction { get; private set; }
    }

    [HarmonyPatch(typeof(ControlsOptions))]
    public static class ControlsOptionsPatches
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ControlsOptions), "Rebuild")]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = new List<CodeInstruction>(instructions);

            var addMap = AccessTools.Method(typeof(ControlsOptionsPatches), nameof(AddMap));
            var mapField = AccessTools.Field(typeof(CustomBindingsPoCPlugin), nameof(ActionMap));

            foreach (var inst in list)
            {
                if (inst.opcode == OpCodes.Newarr && inst.operand is Type t && t == typeof(InputActionMap))
                {
                    yield return inst;
                    yield return new CodeInstruction(OpCodes.Ldsfld, mapField);
                    yield return new CodeInstruction(OpCodes.Call, addMap);

                    continue;
                }

                yield return inst;
            }
        }

        private static InputActionMap[] AddMap(InputActionMap[] original, InputActionMap extra)
        {
            if (original == null)
                return new[] { extra };

            var newArr = new InputActionMap[original.Length + 1];
            Array.Copy(original, newArr, original.Length);
            newArr[newArr.Length - 1] = extra;
            return newArr;
        }
    }
}