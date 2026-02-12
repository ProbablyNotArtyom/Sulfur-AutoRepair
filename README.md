# Sulfur-AutoRepair

A mod for SULFUR that automatically repairs equipped items when entering the hub world (The church).

## Usage

When entering the hub, all equipped items (head, torso, left+right foot, weapon1+2) will be repaired as if Ralphie's repair service was used.
Repairs cost money the same way it would doing it manually, which will be deducted from the stash first and from the player next just like the base game.
The repair sound effect will play if any items were repaired.

You can also press **(L)** when in the hub to trigger repairs manually.

## Requirements

- [BepInEx 5.x](https://github.com/BepInEx/BepInEx). Installation instructions [can be found here](https://docs.bepinex.dev/articles/user_guide/installation/index.html).
- SULFUR `v0.14.14`. Other versions will likely work, but it has not been tested.

## Installation 

Put AutoRepair.dll into `..\SULFUR\BepInEx\plugins\` after installing BepInEx. If the directory does not exist, either create it, or run the game with BepInEx at least once to generate it.

## Building

### Setup

The .NET Framework SDK (Developer Pack) is required to build this project, provided by the `dotnet-sdk` package on Arch.
I am using the latest version (v10.0.100), but anything above v6.0 should work.

I have only tested building on Linux; it should work on Windows, but I can't provide any details on how to do so.

The *.DLLs from `..\steamapps\common\SULFUR\Sulfur_Data\Managed` need to be copied (NOT symlinked) into `lib` folder in the project root.

### Compile

The project can be built by running `dotnet build` in the root directory.
This generates a lot of DLLs in the build folder, but the only important one is AutoRepair.dll, which is the finished plugin.


