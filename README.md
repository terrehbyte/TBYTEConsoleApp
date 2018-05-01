# TBYTEConsole

![GIF of TBYTEConsole](https://terrehbyte.com/images/projects/TBYTEConsole.gif)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fterrehbyte%2FTBYTEConsoleApp.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fterrehbyte%2FTBYTEConsoleApp?ref=badge_shield)

This repository contains work devoted towards the development of a Quake-like
console for use in Unity3D. Developers and players alike should be able to
access the console to use commands and cvars to customize their game or settings
to their liking.

# Repository Structure

```
DEVNOTES.md                 Notes to and from the developer.
README.md                   This file! :)
TBYTEConsole/               TBYTEConsole source-code.
TBYTEConsoleUnity/
    Assets/                 Special folder for assets for Unity project.
        TBYTEConsole/       Assets relating to the Console.
        TBYTEConsoleApp/    Example or test assets demostrating console usage.
    ProjectSettings/        Special folder for settings for Unity project.
```

# Quick Start

There are four major types of tokens that can be defined:

**Contents**

- Declaring CVar and CMDs
  1. [Field-backed CVar](#field-backed-cvar)
  2. [Property-backed CVar](#property-backed-cvar)
  3. [String-backed CVar](#string-backed-cvar)
  4. [Command](#command)
- Accessing CVars

## Field-backed CVar

Class fields can be tagged as a CVar to register them with the Console system.
Any read or write operation will revolve around the tagged field.

```C#
public class PlayerState : MonoBehaviour
{
    [CVar("cl_playerName")]
    public static string name;

    void Start()
    {
        // you must call this method to inform the Console of any tagged CVars
        ConsoleLocator.cvarRegistry.RegisterStaticMembers<PlayerState>();
    }
}
```

Only static fields are supported at this time.

## Property-backed CVar

These really are commands that are used in a manner similar to CVars. Rather
than storing a value in a registry, these CVars call upon a method when
retrieving its value or assigning a value to it.

This revolves around four key pieces of information:
  1. What **token** will be used to refer to this CVar?
  2. What **type** of information is this?
  3. What method is used when **getting** its value?
  4. What method is used when **setting** its value?

There are a variety of ways to handle this.

You may tag a static property member of a class as a property. Its get and set
functions will be used when retrieving the value or assigning a valid input. 
```C#
public class PlayerState : MonoBehaviour
{
    [CVarProperty("cl_playerName")]
    public static string name
    {
        get; set;
    }

    void Start()
    {
        // you must call this method to inform the Console of any tagged CVars
        ConsoleLocator.cvarRegistry.RegisterStaticMembers<PlayerState>();
    }
}
```

You may also declare a property in its own static class as well.

You can specify the token, type, and property in the attribute.
```C#
[CVarProperty("sv_timeScale", typeof(float), "value")]
public static class TimeScaleProp
{
    static string value
    {
        get { return Time.timeScale.ToString(); }
        set { Time.timeScale = (float)System.Convert.ChangeType(value, typeof(float)); }
    }
}
```

You can specify the token and type in the attribute. Another attribute is
attached to the property to identify the methods used.
```C#
[CVarProperty("sv_timeScale", typeof(float))]
public static class TimeScaleProp
{
    [CVarPropertyAccessor]
    static string value
    {
        get { return Time.timeScale.ToString(); }
        set { Time.timeScale = (float)System.Convert.ChangeType(value, typeof(float)); }
    }
}
```

You can specify the token, type, getter, and setter methods in the attribute.
Note that the methods must be static and return/accept a string.
```C#
[CVarProperty("sv_timeScale", typeof(float), "getter", "setter")]
public static class TimeScaleProp
{
    static string getter()
    { return Time.timeScale.ToString(); }

    static void setter(string input)
    { Time.timeScale = (float)System.Convert.ChangeType(input, typeof(float)); }
}

// Yes, you could specify the reserved function names for a property as well.
// In fact, that's exactly what happens if you opt for a property instead!
```

You can specify the token and type in the attribute. Another attribute is used
to tag the getter and setter methods. Note that the methods must be static
and return/accept a string.
```C#
[CVarProperty("sv_timeScale", typeof(float))]
public static class TimeScaleProp
{
    [CVarPropertyGetter]
    static string getter()
    { return Time.timeScale.ToString(); }

    [CVarPropertySetter]
    static void setter(string input)
    { Time.timeScale = (float)System.Convert.ChangeType(input, typeof(float)); }
}
```

You could also opt to manually register the CVarProperty yourself.
```C#
public class RegisterCVarProperties : MonoBehaviour
{
    static string timeScale_getter()
    { return Time.timeScale.ToString(); }

    static void timeScale_setter(string input)
    { Time.timeScale = (float)System.Convert.ChangeType(input, typeof(float)); }

    void Start()
    {
        // register the property at runtime in a script!
        ConsoleLocator.cvarRegistry.Register<float>("sv_timeScale",    // token 
                                                    timeScale_getter,  // getter
                                                    timeScale_setter); // setter
    }
}
```

## String-backed CVar

These are the heart and soul of the console. String-backed CVars store data
in text, but retain knowledge about what type it's intended to be.

Write the following anywhere it will be run prior to any other script running
or before it should be accessed by any script. For example, to register a
CVar named "version" that stores `string` values, try the following:

```C#

// Declares and initializes a CVar named "version" with the default string value.
ConsoleLocator.cvarRegistry.Register<string>("version");

// Declares and initializes a CVar named "version" with the "0.1" value.
ConsoleLocator.cvarRegistry.Register<string>("version", "0.1");

// Declares and initializes a CVar named "version" with the "0.1" value.
// ... the type parameter is inferred from the given initial value.
ConsoleLocator.cvarRegistry.Register("version", "0.1");

```

## Command

Tokens intended to execute a particular action can be executed with zero or more
arguments supplied to it.

Write the following anywhere it will be run prior to any other script running
or before it should be accessed by any script. For example, to register a
command named "kill" that would _kill_ the player, try the following:

```C#
public static class GameCommands
{
    static GameCommands()
    {
        ConsoleLocator.cvarRegistry.Register(new CCommand("kill", KillCommand));
    }

    // Name: KillCommand
    // Args: Arguments supplied in the console expression by the user.
    // Return: A string containing any console output.
    static public string KillCommand(string[] Arguments)
    {
        GameObject.Find("Player").GetComponent<PlayerHealth>().TakeDamage(Mathf.Infinity);
    }
}
```

# Roadmap

During the early stages of development, a roadmap will be informally posted
and maintained here in order to maintain working notes and outline goals.

0. Gather commands tagged with a Command attribute
1. Add support for CVars
    - OnChange hooks per CVar (or from registry?)
    - Instance CVars (per Component)
    - Aliases
    - Run-time checks for two-way conv
        - string -> T
        - T -> string
    - Read `*.json` files containing declarations of CVars
    - Add templated `CVarPropertyDeclaration<T>` class
        - Then I won't have to pass the type in as a parameter!
    - Consider handling the conversion from string to `T` for CVarProperty setter...
        - ...or both?
    - Add a scriptable asset that can be installed into the console via component?
    - Add a base class that you inherit from to add a variable?
        - It could even provide info strings like "help"...
    - How should you handle CVars created at runtime? reset?
    - TODO: HOW THE HELL DO I HANDLE CVARs ON INSTANCES?
2. Add support for config files (`*.cfg`)
3. Add support for different colors
4. Add in-game console support
5. Re-factor because it'll probably be sad by the time I'm done
6. Figure out a way to alert the user when a CVar is already register
   but the user is attempting to assign a new value
   - Maybe a "readonly" bool on the CVar?
7. Add support for mirror Debug.Log messages to Console
8. Cache static registrations to prevent duplicate CVars

# LICENSE

Copyright (c) 2016 Terry Nguyen


[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fterrehbyte%2FTBYTEConsoleApp.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Fterrehbyte%2FTBYTEConsoleApp?ref=badge_large)