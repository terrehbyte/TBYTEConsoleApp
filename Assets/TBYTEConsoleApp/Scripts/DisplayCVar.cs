using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TBYTEConsole;

public class DisplayCVar : MonoBehaviour
{
    public Text label;
    public string cvarName = "version";

    void Update()
    {
        CVar<string> versionString = new CVar<string>(cvarName);

        label.text = versionString.ToString();
    }
}
