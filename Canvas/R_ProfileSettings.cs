namespace UltraNet.Canvas;

using TMPro;
using UltraNet.Classes;
using UnityEngine;

public class R_ProfileSettings : MonoBehaviour
{
    public TMP_InputField bio, pron;

    public void UpdateBio()
    {
        Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.profileSetBioUrl, new() { { "token", ContentManager.GetToken() }, { "input", bio.text } }, (json) => {}));
    }

    public void UpdatePronouns()
    {
        Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.profileSetPronounsUrl, new() { { "token", ContentManager.GetToken() }, { "input", pron.text } }, (json) => {}));
    }
}
