# TBYTEConsole

This repository contains work devoted towards the development of a Quake-like
console for use in Unity3D. Developers and players alike should be able to
access the console to use commands and cvars to customize their game or settings
to their liking.

# Repository Structure

```
DEVNOTES.md                 Notes to and from the developer.
README.md                   This file! :)
Assets/                     Special folder for assets for Unity project.
    TBYTEConsole/           Assets relating to the Console.
    TBYTEConsoleApp/        Example or test assets demostrating console usage.
ProjectSettings/            Special folder for settings for Unity project.
```

# Quick Start

There are three major types of tokens that can be defined:

1. String-backed CVar
2. Property-backed CVar
3. Command

## String-backed CVar

These are the heart and soul of the console. String-backed CVars store data
in text, but retain knowledge about what type it's intended to be.

Write the following anywhere it will be run prior to any other script running
or before it should be accessed by any script. For example, to register a
CVar named "version" that stores `string` values, try the following:

```C#

// Declares and initializes a CVar named "version" with the default string value.
CVarRegistry.Register<string>("version");

// Declares and initializes a CVar named "version" with the "0.1" value.
CVarRegistry.Register<string>("version", "0.1");

// Declares and initializes a CVar named "version" with the "0.1" value.
// ... the type parameter is inferred from the given initial value.
CVarRegistry.Register("version", "0.1");

```

## Property-backed CVar

These really are commands that are used in a manner similar to CVars. Rather
than storing a value in a registry, these CVars call upon a function to
retrieve and assign their value.

```C#
// Mark the declaring class with this attribute.
[CVarProperty("sv_timeScale", typeof(float))]
public static class TimeScaleProp
{
    // Mark the setter function with this attribute.
    [CVarPropertyGetter]
    static public string getter()
    {
        return Time.timeScale.ToString();
    }

    // Mark the getter function with this attribute.
    [CVarPropertySetter]
    static public void setter(string input)
    {
        Time.timeScale = (float)System.Convert.ChangeType(input, typeof(float));
        return;
    }
}
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
        Console.Register(new CCommand("kill", KillCommand));
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

1. Add support for CVars
    - OnChange hooks per CVar (or from registry?)
    - Instance CVars (per Component)
    - Aliases
    - Run-time checks for two-way conv
        - string -> T
        - T -> string
    - Read `*.json` files containing declarations of CVars
    - Add CVarProperty attribute
        - Specify get/set methods by string/methodName
2. Add support for config files (`*.cfg`)
3. Add support for different colors
4. Add in-game console support
5. Re-factor because it'll probably be sad by the time I'm done
6. Figure out a way to alert the user when a CVar is already register
   but the user is attempting to assign a new value
   - Maybe a "readonly" bool on the CVar?
7. Add support for mirror Debug.Log messages to Console

# LICENSE

Copyright (c) 2016 Terry Nguyen