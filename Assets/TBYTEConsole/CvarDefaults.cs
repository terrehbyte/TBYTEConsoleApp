using UnityEngine;
using TBYTEConsole;
using System;

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

[CVarProperty("sv_fixedTimestep", typeof(float), "getter", "setter")]
public static class FixedTimeScaleProp
{
    static public string getter()
    {
        return Time.fixedDeltaTime.ToString();
    }

    static public void setter(string input)
    {
        Time.fixedDeltaTime = (float)System.Convert.ChangeType(input, typeof(float));
        return;
    }
}