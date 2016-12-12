using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TBYTEConsole;

public class DisplayCVar : MonoBehaviour
{
    [CVar("test")]
    public static string cvarTest;

    [CVarProperty("testProp")]
    public static string cvarTestProp
    {
        get { return "lol"; }
        set { /*lol no*/ }
    }

    public Text label;
    public string cvarName = "version";

    static string getter()
    {
        return "getter";
    }
    static void setter(string input)
    {
        return;
    }

    void Start()
    {
        ConsoleLocator.cvarRegistry.RegisterStaticMembers<DisplayCVar>();

        // test runtime methods
        var reg = ConsoleLocator.cvarRegistry;

        reg.Register<string>("cvar1");
        reg.Register("cvar2", "str");
        reg.Register(new CVar<string>("cvar3", true));
        reg.Register<string>("cvar4", getter, setter);
    }

    void Update()
    {
        CVar<string> versionString = new CVar<string>(cvarName);

        label.text = versionString.ToString();
    }
}
