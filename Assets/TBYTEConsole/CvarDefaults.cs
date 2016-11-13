using UnityEngine;
using TBYTEConsole;

[CVarProperty("sv_timeScale", typeof(float))]
public static class TimeScaleProp
{
    [CVarPropertyGetter]
    static public string getter()
    {
        return Time.timeScale.ToString();
    }

    [CVarPropertySetter]
    static public void setter(string input)
    {
        Time.timeScale = (float)System.Convert.ChangeType(input, typeof(float));
        return;
    }
}