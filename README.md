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

# Roadmap

During the early stages of development, a roadmap will be informally posted
and maintained here in order to maintain working notes and outline goals.

1. Add support for CVars
    - OnChange hooks per CVar (or from registry?)
    - Instance CVars (per Component)
    - Aliases
2. Add support for config files (`*.cfg`)
3. Add support for different colors
4. Add in-game console support
5. Re-factor because it'll probably be sad by the time I'm done
6. Figure out a way to alert the user when a CVar is already register
   but the user is attempting to assign a new value
   - Maybe a "readonly" bool on the CVar?

# LICENSE

Copyright (c) 2016 Terry Nguyen