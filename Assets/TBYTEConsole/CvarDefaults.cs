using UnityEngine;
using TBYTEConsole;

// 1. Tag the getter/setter method w/ an attribute.
[CVarProperty("sv_timeScale", typeof(float))]
public static class TimeScaleProp
{
    [CVarPropertyGetter]
    static public string getter() { return Time.timeScale.ToString(); }

    [CVarPropertySetter]
    static public void setter(string input) { Time.timeScale = (float)System.Convert.ChangeType(input, typeof(float)); }
}

// 2. Specify the getter/setter method by name.
[CVarProperty("sv_fixedTimestep", typeof(float), "getter", "setter")]
public static class FixedTimeScaleProp
{
    static public string getter() { return Time.fixedDeltaTime.ToString(); }

    static public void setter(string input) { Time.fixedDeltaTime = (float)System.Convert.ChangeType(input, typeof(float)); }
}

// 3. Specify a get/set property by name.
[CVarProperty("sv_fixedTimestepValue", typeof(float), "value")]
public static class FixedTimeScalePropVal
{
    static string value
    {
        get { return Time.fixedDeltaTime.ToString(); }
        set { Time.fixedDeltaTime = (float)System.Convert.ChangeType(value, typeof(float)); }
    }
}

// 4. Tag the get/set property with an attribute.
[CVarProperty("sv_fixedTimestepValueAttrib", typeof(float))]
public static class FixedTimeScalePropValAttrib
{
    [CVarPropertyAccessor]
    static string value
    {
        get { return Time.fixedDeltaTime.ToString(); }
        set { Time.fixedDeltaTime = (float)System.Convert.ChangeType(value, typeof(float)); }
    }
}