# Sulfur-AutoRepair

A mod for SULFUR that automatically repairs equipped items when entering the hub world (The church)

## Usage

When entering the hub, all equipped items (head, torso, left+right foot, weapon1+2) will be repaired as if Ralphie's repair service was used.
Repairs cost money the same way it would doing it manually, which will be deducted from the stash first and from the player next just like the base game.
The repair sound effect will play if any items were repaired.

You can also press **(L)** when in the hub to trigger repairs manually



## Installation

This mod requires [BepInEx 5.x](https://github.com/BepInEx/BepInEx).
It's a simple install and the instructions to do so [can be found here](https://docs.bepinex.dev/articles/user_guide/installation/index.html).

Put AutoRepair.dll into `..\SULFUR\BepInEx\plugins\`


## Building

I have only tested building on Linux; it should work on windows, but I can't provide any details on how to do so.

The *.DLLs from `<game_dir>\Sulfur_Data\Managed` need to be copied (NOT symlinked) into **.\lib**, where `<game_dir>` is wherever you installed the game (..\steamapps\common\SULFUR).

.NET Framework SDK (Developer Pack) v4.7.2 is required to build this project.

On Linux, install `dotnet-sdk`. Any SDK version above v6.0 should be able to target v4.7.2. This project was built using the latest version (v10.0.100).

Once the game DLLs have been copied into **.\lib**, the project can be built by running `dotnet build` in the root directory.
This generates a lot of DLLs in the build folder, but the only important one is AutoRepair.dll, which is the finished plugin.


