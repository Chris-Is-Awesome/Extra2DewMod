![Extra 2 Dew Mod](/Images/logo.jpg)

### [Download! Install instructions below!](https://github.com/Chris-Is-Awesome/Extra2DewMod/releases)


**Credit: Ludosity** | **Modded by:** **[Chris is Awesome#9819](https://github.com/Chris-Is-Awesome)**, **[Matra#5057](https://github.com/SpaceMatra)** & **[GameWyrm#7777](https://github.com/GameWyrm)**

This is a mod meant to add more content to Ittle Dew 2!
Mainly adds debug console commands, additional game modes, fun stuff, and more.

https://www.youtube.com/watch?v=NwmEcC4uuUc

Join the Discord server for all things Ludo modding! https://discord.gg/qVjcfT2

# INSTRUCTIONS
**Install:** Download the ID2_Data.zip from [here](https://github.com/Chris-Is-Awesome/Extra2DewMod/releases). Extract the folder to the directory: `[Steam Games Directory]\steamapps\common\Ittle Dew 2`. You will be asked to confirm to replace some things in the ID2_Data folder. Replace. This will also add a "mods" folder inside the ID2_Data folder which is used specifically by our mod.

Speficially what gets installed is a modified `Assembly-CSharp.dll` file, which contains all the game's code. And a "mod" folder is added inside root of the ID2_Data folder. This mod folder is used by various aspects of our mod for writing data to files and such.

**Usage:** Pause the game and press `F1` *(default)* to open the debug console. Type `help` in console window for a list of all commands! There are some miscelanious commands that are not listed below, but they're included in `help`!

**Uninstall:** Go to `Steam -> Right click game -> Properties -> Local Files -> Verify integrity of files...`. You can also make a copy of your original `Assembly-CSharp.dll` file before installing the mod and use it to replace the modded file.

# USING THE MOD
The mod includes several in-game menus that handle almost all the mod functions. It is recommended to navigate them using the mouse. For more advanced functions, the mod includes a debug menu. It uses commands to make precise changes into the game.

# GAME MODES
The mod includes several game modes unique to it, which multiple of them can be active at the same time. To turn a game mode on, go to the "Game mode" menu when creating a new file, select the mode you want to play and check the box on the title. The mode will be active when starting the game.

### Heart Rush
You start with a set amount of HP. When you lose it all, you have to start all over!
- You start with 30 hearts, but you never gain any except from Crayons, which add 4 hearts!
- When you take damage, damage is taken from your max HP
- When you run out of HP, it's game over! You return to main menu and Heart Rush file is deleted, making you start all over!
- Start a Heart Rush file by making a new file named "hearts"!
### Boss Rush
Rush through all the bosses back to back in a nonstop frenzy of blood boiling mayhem!
### Dungeon Rush
Rush through all the dungeons back to back in a nonstop frenzy of mind melting puzzles!
### Entrance Randomzier
Randomizes all scene transitions while still making the game beatable. Randomization affects caves, dungeons, overworld connections and some secret places. Hidden entrances to caves will be randomized, such as the grass patch in fancy ruins and jenny berry's home. Connections left out of the randomization: The grand library, all mechanic dungeons, the dream world, the secret remedy and any one way connection.
### Item Randomizer
Randomizes the items throughout the world!
### Cosmetic Randomizer
Randomizes the colors a lot of cosmetic stuff (HUD, particles, water, etc.) and has randomized music as well!
### Enemy Randomizer (alpha)
A very experimental mode that randomizes each enemy in the game, including bosses!

# CREATIVE MENU
The creative menu is a new, GMOD like addition to the mod. It enables moving, modifying or creating new game objects. For instruction in how to use it, please click the "HELP" button on the left-bottom corner of the screen.

# CAMERA CONTROLS
E2D includes 2 new camera modes to play the game in completely new ways! Those are **First person mode** and **Third person mode**. There is also a camera mode for exploring the map and taking screenshots/doing videos, **Free camera mode**.

**Note**: Since ID2 lacks proper controla for mouse movement, the method used in this mod requires calibration. During camera mode setup and scene change, mouse movement will be disabled for some time. Please leave the mouse still to make the process quicker.

### FIRST PERSON MODE
Enter `cam -first` to activate first person mode. This mode simulates a FPS (first person shooter) and adds a fair ammount of difficulty to the game, specially for puzzles. The camera is controlled with the mouse and the movement keys are changed to the classic WASD. To use a weapon, press the primary mouse button (left click in most cases) and press 1-2-3-4 to choose which weapon will be used. Alternatively use the mouse scroll wheel to change weapons. The controls can be changed anytime with the `-controls` argument (however mouse buttons/scroll wheel cannot be changed).

### THIRD PERSON MODE
Enter `cam -third` to activate third person mode. This mode allows rotating the camera around Ittle. The camera is controlled with the mouse and the movement keys are changed to the WASD. To use a weapon, press the primary mouse button (left click in most cases) and press 1-2-3-4 to choose which weapon will be used. Alternatively use the mouse scroll wheel to change weapons. To zoom in/out, hold the secondary mouse button (right click) and scroll with the mouse wheel. The controls can be changed anytime with the `-controls` argument (however mouse buttons/scroll wheel cannot be changed).

### FREE CAMERA MODE
Enter `cam -free` to activate free camera mode. You don't control ittle in this mode, all controls are switched to the camera instead. Use it to fly around and take a closer look to the level! Also remember to leave Ittle somewhere safe first! She will be defenseless while you are gone (or make her invincible with the god command). The camera is controlled with the mouse and WASD moves it around. Use the mouse scroll wheel to change your flying speed. The controls can be changed anytime with the `-controls` argument (however the scroll wheel cannot be changed).

### DEFAULT CAMERA MODE
This is the vanilla camera. Enter `cam -default` to enter this mode.

### CUSTOMIZATION
You can customize the options below. See the `cam` command in the next sub-section for more details:
- Camera controls: You can set the camera controls keys anytime. They are set in a specific order and use KeyCodes, more information about them can be found in the Types of Input section.
- Field of view: Set the camera field of view, the vanilla default is 30 while 65 is used in all E2D camera modes. You can set the field of view for default mode too!
- Mouse sensitivity: Set the sensitivity of the mouse.
- Lock/Unlock vertical panning: In ittle dew 2, you cannot point your weapons upwards, so vertical panning can be unnecesary. You can turn it off/on anytime you want.
- Clipping distance: By default, the camera draws the scene until a certain distance. We moved that value up for this mod, but you can set it even higher if you want. Take in mind though that higher clipping distances are more difficult for your computer.
- Wheel acceleration: Set how strongly the mouse wheel affects your flying speed (free mode only).

# DEBUG MENU COMMANDS
- `atk`
  - Description: Change how Ittle's weapons work, making them more powerful and verastile!
  - Modifiers:
    - `-attack` Supercharge attacks. Removes all attack delays and recoils. Try this with EFCS!
    - `-dynamite` Unlimited dynamite sticks
    - `-fuse <float>` Set the timer for dynamite stick explosion
    - `-radius <float>` Set the radius of the dynamite explosion
    - `-ice` Unlimited ice blocks
    - `-icegrid` Enables/disables ice block gridlock
    - `-proj <int>` Multiply amount of projectiles spawned from Ittle's weapons
    - `-range <float>` Increase Ittle's melee attack range
    - `-reset` Resets atk mods to normal
  - Examples: `atk -attack`, `atk -fuse 10 -radius 50`, `atk -ice`
- `bind`
   - Description: Sets a command to a key, making you able to enter commands out of the debug menu. Any command can be saved to any key in the keyboard. `bind last <Keycode>` is one of the most useful options of this command, binding the last entered command to <KeyCode>, making binding much quicker than the regular way. For valid KeyCode values, see the types of input section.
  - Modifiers:
    - `-bind <Keycode> <command>`: Save <command> to key <Keycode>
    - `-bind last <Keycode>`: Save the last used command to key <Keycode> (this is really useful!)
    - `-bind list`: List all active keybinds
    - `-bind unbindall`: Unbind all keys bound by 'bind'
  - Examples: `bind LeftControl likeaboss` `bind list`
  - Notes: Key names must be exact KeyCode name from [this page](https://docs.unity3d.com/ScriptReference/KeyCode.html)
- `bubble`
	- Description: Changes all text in speech bubbles nearby Ittle
	- Arguments: `bubble <text>`
	- Examples: `bubble Hello World`, `bubble Ittle rocks!`, `echo This is a line|This is a second line`
	- Notes:
		- You can use spaces in the text
		- Use | (thats not l or i) to make a line jump
		- The text accepts <html> codes for style. You can make the text bold, italic, bigger or change its color. Check [here](https://docs.unity3d.com/Manual/StyledText.html) for available codes and how to use them.
- `cam`
	- Description: Configures the camera mode. (See `Alternate Camera Modes` section below!)
	- Modifiers:
	   - `-first`: Switch camera and controls to first person mode.
	   - `-third`: Switch camera and controls to third person mode.
	   - `-free`: Switch camera and controls to free camera mode.
	   - `-default`: Switch camera and controls back to default.
	   - `-fov <float>`: Set the camera field of view.
	   - `-sens <float>`: Set the sensitivity of the mouse.
	   - `-lockvertical / -unlockvertical`: Disallow/allow vertical panning.
	   - `-clip <float>`: Set the farclip of the camera (draw distance).
	   - `-wheel <float>`: Set how strongly does the mouse wheel change the flying speed (free mode only)
	   - `-controls <KeyCode> x 8`: Set the key controls for first person mode (case sensitive!)
	   Order to enter the keycodes: Forward, backward, left, right, stick, force wand, dynamite and ice ring
	   "Example: `cam -first`
	   "Note: Non default cameras needs half a second with the mouse still to calibrate after enabling the mode or changing map!");
- `clearprefs`
  - Description: Clear all mod preferences. You can specify a modifer to only delete specified preference.
  - Examples: `clearprefs all`, `clearprefs BestRollTime`
- `createitem`
	- Description: Creates an item next to the player. `createitem` is limited to the items in the scene, dungeons can contain weapons while caves can contain scrolls. Enter `createitem` alone to print a list of all available items on the scene.
	- Arguments: `itemName (string)`
	- Modifiers:
		- `-p <Vector3>`: Change spawn position from next to ittle to `<Vector3>`. This uses global positions.
	- Examples: `createitem heart`, `createitem fruitbanana`
- `echo`
	- Description: Shows text on the debugger. Useful for loadconfig files
	- Arguments: `echo <text>`
	- Examples: `echo Hello World`, `echo This is gonna be a challenge!`, `echo This is a line|This is a second line`
	- Notes:
		- You can use spaces in the text
		- Use | (thats not l or i) to make a line jump
		- Entering "Error" anywhere on the text will make it be shown red in the console and reduce its size if it has too many letters
		- The text accepts <html> codes for style. You can make the text bold, italic, bigger or change its color. Check https://docs.unity3d.com/Manual/StyledText.html for available codes and how to use them
- `find`
	- Description: Finds a gameobject in the scene, giving information about it and allowing manipulating its transform properties. When multiple objects are found, it will return the one closest to the player. This is a command mostly for debugging, but some very fun stuff can be done by moving npcs and objects around.
	- Arguments: find gameobjectName (string). This can search for a fragment of the name, making searchs easier.
	- Modifiers:
		- `-all`: Include inactive objects in the search
		- `-noparent`: Only show objects which don't have a parent.
		- `-parent <string>`: Only show objects which have a parent in its hierarchy containing <string> in their name
		- `-parent2 <string>`: Only show objects which have a direct parent containing <string> in their name
		- `-i <int>`: Choose which object from the search to show (from <0> to <ammount found - 1>)
		- `-t`: Show transform of the object (parent, children, position, rotation and scale)
		- `-c`: Show components of the object
		- `-save`: Save a reference to the found object
		- `-load`: Use the saved object instead of doing a search
		- `-p/-gp/-r/-gr/-s (vector3 or x/y/z float)`: Change local position/global position/local rotation/global rotation	or scale of the object. Use x/y/z followed by a float to change just one component
		- `-changep`: Change the parent of the found object to the one saved with -save. The object will move, rotate and scale with the parent.
		- `-activate <(optional) bool>`: Sets the gameobject to active or inactive
	- Examples: `find safe` (returns all the object with "safe" in their names
	- Notes:
		- Use this to move objects around. You can even do custom HUDs with it!
		- Changing parents can break the game sometimes, so do it carefully
		- The player gameobject is called "playerent". The player's graphics are called "Ittle".
- `god`
  - Description: Makes you invincible and gives you full health. Enter the command again to disable.
- `goto`
  - Description: Warp to any map at any spawn point. Just type `goto` without modifiers and you'll load at the map's default spawn point!
  - Modifiers:
    - mapName (no spaces), spawn is just spawn number (int)
  - Examples: `goto PillowFort 1`, `goto PF 1`, `goto d1 1`
  - Notes:
    - You can always use the name shown on map screen!
- `help`
  - Description: Shows a list of all available commands
  - Examples: If you need help with this, you need more help than we can provide :(
- `hp`
  - Description: Sets your current and/or max HP. You can add to the current ammount or set it to any number you want. The conversion is 1 = 1/4 heart. The player wont be killed if their health gets reduced to 0 by this command, though they will die by any damage done to them afterwards.
  - Modifiers:
    - `-full` Heals Ittle completely
    - `-addhp <float>` Adds `<float>` HP to Ittle's total; does not heal
    - `-sethp <float>` Sets Ittle's current HP to `<float>`; does not heal
    - `-addmaxhp <float>` Adds `<float>` HP to Ittle's total; fully heals
    - `-setmaxp <float>` Sets Ittle's current HP to `<float>`; fully heals
  - Examples: `hp -addhp 40`, `hp -addmaxhp -5`
- `hum`
	- Description: Makes ittle hum!
	- Notes:
		- Best used bound to a key. `bind n hum` will make ittle hum on command pressing the n key
- `knockback`
  - Description: Sets the knockback multiplier, how far away both the enemies and player are pushed when damaged (default 1)
	- Modifiers:
    - `<float>` (knockback multiplier)
	- Examples: `knockback 0`, `knockback 50.25`, `knockback -1`
	- Notes:
		- Applies to both Ittle and enemies
		- You can set this value to negative numbers to pull the victim instead of pushing them away!
  - Examples:
- `likeaboss`
	- Description: 1 hit kill any enemy with any weapon
	- Examples: `likeaboss`
	- Notes:
		- Currently does not allow killing invincible (steel) enemies, with exception being Mercury Jelly for some reason
- `loadconfig`
	- Description: (Advanced!) Runs commands from a text file in the Ittle Dew 2\ID2_Data\mod folder. They are run in order from top to bottom. This can be useful for entering a lot of commands at the same time or creating custom scenarios adding npcs to the map. You can share txt files, they will run exactly the same in all copies of ittle dew 2 that have this mod.
	- Arguments: loadconfig fileName(string)
	- Modifiers:
	   - `-onload <string>`: Run <string>.txt when loading a new scene.
	   - `-clearonload`: Stop running the -onload file
	   - `-showerrors`: After finishing running the file, return all invalid commands
	- Examples: `loadconfig helloworld`
	- Notes:
		- The "mod"  folder ìs not present in the vanilla game, you have to create it and put the txt files there for this command to work
		- loadconfig automatically looks for the init.txt file when you start the game and runs it. This can be useful to setup key binds or camera modes. If the file doesn't exist, nothing will happen.
		- Warning: If you are going to set a txt in `-onload` that uses the `goto` command, remember to `-clearonload` before the goto command. in the same file or else you will enter an infinite loop!
- `noclip`
	- Description: Enables/disables Ittle's primary hitbox
	- Notes:
		- You will automatically snap to ground level if ground collision is below you. Might include option to disable this in future
- `pos`
	- Description: Set Ittle's position. Can also save/load positions
	- Arguments (setting pos): `posX (float)`, `posY (float)`, `posZ (float)`
	- Arguments (saving/loading pos): `save/load (string)`
	- Examples: `pos 10.275 5 -90`, `pos save`, `pos load`
	- Notes:
		- Saving/loading currently only allows saving 1 position, and it saves current position
    - Giving just 1 number will set all axes to number specified
		- You will automatically snap to ground level if ground collision is below you. Might include option to disable this in future
- `setitem`
	- Description: Gives you an item with the chosen level
	- Arguments: `itemName (string)`, `itemLevel/itemCount (int)`
	- Optional modes: `setitem dev` (get dev weapons), `setitem all` (get all equipable items)
	- Examples: `setitem dynamite 4`, `setitem raft 8`, `setitem localkeys 42`, `setitem dev`
	- Notes:
		- Dynamite, Force Wand and Ice Ring have level 4 upgrades that were used during development for faster testing. Dynamite acts as EFCS without charge/recoil time. Force shoots lightning and does more damage than anything else in the game. Ice does increased damage.
		- Currently the names are names via Unity, no alternate names are accepted (yet!). You will get a complete list of all available items each time you enter this command.
		- Not all items can be set this way due to how items are handled. You can set anything in inventory screen with this. Nothing more, for now.
		- You can use `setitem dev` to get all items + the 3 lv 4 items, `setitem all` to get all items, or `setitem none` to remove all items
- `showhud`
	- Description: Enables/disables display of the HUD (user interface)
	- Notes:
		- Minor bug: Disabling HUD with a speech bubble up, then re-enabling HUD will result in text not being there. Should be fixed soon™
- `size`
	- Description: Set Ittle's or enemies' size. This can work in two different ways. Enter one float to scale the player/enemy by that number or enter a vector3 (3 floats in a row) to set all the axes one by one.
	- Arguments: `self/enemy (string)`, `scaleX (float)`, `scaleY (float)`, `scaleZ (float)`
	- Examples: `size self 5`, `size enemy 2.5 0.25 2.5`
	- Notes:
		- Numbers smaller than 1 and bigger than 0 will make things smaller. Numbers bigger than 1 will make things bigger.
		- Giving just 1 number will set all axes to number specified.
		- Setting one axis to a very small number will make the characters flat! For example `size self 1 1 0.01'.
- `sound`
	- Description: Prints a list of scene sounds on the screen and plays one of them. You can change the pitch of the played sound. When no arguments are entered, the list will scroll to a different page of sounds.
	- Modifiers:
		- `-play <string>`: Play a sound
		- `-pitch <float>`: Change the pitch of the sound
	- Examples: `sound -play eatfruit` `sound -play eatfruit -pitch 0.5`
	- Notes: The same sound being played with multiple pitchs each one bound to a different key, it is possible to play music!
- `spawn`
	- Description: Spawns NPCs on top of ittle facing a random direction. The name of the NPC must be typed exactly as the `-list` modifier shows, but it is not case sensitive. This command can only spawns NPCs present in the scene, that means you normally cannot spawn dungeon bosses outside of dungeons. There is however a way to `hold` that NPC and spawn it anywhere you want. To get a list of all available NPCs, use `-list`.
	- Arguments: `spawn <npcname>` (optional) `<ammountToSpawn>`. For example, `spawn fishbun` `spawn fishbun 5`
	- Modifiers:
		- `-list`: Display all NPCs available for spawning in the scene (VERY USEFUL).
		- `-p <Vector3>`: Spawn position relative to ittle. Use `globalpos` to change it to global position instead. Example: `spawn fishbun -p 1 0 1`.
		- `-s <float or Vector3>`: Scale of NPC (less than 1 for smaller, more than 1 for bigger). If a float to just scale the NPC, a vector3 to do more complex transformations. Example `spawn fishbun -s 2` `spawn fishbun -s 1 1 3`.
		- `-r <float>`: Spawn NPCs facing <float> angle instead of a random direction (angle goes from 0 -facing top- to 360). Example: `spawn fishbun -r 180`
		- `-globalpos`: Use global position coordinates instead of relative to ittle. Use in conjunction with `-p`.
		- `-ai <string>`: Replace the normal Ai controller for one of another NPC. This can get strange results, don't expect the NPC to be able to attack at all. Example: `spawn stupidbee -ai fishbun`
		- `-hp <float>`: Set the HP of the NPC. Example: `spawn fishbun -hp 5000`
		- `-ittlerot`: Spawn NPCs facing the same rotation than ittle instead of a random one.
		- `-hold`: Save the NPC in memory instead of spawning it. Makes it possible to spawn the NPC out of the scene. For example, if you want to spawn a le biadlo boss in fluffy fields, first go to any dungeon and enter `spawn lebiadloa -hold` then go to fluffy fields and enter `spawn lebiadlo`
	- Examples: `spawn fishbun 5 -s 3`
- `speed`
	- Description: Set Ittle's speed (default 5)
	- Arguments: `speedValue (float)`
	- Examples: `speed 15`
	- Notes:
		- Negative speed values will make ittle moonwalk
- `stats`
	- Description: Shows various in-game stats (longest roll, enemies killed, total damage dealt/received, deaths and more!)
- `time`
	- Description: Control time. Set current in-game time, set time flow, or enable/disable level timers (eg. Remedy 7m wait, cow UFO 15s wait, etc.) (default time flow is 4)
	- Parameters: `settime/setflow/timers (string)`
	- Parameters `(settime): timeOfDay (float, 0-24)`
	- Parameters `(setflow): timeRate (float)`
	- Examples: `time settime 12`, `time setflow 100`, `time timers`
	- Notes:
		- Epilepsy warning if setting time flow to a high amount, as time of day will go by so fast, it can result in world lighting flashing every frame
		- Time passes by in-game hours per real-time minute (so time flow of 4 = 4 in-game hours per real-time minute)
		- Setting timeflow to a negative will reverse time! Setting it to 0 will freeze time
		- Passing no arguments will output current time and time flow and if timers status (enabled/disable)
- `zap`
	- Description: Spawn a lightning bolt beneath your feet to zap all enemies around you
	- Notes:
		- Best used bound to a key. `bind n zap` will run zap every time you press the n key

# PARAMETER REFERENCE
For a list of all enemy names for use with commands like `spawn`, [click here](/Reads/npcs.md).<br>
For a list of all map names for use with commands like `goto`, [click here](/Reads/maps.md).

# TYPES OF INPUTS
- `float`: These are numbers. They can be positive, negative, with decimals or not. Examples: 0, -2, 1.6, -7.856
- `int`: These are numbers like floats, but they cannot have decimals. Examples: 0, -2, 1, -7
- `bool`: These are toggles, indicate if something is on or off. Examples: true, false, on, off, yes, no, 1, 0.
- `vector3`: 3 floats in a row. It is important to add spaces between them. Examples: 0 0 0, 1 1 1, 5.5 0 0, -75 -180 180
- `string`: These are words, literally. Examples: fishbun, fluffyfields, ittle
- `KeyCode`: Code number for a key in the keyboard. These are case sensitive, so pay attention to uppercases. Single letter keycodes can be entered in lowercase for your convenience (`p` and `P`do the same). Below is a list of useful keycodes, for a complete list of unity keycodes, go to https://docs.unity3d.com/ScriptReference/KeyCode.html.:
Letters: the same letter (A, B, C)
Numbers on top of the letters: Alpha1, Alpha2, Alpha3, and so on
Numbers on the keypad: Keypad0, Keypad1, Keypad2, and so on
Arrows: UpArrow, DownArrow, RightArrow, LeftArrow
F keys: F1, F2, F3, F4, and so on
Misc keys: Space, Tab, Return, Backspace, LeftControl, RightAlt

# TYPES OF COMMANDS
The commands are split in five different categories, from easier to use to harder to use:

- `Simple commands`: These commands do something when they are entered. They are simple, just enter the command alone and it will do its job, no more inputs required!
Examples: `help`, `zap`, `hum`
- `Toggle commands`: These commands activate the first time they are entered and deactivate when they are entered a second time. For example, entering `god` will make you invincible, enter `god` again and the invincibility will be deactivated. You can also add a `on` or `off` after the command to set a specific mode, `god on` will always activate god every time. You can also use other words, like `1`/`yes`/`true` and `0`/`no`/`false`.
Examples: `god`, `likeaboss`, `noclip`
- `Parameter commands`: These commands need a number to customize how they work. For example, `speed 15` will change the speed of the player to 15. All parameter commands have the option to be entered without a number. This will change the value they modify back to their default value (`speed` will set the player's speed back to normal). This reset function applies to other settable variables in `Arguments commands` and `Modifiers commands`.
Examples: `speed`, `knockback`
- `Arguments commands`: These commands follow a specific order of word inputs to work and can do multiple things depending on the arguments used. For example, `size` can change the size of the player or the enemy depending on how it is entered. `size self 2` will make the player twice as big while `size enemies 2` will instead make the enemies twice as big. If srguments commands are entered without any inputs (`size`), they will list all the arguments available.
Examples: `size`, `bind`, `time`
- `Modifier commands`: These commands are multi-function, they do multiple things or do one thing with heavy customization. They are controlled by modifiers, these are words that change how the commands work, easily recognizable by their names starting with `-`. They can be put anywhere after the command and multiple of them can be used at the same time. For example, `cam -first` will activate first camera mode while `cam -fov 90` will change the field of vision to 90. Or you can enter `cam -first -fov 90` to do both at the same time! Since these commands are complex, entering any of them without arguments (`cam`) will give you a list of all available modifiers and how to use them. Keep in mind that some commands may require a arguments in a specific order before any modifiers are used.
Examples: `spawn`, `cam`, `atk`

# SCRIPT FILES
Extra2dew can run basic scripts composed of debugmenu commands. This allows running a large group of commands easily, making it possible to create fun scenarios that can be shared!

Said scripts are saved as txt files, which can be writen with any text editor. The commands are run in order from top to bottom. The script files will run exactly the same in all copies of ittle dew 2 that have this mod installed.

### SETUP
After installing the mod, the `mod` folder will be created in `[Steam Games Directory]\steamapps\common\Ittle Dew 2\ID2_Data\`. Place scripts files here and run them by entering `loadconfig <filename>`, without the .txt at the end. The mod comes with a few example scripts already available to use, like tinyslayer.txt.

### CONDITIONS AND COMMENTS
The script files can check for conditions during their executions. If the conditions are not met, the execution will halt and an error message will be shown. This can be helpful if an npc needs to be spawned, but it is not yet present on the scene. The error message can be customized.

`#CONDITION_NPC <npcname>`: Looks for an NPC in the scene.
`#ERRORMSG <errortext>`: Error message to be displayed in case of a condition not being met. | are replaced by line jumps.

Example:
`#CONDITION_NPC npcjennycat`
`#CONDITION_NPC npcjennycat `
`#ERRORMSG The cat was not found, go|to fluffy fields first`

In addition, any line starting with `//` will be considered a comment.

### INIT FILE
If you create a file called init.txt and place it in the mod folder, it will run every time a file is loaded or a new game is started. This can be helpful to set key binds or camera modes. The file is not necessary though and the game can be run without it.

### ONLOAD
Similar to init, you can setup loadconfig in such a way that a script will always be run on scene change just using `loadconfig <filename> -onload`. However, if you are going to set a txt in `-onload` that uses the `goto` command, remember to use`-clearonload` before the goto command in the same file or else you will enter an infinite loop!

# KNOWN ISSUES
*The following issues are minor and do not affect the vanilla experience of the game.*
- Setting hp, then using goto before entering a loading zone or saving resets hp; doesn't save
- Spawned enemies don't despawn through room transitions/reloads
- Teleporting enemies (Passel & Hexrots, but not Slayer Jennies) are unculled for a second after they teleport if you're in first person (does not happen in other camera modes, only first) (probably due to cull radius)
- Showhud does not re-enable speech bubble texts after having disabled them (disable hud without bubble on screen, go within range of bubble, then re-enable hud)
- If in Dream World and you set melee item to a lower level than what you entered with, when you leave a dungeon, it'll reset melee to the level you entered with. Does not happen with other items
- Setting dynamite fuse does not apply to already existing dynamite sticks
- Disabling level timers triggers explosion of already existing dynamite sticks, ignoring fuse time

# FUN FACTS
### TEST ROOM
There is a portal world inaccessible through normal gameplay. It contains a time telling crystal, a cannon, a bottomless pit, some movable blocks and switches that do nothing, and more!
To access it, use debugger and type goto deep19!

### LEVEL IV ITEMS
Force Wand, Dynamite, and Ice Ring all have level 4 upgrades that are unobtainable through normal gameplay.
- Force Wand IV shoots lightning and does more damage than any other weapon (69)!
- Dynamite IV comes with a shotgun blast which is EFCS without the charge time or recoil!
- Ice Ring IV does more damage. Nothing special about this :(

 ### SECRET CODES
- The debug menu can be opened up in vanilla game by pausing and while holding left control, press the following: up, down, up, down, left, right, left, right, space. This debug menu only has 3 commands: warpto (this was expanded by us in our mod), setentvar and setsavevar. These are more advanced commands.
- When creating a save file, there is a code to start a new game with all the main items at their highest possible upgrades. The code is: 2AbyssW/Thee as file name.

### DEFAULT TEXT STRINGS
It's good practice to specify a default string for a textbox to show if no text is given. In some cases, Ludo had fun with this ;)
- Default text for signs/speech bubbles: Hello world! What's up? It's a sign"
- Default text for cards: Hello world, what's up? Here's a tip for you: Get Out.
- Tippsie's text: My, my, mister designer. Look who forgot to define a line for me to say here. You know what this means, so let's make it quick - no anesthetics this time. (this can be triggered in game by pausing on the frame between when fall into void is triggered but before player's fall state is set)

### NAME CHANGES
Throughout development, it's not uncommon for developers to change the names of things. Here are the few cases where this happened with ID2:
- Sweetwater Coast was originally named Candy Coast
- Pepperpain Prairie was originally named Vitamin Hills
- None of the Dream World dungeons had names to begin with. Early on, their tentative names were based on what items they would have you use:
     - Wizardry Lab = DreamForce
     - Syncope = DreamDynamite
     - Antigram = DreamChain
     - Bottomless Tower = DreamIce
     - Quietus = DreamAll
- Similarly, the portal worlds also didn't have names to begin with, so they just followed the naming convention deep + # (eg. deep1, deep2, deep14, etc.). The exception being Remedy. Remedy was intentionally named deep19s, as to make it harder for a would-be hacker to hack into it (altho, you just add an s to end and you'll get it lol). deep19 leads to Test Room, see above!
- Monochromes were originally named spiders in early development. The type of Monochrome that spawns in overworld at night is Spider, spawned from a weather event named CaveSpiderSpawner. The Monochromes found in Dark Hypostyle have their proper name, so it seems spider was just a tentative name before Dark Hypostyle was made

### SPECIAL ENEMEY NAMES
- BunboyStationary (map: most of Pepperpain Prairie): The bunboys. Why stationary? We don't know. They can walk just fine.
- CyberJennyA, CyberJennyB, CyberJennyC, LennyA, LennyB, LeBiadloA, LeBiadloB (map: any main dungeon): The dungeon bosses.
- Turnip_Nukenip (map: Deep10): a bazooka turnip that throws projectiles in an arc, does a huge explosion
- Chillyroger2 (map: Deep15): Bigger, meaner Chillyroger
- OctopurrMoving (map: Deep13): Green tentacle that goes underground, throws globs of goo and does a slap attack
- Brutus2 (map: Deep8): Bigger, meaner Brutus
- Lemonjenny (map: LonelyRoad): A creepy Jenny

### MISCELLANEOUS
- The vanilla debug console contains 3 commands: `warpto` (`goto` drastically improved on this), `setentvar` (`setitem` does this), and `setsavevar` which updates variables in save file (a future command I write will make this better!). The debugger can be opened in vanilla game as well as our mod by doing the following code: `While paused, hold left control and press the following (in this order): up arrow, down arrow, up arrow, down arrow, left arrow, right arrow, left arrow, right arrow, space`
- Sunny weather in Pepperpain affects item drops from enemies. While effect lasts, it has an 80% chance to replace any heart drop from an enemy with a lightning drop!
- Art Exhibit's floor reflection is a camera trick. This second camera is a flipped version of the main one!
- The sound file used when Fishbuns walk is named Fishbun_Pain. Ludo confirmed to torture Fishbuns :monkaS:
- The NPC you meet at end of Syncope, his internal name is "DreamRescueNPCOsalig". Osalig is Swedish for "unsaved"
- Ittle says "crap"! :mjau:

# CONTRIBUTIONS
Thank you to everyone who makes use of this wonderful mod! I hope it serves you well! :D
If you would like to contribute or aid in development, feel free to join the [Discord server](https://discord.gg/qVjcfT2) for it!
We can always use an extra hand! If you know Unity and/or C#, or have experience with reverse engineering, you can help a lot!

# LEGAL DISCLAIMER
We are, in no way, claiming copyright ownership of this game or any of its content thereof. Copyright ownership belongs to Ludosity. A game mod, as long as it is not for commercial gain, falls under Section 107 of the Copyright Act as "fair use" due to a) this mod is for non-commercial use, b) this mod only modifies the game's code, it does not replace any visual content with that of our own or someone else's, c) we do not feel that we are negatively affecting Ludosity's business or income from this game or its content by distrubting this mod, as, if anything, it helps bring their fans together and often gets former players finding a new interest in this game, and d) we are not directly distributing Ludosity's copyrighted content; we are distributing a file that is already available to everyone, given they know where it is stored on their system.

Please take any legal inquieries up with `Chris is Awesome#9819` on Discord, or you can make an Issue of it here.

### Happy adventuring! :)
