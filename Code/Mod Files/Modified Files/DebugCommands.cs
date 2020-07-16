﻿//**************************************
//DebugCommands
//**************************************
//Synopsis:
//Class responsible for parsing and executing debugmenu commands. This script is not present in the
//vanilla game.
//
//Known issues:
//
//Possible new features:
//
//**************************************

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.IO;
using System.Linq;
using ModStuff;

public class DebugCommands : Singleton<DebugCommands>
{
	CommonFunctions comFuncs;
	public void Awake()
	{
		//Set name
		gameObject.name = "Debugger";
		// Enable Update function
		this.enabled = true;

		//Commands dictionary
		allComs = new Dictionary<string, DebugCommands.CommandFunc>
	    {
		    {"help", new DebugCommands.CommandFunc(CommandsList)},
		    {"likeaboss", new DebugCommands.CommandFunc(LikeABoss)},
		    {"spawn", new DebugCommands.CommandFunc(Spawn)},
		    {"test", new DebugCommands.CommandFunc(Test)},
            {"test2", new DebugCommands.CommandFunc(Test2)},
            {"find", new DebugCommands.CommandFunc(Find)},
		    {"bind", new DebugCommands.CommandFunc(SetKey)},
		    {"goto", new DebugCommands.CommandFunc(GoTo)},
		    {"knockback", new DebugCommands.CommandFunc(Knockback)},
		    {"zap", new DebugCommands.CommandFunc(Zap)},
		    {"createitem", new DebugCommands.CommandFunc(CreateItem)},
		    {"cam", new DebugCommands.CommandFunc(SetCam)},
		    {"setitem", new DebugCommands.CommandFunc(SetItem)},
		    {"atk", new DebugCommands.CommandFunc(AtkMod)},
		    {"pos", new DebugCommands.CommandFunc(SetPos)},
		    {"noclip", new DebugCommands.CommandFunc(NoClip)},
		    {"speed", new DebugCommands.CommandFunc(SetSpeed)},
		    {"size", new DebugCommands.CommandFunc(SetSize)},
		    {"time", new DebugCommands.CommandFunc(Time)},
		    {"sound", new DebugCommands.CommandFunc(PlaySound)},
		    {"hum", new DebugCommands.CommandFunc(Hum)},
		    {"hp", new DebugCommands.CommandFunc(SetHP)},
		    {"god", new DebugCommands.CommandFunc(God)},
		    {"showhud", new DebugCommands.CommandFunc(ShowHUD)},
		    {"loadconfig", new DebugCommands.CommandFunc(LoadConfig)},
		    {"echo", new DebugCommands.CommandFunc(Echo)},
		    {"bubble", new DebugCommands.CommandFunc(Bubble)},
		    {"stats", new DebugCommands.CommandFunc(Stats)},
		    {"clearprefs", new DebugCommands.CommandFunc(ClearPrefs)},
		    {"progress", new DebugCommands.CommandFunc(Progress)},
		    {"debug", new DebugCommands.CommandFunc(DebugView)},
		  {"secret", new DebugCommands.CommandFunc(SecretRewards)},
		  {"hnc", new DebugCommands.CommandFunc(SecretFindPlace)},
			{"outfit", new DebugCommands.CommandFunc(SetOutfit)},
			{"setflags", new DebugCommands.CommandFunc(SetFlags)},
			{"kill", new DebugCommands.CommandFunc(Kill)},
			{"invisible", new DebugCommands.CommandFunc(Invisible)},
			{"randomsounds", new DebugCommands.CommandFunc(RandomSounds) },
			//{"textureswap", new DebugCommands.CommandFunc(TextureSwap) },
			{"playsong", new DebugCommands.CommandFunc(PlaySong) },
			//{"modelswap", new DebugCommands.CommandFunc(ModelSwap) },
			{"weather", new DebugCommands.CommandFunc(Weather)},
			{"status", new DebugCommands.CommandFunc(SetStatus)},
		    //Ludo code
		    {"setentvar",new DebugCommands.CommandFunc(SetEntVar)},
		    {"setsavevar",new DebugCommands.CommandFunc(SetSaveVar)},
		    {"warpto",new DebugCommands.CommandFunc(WarpTo)},
		    {"clearsavevar",new DebugCommands.CommandFunc(ClearSaveVar)}
	    };

		//Hidden commands awake
		hiddenCommands = new List<string>
	   {
		  "secret",
		  "test",
	   };

		//Bind Awake
		bindComs = new Dictionary<KeyCode, string>();

		//GoTo Awake
		MakeMapsAndSpawns();

		//SetItem Awake
		itemNames = new Dictionary<string, int[]>()
	    {
		    {"dynamite",new int[] {0,4}},
		    {"melee",new int[] {0,3}},
		    {"icering",new int[] {0,4}},
		    {"forcewand",new int[] {0,4}},
		    {"raft",new int[] {0,8}},
		    {"evilKeys",new int[] {0,4}}, // Forbidden keys
			{"chain",new int[] {0,3}},
		    {"keys",new int[] {0,999}},// Lockpicks
			{"localKeys",new int[] {0,999}}, // Dungeon keys
			{"tome",new int[] {0,3}},
		    {"amulet",new int[] {0,3}},
		    {"headband",new int[] {0,3}},
		    {"tracker",new int[] {0,3}},
		    {"loot",new int[] {0,1}},
		    {"hpcs",new int[] {0,20}},
		    {"shards",new int[] {0,24}}
	    };

		comFuncs = GameObject.Find("CommonFunctions").GetComponent<CommonFunctions>();

		//Run init file
		LoadConfig(new string[] { "init" });
		OutputText("Welcome to the debug menu!");
	}

	public void OnLoadNewScene()
	{
		//Refresh references
		overlayCamera = GameObject.Find("OverlayCamera");
		player = GameObject.Find("PlayerEnt");
		wobblecam = GameObject.Find("Cameras").transform.parent.gameObject;
		//Fill spawner lists
		ModSpawner.Instance.FillSpawnerLists();

		// Check if anticheat to disable command persistance through main menu/loads
		if (ModSaver.LoadIntFromFile("mod", "anticheat") == 1) { DisableCommands(); return; }

		//Set camera mode
		if (fpsmode != 0) { FPSToggle(fpsmode); } else { CamConfig(); }
		//Atk modifiers
		if (superDynamite != false || dynamite_fuse != 1.5f || dynamite_radius != 1.7f) { SuperDynamite(); }
		if (superIce != false || iceOffgrid != false) { SuperIce(); }
		if (superAttack) { SuperAttack(); }
		if (projectile_count != 1) { SuperProjectiles(); }
		if (extra_bulge != 0f) { SuperRange(); }
		//Misc player modifiers
		if (move_speed != default_move_speed) { player.GetComponent<Moveable>()._moveSpeed = move_speed; player.transform.Find("Actions").GetComponent<RollAction>()._speed = move_speed; }
		if (godmode) { God(new string[] { "1" }); }
		noclip = false;
		if (isInvisible)
		{
			Invisible(new string[] { "" });
		}
		/*
		if (!string.IsNullOrEmpty(textureChangedTo))
		{
			TextureSwap(new string[] { textureChangedTo });
		}
		*/
		//Time flow
		if (savedTimeFlow != 4)  // If time flow was set
		{
			LevelTimeUpdater timeFlowData = GameObject.Find("BaseLevelData").transform.Find("TimeStuff").GetComponent<LevelTimeUpdater>();
			if (timeFlowData != null) { timeFlowData._hoursPerMinute = savedTimeFlow; }
		}
		//Size
		if (hasResizedSelf) // If has resized self, load custom size
		{
			player.transform.Find("Ittle").transform.localScale = resizeSelfScale;
			player.transform.Find("Hittable").GetComponent<Knockbackable>().RefreshKnockbackScale(player.GetComponent<Entity>());
		}
		if (hasResizedEnemies)   // If has resized enemies, load custom size
		{
			foreach (Entity ent in Resources.FindObjectsOfTypeAll<Entity>())
			{
				if (ent.name != "PlayerEnt")  // Do not include player
				{
					ent.transform.localScale = resizeEnemiesScale;
				}
			}
		}
		//Reset coroutine timer
		hudTextTimer = 0f;
        //Load init.txt. Always last!
        StartCoroutine(DelayInit());
    }

    //Hud Update function
    IEnumerator DelayInit()
    {
        yield return new WaitForSeconds(0.2f);
        string initFile = ModScriptHandler.Instance.OnNewSceneTxt;
        if (!string.IsNullOrEmpty(initFile)) { ModScriptHandler.Instance.ParseTxt(initFile, out string errors); }
    }

    public void DisableCommands()
	{
		likeABoss = false;
		godmode = false;
		noclip = false;
		knockback_multiplier = 5f;
		move_speed = 5f;
		superAttack = false;
		projectile_count = 1;
		extra_bulge = 0f;
		superDynamite = false;
		dynamite_fuse = 3f;
		dynamite_radius = 5f;
		superIce = false;
		timeflow = 4f;
		hasResizedEnemies = false;
		hasResizedSelf = false;
		timersEnabled = true;
		showHUD = true;
		isInvisible = false;
	}

	//-------------------------------------
	//Basic class variables and functions
	//-------------------------------------

	//References variables
	public DebugMenu debugMenu;
	public GameObject player;

	//Commands history dictionary
	public List<string> prevComs = new List<string>();

	//Commands dictionary
	Dictionary<string, DebugCommands.CommandFunc> allComs;

	//Delegate command function
	private delegate void CommandFunc(string[] args);

	//Command string parser
	public bool ParseResultString(string str, bool updateStats = true)
	{
		string[] array = str.Split(new char[] { ' ' });
		if (array.Length != 0)
		{
			string text = array[0];
			DebugCommands.CommandFunc commandFunc;
			if (allComs.TryGetValue(text.ToLower(), out commandFunc))
			{
				string[] array2 = new string[array.Length - 1];
				for (int i = 0; i < array2.Length; i++) { array2[i] = array[i + 1]; }
				try
				{
					commandFunc(array2);
				}
				catch (Exception ex)
				{
					OutputText("Error in " + text + ": " + ex.Message + "\n" + ex.StackTrace);
				}
				if (updateStats) { ModMaster.UpdateStats("ComsUsed"); }
				return true;
			}
			OutputText("Error: Unknown command " + text);
		}
		return false;
	}

	//Print debug text
	public void OutputText(string text)
	{
		if (debugMenu == null)
		{
			GameObject debug = GameObject.Find("Debug");
			if (debug == null) { return; }
			debugMenu = debug.GetComponent<DebugMenu>();
		}
		debugMenu.UpdateOutput(ModText.WrapText(text, 40f, false));
	}

    public void OutputText(string text, bool useWrap = true)
    {
        if (debugMenu == null)
        {
            GameObject debug = GameObject.Find("Debug");
            if (debug == null) { return; }
            debugMenu = debug.GetComponent<DebugMenu>();
        }
        if(useWrap) text = ModText.WrapText(text, 40f, false);
        debugMenu.UpdateOutput(text);
    }

    //ParseBool
    private bool ParseBool(string s, out bool result)
	{
		string arg_value = s.ToLower();
		result = false;
		if (arg_value == "on" || arg_value == "yes" || arg_value == "1" || arg_value == "true")
		{
			result = true;
			return true;
		}
		if (arg_value == "off" || arg_value == "no" || arg_value == "0" || arg_value == "false")
		{
			result = false;
			return true;
		}
		return false;
	}

	//ParseVector3 (for string array)
	private Boolean ParseVector3(string[] args, out Vector3 result, int index = 0)
	{
		result = Vector3.zero;

		if ((index + 3) > args.Length) { return false; }

		if (float.TryParse(args[index], out float x) && float.TryParse(args[index + 1], out float y) && float.TryParse(args[index + 2], out float z))
		{
			result = new Vector3(x, y, z);
			return true;
		}
		return false;
	}

	//ParseVector3 (for string)
	private Boolean ParseVector3(string args, out Vector3 result)
	{
		result = Vector3.zero;

		if (float.TryParse(args, out float x))
		{
			result = new Vector3(x, x, x);
			return true;
		}
		return false;
	}

	//AddLineJumps (move to debugmenu eventually, it doesnt detect line jump and doesn't work very well)
	private string AddLineJumps(string text, int letterPerLine = 65)
	{
		string[] words = text.Split(new char[] { ' ' });
		string output = "";
		string temp = "";
		for (int i = 0; i < words.Length; i++)
		{
			if ((temp + words[i]).Length > letterPerLine)
			{
				output += "\n";
				temp = words[i];
			}
			else
			{
				temp += words[i];
			}
			output += words[i] + " ";
		}
		return output;
	}

	//-------------------------------------
	//HUD text
	//-------------------------------------
	TextMesh hudText;
	AnimationCurve textScaleCurve;
	bool animateHudText;
	float hudTextTimer;
	float lastTextHudTimer;
	float hudTextUpdateTime = 0.03f;

	//Print hud text
	private void HudOutput(string output, bool animate = false, float timeOnScreen = 9999999f)
	{
		//If there is no hud element, create it
		if (hudText == null)
		{
			//Set anchor
			GameObject anchor = new GameObject("BindText_anchor");
			anchor.transform.parent = GameObject.Find("OverlayCamera").transform;
			anchor.transform.localPosition = new Vector3(-5f, -2.65f, 0f);
			anchor.transform.localScale = Vector3.one;
			anchor.transform.localEulerAngles = Vector3.zero;

			//Set text gameobject
			GameObject textGo = new GameObject("BindText");
			textGo.transform.parent = anchor.transform;
			textGo.transform.localPosition = Vector3.zero;
			textGo.transform.localEulerAngles = Vector3.zero;
			textGo.transform.localScale = Vector3.one;
			textGo.layer = 9; //UI layer

			//Set text
			hudText = textGo.AddComponent<TextMesh>();
			foreach (Font fo in Resources.FindObjectsOfTypeAll(typeof(Font)) as Font[])
			{
				if (fo.name == "Cutscene")
				{
					hudText.font = fo;
					textGo.GetComponent<UnityEngine.MeshRenderer>().sharedMaterial = FontMaterialMap.LookupFontMaterial(fo);
					break;
				}
			}
			hudText.alignment = TextAlignment.Left;
			hudText.color = Color.white;
			hudText.characterSize = 0.018f;
			hudText.fontSize = 100;

			//Set animation curve
			textScaleCurve = new AnimationCurve();
			textScaleCurve.preWrapMode = WrapMode.ClampForever;
			textScaleCurve.postWrapMode = WrapMode.ClampForever;
			textScaleCurve.AddKey(0f, 1f);
			textScaleCurve.AddKey(0.04f, 1.3f);
			textScaleCurve.AddKey(0.2f, 1f);
		}

		lastTextHudTimer = timeOnScreen;
		animateHudText = animate;
		hudText.text = output;

		if (hudTextTimer <= 0f)
		{
			hudTextTimer = timeOnScreen;
			StartCoroutine(UpdateHudOutput());
		}
		else
		{
			hudTextTimer = timeOnScreen;
		}
	}

	//Hud Update function
	IEnumerator UpdateHudOutput()
	{
		float evaluatedTextScale;

		while (hudTextTimer > 0f)
		{
			hudTextTimer -= hudTextUpdateTime;
			evaluatedTextScale = animateHudText ? textScaleCurve.Evaluate(lastTextHudTimer - hudTextTimer) : 1f;
			hudText.gameObject.transform.localScale = new Vector3(evaluatedTextScale, evaluatedTextScale, evaluatedTextScale);
			yield return new WaitForSeconds(hudTextUpdateTime);
		}
		hudText.text = "";
	}

	//-------------------------------------
	//GetArg functions
	//-------------------------------------

	//Find empty argument
	private bool GetArg(string key_arg, string[] args)
	{
		string target_arg = key_arg.ToLower();
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == target_arg) { return true; }
		}
		return false;
	}

	//Find float argument
	private bool GetArg(string key_arg, out float output, string[] args, out bool invalidValue)
	{
		string target_arg = key_arg.ToLower();
		bool result = false;
		output = 0f;
		invalidValue = false;

		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == target_arg)
			{
				if ((i + 1) < args.Length)
				{
					if (float.TryParse(args[i + 1], out float tempvalue))
					{
						output = tempvalue;
						result = true;
					}
				}
				invalidValue = true;
			}
		}
		if (result) { invalidValue = false; }
		return result;
	}

	//Find int argument
	private bool GetArg(string key_arg, out int output, string[] args, out bool invalidValue)
	{
		string target_arg = key_arg.ToLower();
		bool result = false;
		output = 0;
		invalidValue = false;

		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == target_arg)
			{
				if ((i + 1) < args.Length)
				{
					if (int.TryParse(args[i + 1], out int tempvalue))
					{
						output = tempvalue;
						result = true;
					}
				}
				invalidValue = true;
			}
		}
		if (result) { invalidValue = false; }
		return result;
	}

	//Find bool argument
	private bool GetArg(string key_arg, out bool output, string[] args, out bool invalidValue)
	{
		string target_arg = key_arg.ToLower();
		bool result = false;
		output = false;
		invalidValue = false;

		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == target_arg)
			{
				if ((i + 1) < args.Length)
				{
					if (ParseBool(args[i + 1], out bool tempvalue))
					{
						output = tempvalue;
						result = true;
					}
				}
				invalidValue = true;
			}
		}
		if (result) { invalidValue = false; }
		return result;
	}

	//Find string argument
	private bool GetArg(string key_arg, out string output, string[] args, out bool invalidValue)
	{
		string target_arg = key_arg.ToLower();
		bool result = false;
		output = "";
		invalidValue = false;

		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == target_arg)
			{
				invalidValue = true;
				if ((i + 1) < args.Length)
				{
					if (args[i + 1] != "")
					{
						output = args[i + 1];
						result = true;
					}
				}
			}
		}
		if (result) { invalidValue = false; }
		return result;
	}

	//Find Vector3 argument
	private bool GetArg(string key_arg, out Vector3 output, string[] args, out bool invalidValue)
	{
		string target_arg = key_arg.ToLower();
		bool result = false;
		output = Vector3.zero;
		invalidValue = false;

		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == target_arg)
			{
				if ((i + 3) < args.Length)
				{
					if (float.TryParse(args[i + 1], out float x) && float.TryParse(args[i + 2], out float y) && float.TryParse(args[i + 3], out float z))
					{
						output = new Vector3(x, y, z);
						result = true;
					}
				}
				invalidValue = true;
			}
		}
		if (result) { invalidValue = false; }
		return result;
	}

	//Find array argument (string output)
	private bool GetArg(string key_arg, int arrayLength, out string[] output, string[] args, out bool invalidValue)
	{
		output = new string[arrayLength];
		string target_arg = key_arg.ToLower();
		bool result = false;
		invalidValue = false;

		for (int i = 0; i < args.Length; i++)
		{
			//Check for key word
			if (args[i] != target_arg)
			{
				continue;
			}
			if ((i + arrayLength) < args.Length)
			{
				result = true;
				for (int j = 0; j < arrayLength; j++)
				{
					if (args[i + j + 1] != null) { output[j] = args[i + j + 1]; } else { result = false; }
				}
			}
			invalidValue = true;
		}
		if (result) { invalidValue = false; }
		return result;
	}

	//Find array elements argument (string return)
	private bool GetArg(string key_arg, string[] key_parameters, out string[] output, string[] args, out bool invalidValue)
	{
		output = new string[key_parameters.Length];
		string target_arg = key_arg.ToLower();
		bool result = false;
		invalidValue = false;

		for (int i = 0; i < args.Length; i++)
		{
			//Check for key word
			if (args[i] != target_arg)
			{
				continue;
			}
			//Change invalidValue to true, if a correct value was
			//found, invalidValue will be changed before returning
			invalidValue = true;

			//If there is no space for the second argumentand number, go to the next loop
			if ((i + 2) >= args.Length)
			{
				continue;
			}

			for (int j = 0; j < key_parameters.Length; j++)
			{
				if (args[i + 1] == key_parameters[j].ToLower() && args[i + 2] != "")
				{
					output[j] = args[i + 2];
					result = true;
				}
			}
		}
		if (result) { invalidValue = false; }
		return result;
	}

	//Find array elements argument (float return)
	private bool GetArg(string key_arg, string[] key_parameters, out float[] output, string[] args, out bool invalidValue)
	{
		output = new float[key_parameters.Length];
		string target_arg = key_arg.ToLower();
		bool result = false;
		invalidValue = false;

		for (int i = 0; i < args.Length; i++)
		{
			//Check for key word
			if (args[i] != target_arg)
			{
				continue;
			}
			//Change invalidValue to true, if a correct value was
			//found, invalidValue will be changed before returning
			invalidValue = true;

			//If there is no space for the second argumentand number, go to the next loop
			if ((i + 2) >= args.Length)
			{
				continue;
			}

			for (int j = 0; j < key_parameters.Length; j++)
			{
				if (args[i + 1] == key_parameters[j].ToLower() && float.TryParse(args[i + 2], out float floatvalue))
				{
					output[j] = floatvalue;
					result = true;
				}
			}
		}
		if (result) { invalidValue = false; }
		return result;
	}

	//-------------------------------------
	//Speech bubble
	//-------------------------------------
	private void Bubble(string[] args)
	{
		if (args.Length < 1)
		{
			OutputText("Error: 'bubble' requires text");
			return;
		}
		string text = args[0];
		for (int i = 1; i < args.Length; i++)
		{
			text += " " + args[i];
		}
		text = text.Replace("|", "\n");
		foreach (Sign sign in FindObjectsOfType<Sign>())
		{
			if ((player.transform.position - sign.gameObject.transform.position).sqrMagnitude > 5f)
			{
				continue;
			}
			sign.ChangeText(text);
		}
		OutputText("All nearby speech bubbles changed to:\n\n" + text);
	}

	//-------------------------------------
	//Play sound
	//-------------------------------------
	List<SoundClip> soundsInScene;
	int sound_menu_position;
	public int sound_menu_count = 25;
	private void PlaySound(string[] args)
	{
		string output = "";
		bool scrollList = true;

		//Attempt to play sound if -play was entered
		if (GetArg("-play", out string sound_string, args, out bool invalidValue))
		{
			float pitch = 1f;
			scrollList = false;

			//Check if the pitch must be changed
			if (GetArg("-pitch", out float float_value, args, out invalidValue))
			{
				pitch = float_value;
				output += "Pitch set to " + pitch + "\n";
			}
			if (invalidValue)
			{
				output += "Invalid pitch value, using default (1)\n";
			}
			sound_string = sound_string.ToLower();
			SoundClip toPlay = null;
			foreach (SoundClip sound in Resources.FindObjectsOfTypeAll(typeof(SoundClip)))
			{
				if (sound.RawSoundName.ToLower() == sound_string)
				{
					toPlay = sound;
				}
			}
			if (toPlay != null)
			{
				SoundPlayer.Instance.PlayPositionedSoundMod(toPlay, player.transform.position, pitch);
				output += "Playing " + toPlay.RawSoundName + "\n\n";
			}
			else
			{
				output += "Sound file not found!\n\n";
			}
		}
		else
		{
			//If no -play found, show the help text
			output = "Show a list of available sounds to play. Enter the command multiple times \n" +
				    "without -play to scroll the list.\n" +
				    "Modifiers: -play <string>: Play a sound\n" +
				    "-pitch <float>: Change the pitch of the sound\n\n";
		}

		string list = "";
		//If the list exists and -play was not used, scroll the list
		if (scrollList && soundsInScene != null)
		{
			sound_menu_position += sound_menu_count;
			if (sound_menu_position >= soundsInScene.Count) { soundsInScene = null; }
		}

		//If the sound list doesnt exist, create it
		if (soundsInScene == null)
		{
			soundsInScene = new List<SoundClip>();
			sound_menu_position = 0;
			foreach (SoundClip sound in Resources.FindObjectsOfTypeAll(typeof(SoundClip)))
			{
				soundsInScene.Add(sound);
			}
		}

		//Print sound list
		for (int i = sound_menu_position; i < sound_menu_position + sound_menu_count; i++)
		{
			if (i >= soundsInScene.Count) { break; }
			list += soundsInScene[i].RawSoundName + " ";
		}

		OutputText(output + AddLineJumps(list));
	}

	//Hum
	private void Hum(string[] args)
	{
		foreach (SoundClip sound in Resources.FindObjectsOfTypeAll(typeof(SoundClip)))
		{
			if (sound.RawSoundName == "VocIttle_EFCS_A")
			{
				SoundPlayer.Instance.PlayPositionedSoundMod(sound, player.transform.position, 1f);
				break;
			}
		}
	}

	//-------------------------------------
	//CommandsList
	//-------------------------------------
	private List<string> hiddenCommands;

	private void CommandsList(string[] args)
	{
		string output = "";
		foreach (KeyValuePair<string, DebugCommands.CommandFunc> command in allComs)
		{
			if (!hiddenCommands.Contains(command.Key)) { output += command.Key + " "; }
		}

		OutputText(ModStuff.ModMaster.GetDebugMenuHelp() + "List of available commands:\n\n" + AddLineJumps(output, 60));
	}

	//-------------------------------------
	//Find
	//-------------------------------------
	private GameObject savedFind;
	private void Find(string[] args)
	{
		//If no gameobject name was given, exit
		if (args.Length < 1)
		{
			OutputText(
			"Error: 'find' requires at least one parameter. Return the closest gameobject with the chosen name\n" +
			"Parameters: gameobject(string)\n\n" +
			"Modifiers:\n" +
			"-all: Include inactive objects in the search\n" +
			"-parent <string>: Only show objects which have a parent in its hierarchy containing <string> in their name\n" +
			"-parent2 <string>: Only show objects which their direct parents contain <string> in their name\n" +
			"-noparent: Only show objects which don't have a parent\n" +
			"-i <int>: Choose which object from the search to show (from <1> to <ammount found>)\n" +
			"-t: Show transform of the object (parent, children, position, rotation and scale)\n" +
			"-children: List ALL the children of the object\n" +
			"-c: Show components of the object\n" +
			"-save: Save a reference to the found object\n" +
			"-load: Use the saved object instead of doing a search\n" +
			"-p/-gp/-r/-gr/-s (vector3 or x/y/z float): Change local position/global position/local rotation/global rotation\n" +
			"or scale of the object. Use x/y/z followed by a float to change just one component\n" +
			"-changep: Change the parent of the found object to the one saved with -save\n" +
		  "-activate <(optional)bool>: Sets the gameobject to active or inactive\n" +
		  "-noui: Don't draw the overlay");
			return;
		}
		//GameObject to find variables
		string go_target = args[0].ToLower();
		GameObject goFound = null;

		//Error and output tracking variables
		bool invalidValue;
		string output = "";
		List<GameObject> goMatches = new List<GameObject>();
		int index;

		if (!GetArg("-load", args))
		{
			bool useInactiveGo = false;
			int activeGoFound = 0;

			//Check if all gameobjects should be listed
			if (GetArg("-all", args))
			{
				useInactiveGo = true;
			}

			//Search for name
			foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
			{
				if (go.name.ToLower().Contains(go_target))
				{
					if (go.activeInHierarchy)
					{
						goMatches.Add(go);
						activeGoFound++;
					}
					else if (useInactiveGo)
					{
						goMatches.Add(go);
					}
				}
			}

			//Check for no parents
			bool noParents = false;
			if (GetArg("-noparent", args))
			{
				activeGoFound = 0;
				noParents = true;
				List<GameObject> templist = new List<GameObject>();

				foreach (GameObject go in goMatches)
				{
					if (go.transform.parent == null)
					{
						if (go.activeInHierarchy) { activeGoFound++; }
						templist.Add(go);
					}
				}
				goMatches = templist;
			}

			//Check for any parent with parent name
			if (GetArg("-parent", out string targetparent, args, out invalidValue))
			{
				activeGoFound = 0;
				List<GameObject> templist = new List<GameObject>();
				foreach (GameObject go in goMatches)
				{
					Transform currentParent = go.transform;
					while (currentParent.parent != null)
					{
						if (currentParent.parent.name.ToLower().Contains(targetparent.ToLower()))
						{
							if (go.activeInHierarchy) { activeGoFound++; }
							templist.Add(go);
							break;
						}
						currentParent = currentParent.parent;
					}
				}
				goMatches = templist;
			}
			if (invalidValue)
			{
				OutputText("Error: invalid parent string");
				return;
			}

			//Check for parent
			if (GetArg("-parent2", out string targetparent2, args, out invalidValue))
			{
				activeGoFound = 0;
				List<GameObject> templist = new List<GameObject>();
				foreach (GameObject go in goMatches)
				{
					if (go.transform.parent && go.transform.parent.name.ToLower().Contains(targetparent2.ToLower()))
					{
						if (go.activeInHierarchy) { activeGoFound++; }
						templist.Add(go);
					}
				}
				goMatches = templist;
			}
			if (invalidValue)
			{
				OutputText("Error: invalid parent2 string");
				return;
			}


			//If no match was found, exit
			if (goMatches.Count == 0)
			{
				output += args[0] + " not found. ";
				if (targetparent2 != "") { output += "Search narrowed with '" + targetparent2 + "' as direct parent. "; }
				else if (targetparent != "") { output += "Search narrowed with '" + targetparent + "' as parent. "; }
				else if (noParents) { output += "Search narrowed to objects without parents. "; }

				OutputText(AddLineJumps(output));
				return;
			}

			//Choose a gameobject to show by index number
			if (GetArg("-i", out index, args, out invalidValue))
			{
				index = Mathf.Clamp(index, 1, goMatches.Count);
			}
			else
			{
				Vector3 playerpos = (player != null ? player.transform.position : Vector3.zero);
				float minDistToPlayer = Mathf.Infinity;
				for (int i = 0; i < goMatches.Count; i++)
				{
					float distToPlayer = (goMatches[i].transform.position - playerpos).sqrMagnitude;
					if (distToPlayer < minDistToPlayer)
					{
						index = i + 1;
						minDistToPlayer = distToPlayer;
					}
				}
			}
			if (invalidValue)
			{
				OutputText("Error: invalid index value");
				return;
			}

			goFound = goMatches[index - 1];

			//Give count
			output += "'" + go_target + "' found! ";
			if (targetparent2 != "") { output += " Search narrowed with '" + targetparent2 + "' as direct parent. "; }
			else if (targetparent != "") { output += " Search narrowed with '" + targetparent + "' as parent. "; }
			else if (noParents) { output += "Search narrowed to objects without parents. "; }

			if (goMatches.Count > 1)
			{
				output += "Result " + index + " of " + goMatches.Count + ". ";
				if (useInactiveGo) { output += activeGoFound + " of them active objects. "; }
			}

			output = AddLineJumps(output) + "\n\n";

		}
		else
		{
			if (savedFind == null)
			{
				OutputText("Error: Saved gameobject not found");
				return;
			}
			goFound = savedFind;
			output += "GameObject reference loaded.\n\n";
			goMatches.Add(goFound);
			index = 1;
		}
		if (!GetArg("-noui", args)) { MakeCommUI(goMatches.ToArray(), index - 1); }
		//Show name of gameobject and if it is active
		output += "Name: " + goFound.name;
		if (!goFound.activeInHierarchy) { output += ". The object is inactive."; }
		output += "\n\n";

		if (GetArg("-children", args))
		{
			string childList = "";
			// For each child
			foreach (Transform trans in goFound.transform.GetComponentsInChildren<Transform>())
			{
				childList += trans.gameObject.name + " ";
			}
			if (childList == "") { childList = "None found"; }
			output += AddLineJumps("ALL Children: " + childList + "\n\n");
		}

		//Display transform
		if (GetArg("-t", args))
		{
			//Show hierarchy of gameobject
			Transform currentParent2;
			string parentsString = goFound.name;

			currentParent2 = goFound.transform;
			while (currentParent2.parent != null)
			{
				parentsString = currentParent2.parent.gameObject.name + " / " + parentsString;
				currentParent2 = currentParent2.parent;
			}

			output += "Layer: " + goFound.layer.ToString() + "\n";

			output += AddLineJumps("Hierarchy: " + parentsString) + "\n";

			//Show children of gameobject
			string children = "";
			foreach (Transform child in goFound.transform)
			{
				children += child.name + " ";
			}
			if (children.Length == 0)
			{
				output += "Children: -\n";
			}
			else
			{
				output += AddLineJumps("Children: " + children) + "\n\n";
			}

			output += "Local position: " + goFound.transform.localPosition.ToString() + " / Global position: " + goFound.transform.position.ToString() + "\n";
			output += "Local rotation: " + goFound.transform.localRotation.eulerAngles.ToString() + " / Global rotation: " + goFound.transform.rotation.eulerAngles.ToString() + "\n";
			output += "Local scale: " + goFound.transform.localScale.ToString() + "\n\n";
		}

		//Display components
		if (GetArg("-c", args))
		{
			string componentlist = "";
			foreach (Component comp in goFound.GetComponents(typeof(Component)))
			{
				componentlist += comp.GetType().ToString() + " ";
			}
			if (componentlist == "") { componentlist = "-"; }
			output += AddLineJumps("Components: " + componentlist + "\n\n");
		}

		//Change parent
		if (GetArg("-changep", args))
		{
			if (savedFind == null) { output += "No gameobject saved to use as parent\n"; }
			else if (savedFind == goFound) { output += "You cannot parent an object to itself!\n"; }
			else
			{
				goFound.transform.SetParent(savedFind.transform);
				output += goFound.name + " parented to " + savedFind.name + "\n";
			}
		}

		//Set position
		Vector3 tempvector;
		string[] stringoutput;
		float floatValue;
		if (GetArg("-p", out tempvector, args, out invalidValue))
		{
			goFound.transform.localPosition = tempvector;
			output += "Changed local position to " + tempvector.ToString() + "\n";
		}
		else if (GetArg("-p", new string[] { "x", "y", "z" }, out stringoutput, args, out invalidValue))
		{
			tempvector = goFound.transform.localPosition;
			if (float.TryParse(stringoutput[0], out floatValue)) { tempvector.x = floatValue; }
			if (float.TryParse(stringoutput[1], out floatValue)) { tempvector.y = floatValue; }
			if (float.TryParse(stringoutput[2], out floatValue)) { tempvector.z = floatValue; }
			goFound.transform.localPosition = tempvector;
			output += "Changed local position to " + tempvector.ToString() + "\n";
		}
		if (invalidValue) { output += "Invalid local position value\n"; }

		if (GetArg("-gp", out tempvector, args, out invalidValue))
		{
			goFound.transform.position = tempvector;
			output += "Changed global position to " + tempvector.ToString() + "\n";
		}
		else if (GetArg("-gp", new string[] { "x", "y", "z" }, out stringoutput, args, out invalidValue))
		{
			tempvector = goFound.transform.position;
			if (float.TryParse(stringoutput[0], out floatValue)) { tempvector.x = floatValue; }
			if (float.TryParse(stringoutput[1], out floatValue)) { tempvector.y = floatValue; }
			if (float.TryParse(stringoutput[2], out floatValue)) { tempvector.z = floatValue; }
			goFound.transform.position = tempvector;
			output += "Changed local position to " + tempvector.ToString() + "\n";
		}
		if (invalidValue) { output += "Invalid global position value\n"; }

		//Set rotation
		if (GetArg("-r", out tempvector, args, out invalidValue))
		{
			goFound.transform.localEulerAngles = tempvector;
			output += "Changed local rotation to " + tempvector.ToString() + "\n";
		}
		else if (GetArg("-r", new string[] { "x", "y", "z" }, out stringoutput, args, out invalidValue))
		{
			tempvector = goFound.transform.localEulerAngles;
			if (float.TryParse(stringoutput[0], out floatValue)) { tempvector.x = floatValue; }
			if (float.TryParse(stringoutput[1], out floatValue)) { tempvector.y = floatValue; }
			if (float.TryParse(stringoutput[2], out floatValue)) { tempvector.z = floatValue; }
			goFound.transform.localEulerAngles = tempvector;
			output += "Changed local rotation to " + tempvector.ToString() + "\n";
		}
		if (invalidValue) { output += "Invalid local rotation value\n"; }

		if (GetArg("-gr", out tempvector, args, out invalidValue))
		{
			goFound.transform.eulerAngles = tempvector;
			output += "Changed local rotation to " + tempvector.ToString() + "\n";
		}
		else if (GetArg("-gr", new string[] { "x", "y", "z" }, out stringoutput, args, out invalidValue))
		{
			tempvector = goFound.transform.eulerAngles;
			if (float.TryParse(stringoutput[0], out floatValue)) { tempvector.x = floatValue; }
			if (float.TryParse(stringoutput[1], out floatValue)) { tempvector.y = floatValue; }
			if (float.TryParse(stringoutput[2], out floatValue)) { tempvector.z = floatValue; }
			goFound.transform.eulerAngles = tempvector;
			output += "Changed local position to " + tempvector.ToString() + "\n";
		}
		if (invalidValue) { output += "Invalid global rotation value\n"; }

		//Set scale
		if (GetArg("-s", out tempvector, args, out invalidValue))
		{
			goFound.transform.localScale = tempvector;
			output += "Changed scale to " + tempvector.ToString() + "\n"; ;
		}
		else if (GetArg("-s", out float tempscale, args, out invalidValue))
		{
			tempvector = new Vector3(tempscale, tempscale, tempscale);
			goFound.transform.localScale = tempvector;
			output += "Changed scale to " + tempvector.ToString() + "\n"; ;
		}
		else if (GetArg("-s", new string[] { "x", "y", "z" }, out stringoutput, args, out invalidValue))
		{
			tempvector = goFound.transform.localScale;
			if (float.TryParse(stringoutput[0], out floatValue)) { tempvector.x = floatValue; }
			if (float.TryParse(stringoutput[1], out floatValue)) { tempvector.y = floatValue; }
			if (float.TryParse(stringoutput[2], out floatValue)) { tempvector.z = floatValue; }
			goFound.transform.localScale = tempvector;
			output += "Changed local position to " + tempvector.ToString() + "\n";
		}
		if (invalidValue) { output += "Invalid local scale value\n"; }

		//Set active/inactive
		if (GetArg("-activate", out bool newState, args, out invalidValue))
		{
			goFound.SetActive(newState);
			output += goFound.activeSelf ? "GameObject is now active\n" : "GameObject is now inactive\n";
		}
		if (invalidValue)
		{
			goFound.SetActive(!goFound.activeSelf);
			output += goFound.activeSelf ? "GameObject is now active\n" : "GameObject is now inactive\n";
		}

		if (GetArg("-save", args))
		{
			output += "Object reference saved.";
			savedFind = goFound;
		}

		OutputText(output);
	}

	//Find command graphics
	List<GameObject> commGraphics;
	void MakeCommUI(GameObject[] goArray, int selected)
	{
		if (commGraphics != null)
		{
			foreach (GameObject go in commGraphics)
			{
				Destroy(go);
			}
		}
		commGraphics = new List<GameObject>();
		Camera cam = wobblecam.transform.Find("Cameras").Find("OutlineCamera").GetComponent<Camera>();
		for (int i = 0; i < goArray.Length; i++)
		{
			GameObject go = new GameObject("FindCommUI");
			FindCommGraphic uiElement = (FindCommGraphic)go.AddComponent<FindCommGraphic>();
			uiElement.Init(goArray[i], cam, i == selected);
			commGraphics.Add(go);
		}
	}

	//-------------------------------------
	//SetItem
	//-------------------------------------
	private Dictionary<string, int[]> itemNames;
	public void SetItem(string[] args)
	{
		string output = "";
		if (args.Length < 1)
		{
			foreach (KeyValuePair<string, int[]> item in itemNames)
			{
				output += item.Key + " ";
			}
			OutputText("Error: 'setitem' requires an item name and level value\n" +
					 "Use 'setitem dev' to equip dev weapons and 'setitem all' to get all equipable items\n" +
					 "Example: 'setitem melee 1' 'setitem dev'\n\n" +
					 "List of settable items:\n\n" + AddLineJumps(output));
			return;
		}
		Entity ent = player.GetComponent<Entity>();
		if (ent == null)
		{
			OutputText("Error: Player entity not found!");
			return;
		}
		string itemkey = args[0].ToLower();
		if (itemkey == "dev")
		{
			ModSaver.SaveToEnt("melee", 3);
			ModSaver.SaveToEnt("forcewand", 4);
			ModSaver.SaveToEnt("dynamite", 4);
			ModSaver.SaveToEnt("icering", 4);
			ModSaver.SaveToEnt("tracker", 3);
			ModSaver.SaveToEnt("amulet", 3);
			ModSaver.SaveToEnt("tome", 3);
			ModSaver.SaveToEnt("headband", 3);
			ModSaver.SaveToEnt("chain", 3);
			ModSaver.SaveToEnt("keys", 12);
			ModSaver.SaveToEnt("shards", 24);
			ModSaver.SaveToEnt("evilKeys", 4);
			ModSaver.SaveToEnt("raft", 8);

			ModSaver.SaveIntToFile("mod", "hasSetItems", 1);
			OutputText("You acquired the dev arsenal + all items!");
			return;
		}
		if (itemkey == "all")
		{
			ModSaver.SaveToEnt("melee", 3);
			ModSaver.SaveToEnt("forcewand", 3);
			ModSaver.SaveToEnt("dynamite", 3);
			ModSaver.SaveToEnt("icering", 3);
			ModSaver.SaveToEnt("tracker", 3);
			ModSaver.SaveToEnt("amulet", 3);
			ModSaver.SaveToEnt("tome", 3);
			ModSaver.SaveToEnt("headband", 3);
			ModSaver.SaveToEnt("chain", 3);
			ModSaver.SaveToEnt("keys", 12);
			ModSaver.SaveToEnt("shards", 24);
			ModSaver.SaveToEnt("evilKeys", 4);
			ModSaver.SaveToEnt("raft", 8);

			ModSaver.SaveIntToFile("mod", "hasSetItems", 1);
			OutputText("Ittle is fully armed!");
			return;
		}
		if (itemkey == "none")
		{
			ModSaver.SaveToEnt("melee", 0);
			ModSaver.SaveToEnt("forcewand", 0);
			ModSaver.SaveToEnt("dynamite", 0);
			ModSaver.SaveToEnt("icering", 0);
			ModSaver.SaveToEnt("tracker", 0);
			ModSaver.SaveToEnt("amulet", 0);
			ModSaver.SaveToEnt("tome", 0);
			ModSaver.SaveToEnt("headband", 0);
			ModSaver.SaveToEnt("chain", 0);
			ModSaver.SaveToEnt("keys", 0);
			ModSaver.SaveToEnt("shards", 0);
			ModSaver.SaveToEnt("evilKeys", 0);
			ModSaver.SaveToEnt("raft", 0);

			ModSaver.SaveIntToFile("mod", "hasSetItems", 1);
			OutputText("Removed all items");
			return;
		}
		if (args.Length < 2)
		{
			OutputText("Error: 'setitem' requires an item level value\n");
			return;
		}
		if (!int.TryParse(args[1], out int target_value))
		{
			OutputText("Error: 'setitem' only accepts integers");
			return;
		}
		if (itemkey == "localkeys") { itemkey = "localKeys"; }
		if (itemkey == "evilkeys") { itemkey = "evilKeys"; }
		if (itemNames.TryGetValue(itemkey, out int[] minmaxvalues))
		{

			if (itemkey == "loot" && ent.GetStateVariable("shards") != 24)
			{
				OutputText("Error: Cannot set loot to 1 without 24 shards");
				return;
			}
			int new_value = Mathf.Clamp(target_value, minmaxvalues[0], minmaxvalues[1]);
			int old_value = ent.GetStateVariable(itemkey);
			ModSaver.SaveToEnt(itemkey, new_value);
			ModSaver.SaveIntToFile("mod", "hasSetItems", 1);
			OutputText(itemkey + " changed from " + old_value + " to " + new_value);
			return;
		}
		else
		{
			foreach (KeyValuePair<string, int[]> item in itemNames)
			{
				output += item.Key + " ";
			}
			OutputText("Error: " + itemkey + " is not a valid item name\n\n" +
					 "List of settable items:\n\n" + AddLineJumps(output));
		}
	}

	//-------------------------------------
	//Knockback
	//-------------------------------------
	public float knockback_multiplier = 1f;
	public void Knockback(string[] args)
	{
		if (args.Length < 1)
		{
			OutputText("Setting knockback to default (1)");
			knockback_multiplier = 1f;
			ModSaver.SaveFloatToPrefs("knockbackMultiplier", knockback_multiplier);
		}
		else if (float.TryParse(args[0], out float value))
		{
			float old_value = knockback_multiplier;
			knockback_multiplier = value;
			ModSaver.SaveFloatToPrefs("knockbackMultiplier", value);
			OutputText("Setting knockback from " + old_value + " to " + knockback_multiplier);
		}
		else
		{
			OutputText("Error: Invalid float value");
		}
	}

	//-------------------------------------
	//Secrets!
	//-------------------------------------
	private void SecretRewards(string[] args)
	{
		string currMap = SceneManager.GetActiveScene().name;
		if (args.Length < 1)
		{
			SecretAnnoying();
			return;
		}
		else
		{
			switch (args[0])
			{
				case "incognito":
					OutputText("Entering incognito mode");
					break;
				case "youareveryannoying":
					bighead = !bighead;
					OutputText("You think yourself so smart with that big head of yours, huh?");
					break;
				default:
					SecretAnnoying();
					break;
			}
		}
	}

	//-------------------------------------
	//Secret being annoying
	//-------------------------------------
	int secretAnnoyingCount;
	bool bighead;
	Transform ittle_head;
	private void SecretAnnoying()
	{
		string output;
		switch (secretAnnoyingCount)
		{
			case 0:
				output = "Error: That is not a command";
				break;
			case 1:
				output = "Error: That isn't a command";
				break;
			case 2:
				output = "Error: Secret is not a command";
				break;
			case 6:
				output = "Error: Nothing to see here, go play with something else";
				break;
			case 10:
				output = "Error: Ok, technically, 'secret' is a command, but it does nothing";
				break;
			case 14:
				output = "Error: Yes, you were right, this is a command. Happy? Please go away";
				break;
			case 18:
				output = "Fine, do you want a prize? Is that what you are looking for?\n\n" +
					    "                          secret youareveryannoying\n\n" +
					    "Now scram";
				secretAnnoyingCount = -1;
				break;
			default:
				output = "Error";
				break;
		}
		secretAnnoyingCount++;
		OutputText(output);
	}

	//-------------------------------------
	//Secret treasure search
	//-------------------------------------
	private void SecretFindPlace(string[] args)
	{
		string currMap = SceneManager.GetActiveScene().name;
		string output = "\n\n\n\n\n";
		if (currMap == "VitaminHills")
		{
			Vector3 treasurePoint = new Vector3(55.7f, 0f, -3.0f);
			float distanceToTreasure = (player.transform.localPosition - treasurePoint).sqrMagnitude;
			if (distanceToTreasure > 3000f) { output += "Very very cold"; }
			else if (distanceToTreasure > 1500f) { output += "Very cold"; }
			else if (distanceToTreasure > 1000f) { output += "Cold"; }
			else if (distanceToTreasure > 650f) { output += "Tepid"; }
			else if (distanceToTreasure > 400f) { output += "Warm"; }
			else if (distanceToTreasure > 200f) { output += "Hot"; }
			else if (distanceToTreasure > 50f) { output += "Hot! HOT!"; }
			else if (distanceToTreasure > 2.5f) { output += "TOO HOT! YOU ARE BURNING!"; }
			else { output += "                                  secret incognito"; }
		}
		else
		{
			output += "You are not even in the correct part of the island! You are freezing!";
		}
		OutputText(output);
	}

	//Hud Update function
	IEnumerator ShowDistance()
	{
		while (true)
		{
			if (player != null)
			{
				float distanceToTreasure = (player.transform.localPosition - new Vector3(55.7f, 0f, -3.0f)).sqrMagnitude;
				HudOutput(distanceToTreasure.ToString());
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	//-------------------------------------
	//Like a boss
	//-------------------------------------
	public bool likeABoss;
	public float likeABossDmg = 10000f;

	public void LikeABoss(string[] args)
	{
		// If args are given, convert to bool
		if (args.Length > 0)
		{
			if (ParseBool(args[0], out bool result)) { likeABoss = result; }
		}
		else { likeABoss = !likeABoss; }

		// If enabled
		if (likeABoss) { OutputText("One Punch-Girl mode enabled!"); }
		else { OutputText("Back to being a wimp..."); }
	}

	//-------------------------------------
	//Spawner
	//-------------------------------------

	//Spawn command
	private void Spawn(string[] args)
	{
		//If no entity was entered, exit
		if (args.Length < 1)
		{
			OutputText(
			"Error: 'spawn' requires an NPC (string) and ammount (int, optional). Spawns NPCs on top of ittle facing a random direction.\n\n" +
			"IMPORTANT: Use '-list' to list all available NPCs for spawning (Enter 'spawn -list'). If you want to save an NPC out of its\n" +
			"original scene, use '-hold' to save it in memory.\n\n" +
			"Modifiers: \n" +
			"-list: Display all NPCs available for spawning in the scene\n" +
			"-p <Vector3>: Spawn position\n" +
			"-s <float or Vector3>: Scale of NPC (less than 1 for smaller, more than 1 for bigger)\n" +
			"-r <float>: Spawn NPCs facing <float> angle instead of a random direction (angle goes from 0 -facing top- to 360)\n" +
			"-globalpos: Use global position coordinates instead of relative to ittle\n" +
			"-ai <string>: Replace the normal Ai controller for one of another NPC. Enter 'null' to spawn without AI\n" +
			"-hpx <float>: Multiply the HP of the NPC by <float>\n" +
			"-hp <float>: Set the HP of the NPC\n" +
			"-ittlerot: Spawn NPCs facing the same rotation than ittle instead of a random one\n" +
			"-randompos <float>: Spawn NPCs around the target position with a maximum distance of <float>\n" +
			"-infront <float>: Spawn NPCs in front of ittle, <float> away\n" +
			"-hold: Save the NPC in memory instead of spawning it. Makes it possible to spawn the NPC out of the scene\n\n" +
			"Example: 'spawn Fishbun'\n" +
			"Example: 'spawn Fishbun 5'\n" +
			"Example: 'spawn Fishbun 2 -s 2 2 5 -ittlerot'");
			return;
		}

		ModSpawner.SpawnProperties spawnProperties = new ModSpawner.SpawnProperties();
		string spawn_Type;
		int spawn_Ammount;

		//Check if intuitive mode is used, check spawn_type and ammount
		if (int.TryParse(args[0], out spawn_Ammount) && args.Length > 1)
		{
			spawn_Type = args[1].ToLower();
		}
		else
		{
			spawn_Type = args[0].ToLower();
			if (args.Length > 1)
			{
				int.TryParse(args[1], out spawn_Ammount);
			}
		}
		if (spawn_Ammount < 1) spawn_Ammount = 1;
		spawnProperties.amount = spawn_Ammount;

		//If the entity is PlayerEnt, exit
		if (spawn_Type == "playerent")
		{
			OutputText("Error: 'PlayerEnt' cannot be spawned!");
			return;
		}
		spawnProperties.npcName = spawn_Type;

		//Variables for error tracking
		bool invalidValue;
		string output = "";
		bool allowspawn = true;

		//If -list is the first word, don't spawn
		if (spawn_Type == "-list") { allowspawn = false; }

		//Check for spawn position
		GetArg("-p", out Vector3 spawn_Position, args, out invalidValue);
		if (invalidValue)
		{
			output += "Error: Invalid position value\n";
			allowspawn = false;
		}

		//Check for global position
		if (!GetArg("-globalpos", args)) { spawn_Position += player.transform.localPosition; }

		//Check for in front of ittle
		if (GetArg("-infront", out float spawnDistance, args, out invalidValue))
		{
			spawn_Position = spawn_Position + player.transform.localRotation * Vector3.forward * spawnDistance;
		}
		if (invalidValue)
		{
			output += "Error: Invalid distance value\n";
			allowspawn = false;
		}

		//Check for random position
		if (GetArg("-randompos", out float posDistance, args, out invalidValue))
		{
			spawnProperties.aroundPoint = true;
			spawnProperties.distanceFromPoint = posDistance;
		}
		if (invalidValue)
		{
			output += "Error: Invalid distance value\n";
			allowspawn = false;
		}
		spawnProperties.fixedPosition = spawn_Position;

		//Spawn rotation and random facing direction
		Boolean randomdir = true;

		//Check for ittle rotation
		if (GetArg("-ittlerot", args))
		{
			randomdir = false;
            spawnProperties.UsePlayerRotation();

        }
		//Check for user rotation
		if (GetArg("-r", out float spawn_Rotation_F, args, out invalidValue))
		{
            randomdir = false;
            spawnProperties.fixedRotation = spawn_Rotation_F;
		}
		if (invalidValue)
		{
			output += "Error: Invalid rotation value\n";
			allowspawn = false;
		}
		spawnProperties.useRandomRotation = randomdir;

		//Spawn scale
		//Check for vector scale
		if (GetArg("-s", out Vector3 spawn_Scale, args, out invalidValue)) { }
		//Check for float scale
		else if (GetArg("-s", out float spawn_Scale_F, args, out invalidValue)) { spawn_Scale = new Vector3(spawn_Scale_F, spawn_Scale_F, spawn_Scale_F); }
		//If no custom scale found, use default scale
		else { spawn_Scale = Vector3.one; }
		if (invalidValue)
		{
			output += "Error: Invalid scale value\n";
			allowspawn = false;
		}
		spawnProperties.scale = spawn_Scale;

		//Check for AI
		if (!GetArg("-ai", out string spawn_Ai, args, out invalidValue)) { spawn_Ai = spawn_Type; }
		if (invalidValue)
		{
			output += "Error: Invalid AIcontroller\n";
			allowspawn = false;
		}
		spawnProperties.ai = spawn_Ai;

		//Check for max hp
		if (!GetArg("-hp", out float spawn_Hp, args, out invalidValue)) { spawn_Hp = 0f; }
		if (invalidValue)
		{
			output += "Error: Invalid HP value\n";
			allowspawn = false;
		}

		//Check for max hp multiplier
		if (!GetArg("-hpx", out float spawn_Hpx, args, out invalidValue)) { spawn_Hpx = 1f; }
		if (invalidValue)
		{
			output += "Error: Invalid HP multiplier\n";
			allowspawn = false;
		}
		if (spawn_Hp != 0f || spawn_Hpx != 1f) { spawnProperties.ConfigureHP(spawn_Hp, spawn_Hpx); }

		//Hold NPC and its AIcontroller in memory
		if (GetArg("-hold", args))
		{
			allowspawn = false;
            output += ModSpawner.Instance.HoldNPC(spawn_Type, spawn_Ai);
		}

		//List NPCs
		if (GetArg("-list", args))
		{
			if (output != "") { output += "\n"; }
			output += "Available NPCs to spawn:\n\n" + ModSpawner.Instance.NPCList();
		}

		//If no errors were found, spawn NPCs
		if (allowspawn) { output += ModSpawner.Instance.SpawnNPC(spawnProperties); }

		OutputText(output);
	}

	//-------------------------------------
	//God
	//-------------------------------------
	public bool godmode;
	public void God(string[] args)
	{
		if (args.Length > 0)
		{
			if (ParseBool(args[0], out bool result))
			{
				godmode = result;
			}
		}
		else
		{
			godmode = !godmode;
		}

		//Add hit invulnerability
		EntityHittable component = player.transform.Find("Hittable").gameObject.GetComponent<EntityHittable>();
		component.Disable = godmode;

		//Add enviro invulnerability
		Envirodeathable envirodeathable = player.GetComponent<Envirodeathable>();
		if (godmode)
		{
			if (envirodeathable != null)
			{
				Destroy(envirodeathable);
			}
		}
		else
		{
			if (envirodeathable == null)
			{
				EntityEnvirodeathable enviro = player.transform.Find("Envirodeath").GetComponent<EntityEnvirodeathable>();
				enviro.Enable(player.GetComponent<Entity>());
			}
		}

		if (godmode)
		{
			Killable entityComponent = player.GetComponent<Entity>().GetEntityComponent<Killable>();
			if (entityComponent != null) { entityComponent.CurrentHp = entityComponent.MaxHp; }
			OutputText("God mode ENGAGED!");
			return;
		}

		OutputText("God mode disabled");
	}

	//-------------------------------------
	//SetKey
	//-------------------------------------
	private Dictionary<KeyCode, string> bindComs;
	//SetKey command
	private void SetKey(string[] args)
	{
		if (args.Length == 0)
		{
			OutputText("Error: bind command requires more arguments:\n" +
					 "bind <Keycode> <command>: Save <command> to key <Keycode>\n" +
					 "bind last <Keycode>: Save the last command used to key <Keycode>\n" +
					 //"bind debug\n"
					 "bind list: List all active commands\n" +
					 "bind unbindall: Unbind all keys bound by 'bind'\n\n" +
					 "Example: bind LeftControl likeaboss" +
					 "Example: bind list");
			return;
		}

		KeyCode bind_key;
		string target_key;
		string bind_command;

		switch (args[0].ToLower())
		{
			case "list":
				ListBinds();
				return;
			case "debug":
				//Setup debug keybind here
				break;
			case "unbindall":
				UnbindAll();
				return;
			case "last":
				if (prevComs.Count == 0)
				{
					OutputText("Error: No last command to save!");
					return;
				}
				target_key = (args[1].Length == 1 ? args[1].ToUpper() : args[1]);
				if (!Enum.IsDefined(typeof(KeyCode), target_key))
				{
					OutputText("Error: " + target_key + " is an invalid key");
					return;
				}
				bind_key = (KeyCode)Enum.Parse(typeof(KeyCode), target_key);
				bind_command = prevComs[prevComs.Count - 1];
				if (bindComs.ContainsKey(bind_key)) { bindComs.Remove(bind_key); }
				bindComs.Add(bind_key, bind_command);
				OutputText("Last used command saved!\n" + bind_command + " bound to key " + bind_key.ToString());
				return;

		}
		target_key = (args[0].Length == 1 ? args[0].ToUpper() : args[0]);
		if (!Enum.IsDefined(typeof(KeyCode), target_key))
		{
			OutputText("Error: " + target_key + " is an invalid key");
			return;
		}
		bind_key = (KeyCode)Enum.Parse(typeof(KeyCode), target_key);

		if (args.Length < 2)
		{
			OutputText("Error: No command to bind");
			return;
		}

		bind_command = args[1];

		for (int i = 2; i < args.Length; i++) { bind_command += " " + args[i]; }

		if (bindComs.ContainsKey(bind_key)) { bindComs.Remove(bind_key); }
		bindComs.Add(bind_key, bind_command);

		OutputText("Command saved!\n" + bind_command + " bound to key " + bind_key.ToString());
	}

	//Unbind all
	private void UnbindAll()
	{
		bindComs.Clear();
		OutputText("All keys unbound");
	}

	//List binds
	private void ListBinds()
	{
		string s = "";
		foreach (KeyValuePair<KeyCode, string> com in bindComs)
		{
			s += com.Key.ToString() + ": " + com.Value + "\n";
		}
		if (bindComs.Count == 0) { s = "No keybinds found"; }
		OutputText(s);
	}

	//Function called from playercontroller to loop through the binds
	float bindResetTimer = 2.50f;
	public void CheckCustomKeys()
	{
		// Check if anticheat to disable command key bindings
		if (ModSaver.LoadIntFromFile("mod", "anticheat") == 1) { return; }

		foreach (KeyValuePair<KeyCode, string> com in bindComs)
		{
			if (Input.GetKeyDown(com.Key))
			{
				HudOutput("> " + com.Value, true, bindResetTimer);
				ParseResultString(com.Value, true);
			}
		}
	}

	//-------------------------------------
	//Item
	//-------------------------------------
	//Create item command
	public void CreateItem(string[] args)
	{
		if (args.Length < 1)
		{
			OutputText("Error: 'createitem' requires at least one parameter.\n" +
					 "Modifiers: -p <Vector3>: Change spawn position to <Vector3>\n\n" +
					 "Example: 'createitem Heart'  'createitem Heart -p 29 0 -38'\n\n" +
					 "List of available items:\n\n" + AddLineJumps(ItemList()));
			return;
		}

		if (!GetArg("-p", out Vector3 spawn_Position, args, out bool invalidValue))
		{
			spawn_Position = player.transform.localPosition + new Vector3(0f, 0f, -1f);
		}
		if (invalidValue)
		{
			OutputText("Error: Invalid position value\n");
			return;
		}

		string item_name = args[0];
		string itemToSpawn = item_name.ToLower() != "dungeon_key" ? "Item_" + item_name : "dungeon_key";

		if (SpawnItem(itemToSpawn, spawn_Position))
		{
			OutputText(item_name + " spawned!");
		}
		else
		{
			OutputText("Error: " + item_name + " item not found\n\n" +
					 "List of available items:\n\n" + AddLineJumps(ItemList()));
		}
	}

	//List items
	public string ItemList()
	{
		List<string> itemList = new List<string>();
		List<string> outputList = new List<string>();
		foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
		{
			if (gameObject.GetComponent<Item>() != null)
			{
				if (!itemList.Contains(gameObject.name))
				{
					itemList.Add(gameObject.name);
					outputList.Add(gameObject.name.Replace("Item_", ""));
				}
			}
		}
		string[] temp = outputList.ToArray();
		Array.Sort(temp, StringComparer.InvariantCulture);
		return string.Join(" ", temp);
	}

	//Zap command
	private Item lightningBall;
	private void Zap(string[] args)
	{
		if (lightningBall == null)
		{
			if (FindItem("Item_LightningBall", out Item itemOutput))
			{
				GameObject myThunder = Instantiate(itemOutput.gameObject);
				myThunder.SetActive(false);
				myThunder.name = itemOutput.name;
				DontDestroyOnLoad(myThunder);
				lightningBall = myThunder.GetComponent<Item>();
			}
			else
			{
				OutputText("No lightning available here. SAD!");
				return;
			}
		}

		SpawnItem(lightningBall, player.transform.localPosition);
		OutputText("ZAP!");
	}

	//Spawns an item given an item name. Does a resources.findall, SLOW
	private bool SpawnItem(string itemname, Vector3 position)
	{
		if (FindItem(itemname, out Item itemcomponent))
		{
			Item spawneditem = ItemFactory.Instance.GetItem(itemcomponent, null, position, true);
			spawneditem.gameObject.name = itemcomponent.gameObject.name;
			return true;
		}
		return false;
	}

	//Spawns an item given an item component. Fast!
	private bool SpawnItem(Item itemcomponent, Vector3 position)
	{
		if (itemcomponent != null)
		{
			Item spawneditem = ItemFactory.Instance.GetItem(itemcomponent, null, position, true);
			spawneditem.gameObject.name = itemcomponent.gameObject.name;
			return true;
		}
		return false;
	}

	//Finds an itemcomponent using a string. Does a resources.findall, SLOW
	private bool FindItem(string itemname, out Item itemcomponent)
	{
		itemcomponent = null;

		foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
		{
			if (gameObject.name.ToLower() == itemname.ToLower())
			{
				itemcomponent = gameObject.GetComponent<Item>();
				return true;
			}
		}
		return false;
	}

	//-------------------------------------
	//FPSmode
	//-------------------------------------
	public int fpsmode;
	private GameObject wobblecam;

	public float cam_fov = 35f;
	public float default_cam_fov = 35f;
	public float optional_cam_fov = 65f;
	public float cam_accel = 0.5f;

	public float cam_sens = 3f;
	public bool lock_vertical;
	public string[] cam_controls = new string[] { "W", "S", "A", "D", "Alpha1", "Alpha2", "Alpha3", "Alpha4" };
	public float cam_farclip = 1000000f;

	//FPSmode command
	public void SetCam(string[] args)
	{
		if (args.Length < 1)
		{
			OutputText("Error: cam needs modifiers\n" +
				    "Modifiers:\n" +
				    "-first: Switch camera and controls to first person mode\n" +
				    "-third: Switch camera and controls to third person mode\n" +
				    "-free: Switch camera and controls to free camera mode\n" +
				    "-default: Switch camera and controls back to default\n" +
				    "-fov <float>: Set the camera field of view\n" +
				    "-sens <float>: Set the sensitivity of the mouse\n" +
				    "-lockvertical / -unlockvertical: Disallow/allow vertical panning\n" +
				    "-clip <float>: Set the farclip of the camera (draw distance)\n" +
				    "-wheel <float>: Set how strongly does the mouse wheel change the flying speed (free mode only)\n" +
				    "-controls <KeyCode> x 8: Set the key controls for first person mode (case sensitive!)\n" +
				    "Order to enter the keycodes: Forward, backward, left, right, stick, force wand, dynamite and ice ring\n\n" +
				    "Example: cam -first\n\n" +
				    "Note: Non default cameras needs half a second with the mouse still to calibrate after enabling the mode or changing map!");
			return;
		}

		string output = "";
		bool invalidValue;
		float float_value;

		//Set mode
		if (GetArg("-default", args))
		{
			FPSToggle(0);
			output += "Camera mode returned to normal\n";
		}
		else if (GetArg("-first", args))
		{
			FPSToggle(1);
			output += "FPS mode ONLINE! \n\n" +
					"Look around with the mouse, move with WASD, use 1, 2, 3, and 4 or\n" +
					"mouse wheel to change weapons and click primary mouse button to attack\n";
		}
		else if (GetArg("-third", args))
		{
			FPSToggle(2);
			output += "Third person mode ACTIVATED! \n\n" +
					"Look around with the mouse, move with WASD, use 1, 2, 3, and 4 or\n" +
					"mouse wheel to change weapons and click primary mouse button to attack.\n" +
					"Hold secondary mouse button and scroll the mouse wheel to zoom in/out";
		}
		else if (GetArg("-free", args))
		{
			FPSToggle(3);
			output += "Free camera mode ON!\n\n" +
					"Look around with the mouse, move with WASD and change your flying speed\n" +
					"with the mouse wheel\n";
		}

		//Set fov
		if (GetArg("-fov", out float_value, args, out invalidValue))
		{
			output += "Camera Field of View updated from " + cam_fov + " to " + float_value + "\n";
			cam_fov = float_value;
			if (fpsmode != 0)
			{
				optional_cam_fov = cam_fov;
			}
			else
			{
				default_cam_fov = cam_fov;
			}
		}
		if (invalidValue)
		{
			if (fpsmode != 0)
			{
				optional_cam_fov = 65f;
			}
			else
			{
				default_cam_fov = 35f;
			}
			cam_fov = default_cam_fov;
			output += "Field of view value reset to default (" + cam_fov + ")\n";
		}

		//Set mouse wheel acceleration
		if (GetArg("-wheel", out float_value, args, out invalidValue))
		{
			output += "Mouse wheel flying speed acceleration updated from " + cam_accel + " to " + float_value + "\n";
			cam_accel = float_value;
		}
		if (invalidValue)
		{
			cam_accel = 0.5f;
			output += "Mouse wheel flying speed acceleration reset to default (" + cam_accel + ")\n";
		}

		//Lock vertical panning
		if (GetArg("-unlockvertical", args))
		{
			lock_vertical = false;
			output += "Vertical panning enabled\n";
		}
		else if (GetArg("-lockvertical", args))
		{
			lock_vertical = true;
			output += "Vertical panning disabled\n";
		}

		//Set clip
		if (GetArg("-clip", out float_value, args, out invalidValue))
		{
			output += "Camera far clip distance updated from " + cam_farclip + " to " + float_value + "\n";
			cam_farclip = float_value;
		}
		if (invalidValue)
		{
			output += "Camera far clip reset to default (" + cam_farclip + ")\n";
		}

		//Set rotation speed
		if (GetArg("-sens", out float_value, args, out invalidValue))
		{
			output += "Mouse sensitivity updated from " + cam_sens + " to " + float_value + "\n";
			cam_sens = float_value;
		}
		if (invalidValue)
		{
			cam_sens = 3f;
			output += "Mouse sensitivity reset to default (" + cam_sens + ")\n";
		}

		//Set controls
		if (GetArg("-controls", 8, out string[] string_values, args, out invalidValue))
		{
			bool keycodeOk = true;
			for (int i = 0; i < 8; i++)
			{
				string_values[i] = (string_values[i].Length == 1 ? string_values[i].ToUpper() : string_values[i]);
				if (!Enum.IsDefined(typeof(KeyCode), string_values[i]))
				{
					output += "Warning: " + string_values[i] + " is an invalid key, controls setup cancelled\n";
					keycodeOk = false;
				}
			}
			if (keycodeOk)
			{
				output += "Controls updated from:\n" + PrintCamControls(cam_controls) + "To:\n" + PrintCamControls(string_values);
				cam_controls = string_values;
			}
		}
		if (invalidValue)
		{
			cam_controls = new string[] { "W", "S", "A", "D", "Alpha1", "Alpha2", "Alpha3", "Alpha4" };
			output += "Camera controls reset to default:\n" + PrintCamControls(cam_controls);
		}

		//Reset
		if (GetArg("-reset", args))
		{
			cam_fov = 35f;
			default_cam_fov = 35f;
			optional_cam_fov = 65f;
			cam_sens = 3f;
			cam_accel = 1f;
			cam_controls = new string[] { "W", "S", "A", "D", "Alpha1", "Alpha2", "Alpha3", "Alpha4" };
			FPSToggle(0);

			output = "All camera values reset to default";
		}

		OutputText(output);
		CamConfig();
	}

	//Print FPS controls
	private string PrintCamControls(string[] controls)
	{
		return "Forward: " + controls[0] + " - Backward: " + controls[1] + " - Left: " + controls[2] + " - Right: " + controls[3] + "\n" +
			  "Stick: " + controls[4] + " - Force wand: " + controls[5] + " - Dynamite: " + controls[6] + " - Ice ring: " + controls[7] + "\n";
	}

	//FPSmode on/off toggler
	public void FPSToggle(int mode)
	{
		fpsmode = mode;
		bool nonDefaultCamera = (fpsmode != 0 ? true : false);

		cam_fov = (mode > 0 ? optional_cam_fov : default_cam_fov);
		//First person and free camera
		if (mode == 1 || mode == 3)
		{
			//Move Cameras, gameobject parent of the cameras
			wobblecam.transform.Find("Cameras").localPosition = Vector3.zero;

			//Change MainCamera and OutlineCamera positions
			wobblecam.transform.Find("Cameras").Find("Main Camera").localPosition = Vector3.zero;
			wobblecam.transform.Find("Cameras").Find("OutlineCamera").localPosition = Vector3.zero;

			//Change AudioListener position
			wobblecam.transform.Find("Cameras").Find("Main Camera").GetChild(0).localPosition = Vector3.zero;

			// Disable culling for pathfinding ents
			OnScreenCanceller waitState;
			OnScreenCanceller walkState;
			if (GameObject.Find("SecretPathController(Clone)") != null)
			{
				Transform pathController = GameObject.Find("SecretPathController(Clone)").transform;
				waitState = pathController.Find("WaitState").GetComponent<OnScreenCanceller>();
				walkState = pathController.Find("WalkState").GetComponent<OnScreenCanceller>();

				waitState._minTime = 0.1f;
				walkState._minTime = 999f;
			}
		}
		//Third person
		else if (mode == 2)
		{
			//Move Cameras, gameobject parent of the cameras
			wobblecam.transform.Find("Cameras").localPosition = Vector3.zero;

			//Change MainCamera and OutlineCamera positions
			wobblecam.transform.Find("Cameras").Find("Main Camera").localPosition = new Vector3(0f, -0.5f, -2.3f);
			wobblecam.transform.Find("Cameras").Find("OutlineCamera").localPosition = new Vector3(0f, -0.5f, -2.3f);

			//Change AudioListener position
			wobblecam.transform.Find("Cameras").Find("Main Camera").GetChild(0).localPosition = Vector3.zero;
		}
		//Default
		else
		{
			//Move Cameras, gameobject parent of the cameras
			wobblecam.transform.Find("Cameras").localPosition = new Vector3(0f, 1f, 0f);

			//Change MainCamera and OutlineCamera positions
			wobblecam.transform.Find("Cameras").Find("Main Camera").localPosition = new Vector3(0f, -0.5f, -2.3f);
			wobblecam.transform.Find("Cameras").Find("OutlineCamera").localPosition = new Vector3(0f, -0.5f, -2.3f);

			//Setup AudioListener
			wobblecam.transform.Find("Cameras").Find("Main Camera").GetChild(0).localPosition = new Vector3(0f, 0f, 15f);
		}

		//Remove art exhibit reflection when outside default mode
		GameObject artExhibitReflection = GameObject.Find("Refl");
		if (artExhibitReflection != null)
		{
			UnityEngine.MeshRenderer reflection = artExhibitReflection.GetComponent<UnityEngine.MeshRenderer>();
			if (reflection != null) { reflection.enabled = !nonDefaultCamera; }
		}

		//Setup camera movement
		wobblecam.GetComponent<FollowTransform>().SetupFPSMode(nonDefaultCamera);
		wobblecam.GetComponent<LevelCamera>().SetupFPSMode(nonDefaultCamera);
		wobblecam.GetComponent<CameraContainer>().SetupFPSMode(nonDefaultCamera);
		//Setup PlayerController
		GameObject.Find("PlayerController").GetComponent<PlayerController>().SetupFPSMode(mode);
		//Setup camera attributes
		CamConfig();
	}

	//Cameras and controls configuration
	private void CamConfig()
	{
		//Setup cameras
		Camera[] scenecameras = new Camera[]
		{
		  wobblecam.transform.Find("Cameras").Find("Main Camera").gameObject.GetComponent<Camera>(),
		  wobblecam.transform.Find("Cameras").Find("OutlineCamera").gameObject.GetComponent<Camera>()
		};
		foreach (Camera cam in scenecameras)
		{
			cam.fieldOfView = cam_fov;
			cam.nearClipPlane = fpsmode != 0 ? 0.1f : 0.3f;
			cam.farClipPlane = cam_farclip;
		}

		//Setup PlayerController
		PlayerController playercont = GameObject.Find("PlayerController").GetComponent<PlayerController>();
		playercont.mouse_sensitivity = cam_sens;
		playercont.lock_y_axis = lock_vertical;
		playercont.flyingAccel = cam_accel / 100f;
		if (lock_vertical) { playercont.cameraRotation.x = 0f; }
		playercont.SetFpsControls(cam_controls);
	}

	//-------------------------------------
	// SetSpeed
	//-------------------------------------
	public float move_speed = 5f;
	public float default_move_speed = 5f;
	public float default_roll_speed = 5.2f;
	public void SetSpeed(string[] args)
	{
		Moveable moveableData = player.GetComponent<Moveable>();
		float oldSpeed = moveableData._moveSpeed;

		if (args.Length < 1)     // If no args given, set to default
		{
			moveableData._moveSpeed = default_move_speed;
			move_speed = default_move_speed;
			player.transform.Find("Actions").GetComponent<RollAction>()._speed = default_roll_speed;
			OutputText("Set speed from " + oldSpeed + " to default (5)");
			return;
		}

		if (args.Length == 1)    // If arg given
		{
			if (float.TryParse(args[0], out float multiplier))     // If number
			{
				moveableData._moveSpeed = multiplier;
				move_speed = multiplier;
				player.transform.Find("Actions").GetComponent<RollAction>()._speed = multiplier;
				OutputText("Set speed from " + oldSpeed + " to " + multiplier);
				return;
			}
			else // If not number
			{
				OutputText("Error: Argument 0 (speed multiplier) must be a number\nExample: 'speed 15', 'speed 0.5'");
				return;
			}
		}

		else
		{
			OutputText("Error: Requires 1 argument of type int/float\nExample: 'speed 15', 'speed 0.5'");
		}
	}

	//-------------------------------------
	// Good ol' ludo code
	//-------------------------------------
	private static Entity FindEntity(string name)
	{
		Entity entity = EntityTag.GetEntityByName(name);
		if (entity == null)
		{
			GameObject gameObject = GameObject.Find(name);
			if (gameObject != null)
			{
				entity = gameObject.GetComponent<Entity>();
			}
		}
		return entity;
	}

	private void SetEntVar(string[] args)
	{
		if (args.Length < 3)
		{
			OutputText("SetEntVar expected at least three arguments\n\n" +
					 "This is a command originally made by ludosity, we are not entirely\n" +
					 "sure what it does, use at you own risk");
		}
		else
		{
			string text = args[0];
			string text2 = args[1];
			Entity entity = DebugCommands.FindEntity(text);
			if (entity == null)
			{
				OutputText("Entity not found: " + text);
				return;
			}
			int num;
			if (!int.TryParse(args[2], out num))
			{
				OutputText("Invalid value: " + args[2]);
				return;
			}
			entity.SetStateVariable(text2, num);
			OutputText("Set " + text + "." + text2 + " -> " + num);
		}
	}

	private void SetSaveVar(string[] args)
	{
		if (args.Length < 2)
		{
			OutputText("SetSaveVar expected at least two arguments\n\n" +
						 "This is a command originally made by Ludosity, we are not entirely\n" +
						 "sure what it does, use at you own risk");
		}
		else
		{
			if (debugMenu == null)
			{
				debugMenu = UnityEngine.Object.FindObjectOfType<DebugMenu>();
				if (debugMenu == null) { return; }
			}
			string key;
			IDataSaver saverAndName = debugMenu._saver.GetSaverAndName(args[0], out key, false);
			saverAndName.SaveData(key, args[1]);
			OutputText("set " + args[0] + " -> " + args[1]);
		}
	}

	private void ClearSaveVar(string[] args)
	{
		if (args.Length < 1)
		{
			OutputText("ClearSaveVar expected at least one argument\n\n" +
					 "This is a command originally made by Ludosity, we are not entirely\n" +
					 "sure what it does, use at you own risk");
		}
		else
		{
			if (debugMenu == null)
			{
				debugMenu = UnityEngine.Object.FindObjectOfType<DebugMenu>();
				if (debugMenu == null) { return; }
			}
			string text;
			IDataSaver saverAndName = debugMenu._saver.GetSaverAndName(args[0], out text, true);
			if (saverAndName != null)
			{
				if (saverAndName.HasData(text))
				{
					saverAndName.ClearValue(text);
					OutputText("cleared var " + text);
				}
				else if (saverAndName.HasLocalSaver(text))
				{
					saverAndName.ClearLocalSaver(text);
					OutputText("cleared saver " + text);
				}
			}
		}
	}

	private void WarpTo(string[] args)
	{
		if (args.Length < 1)
		{
			OutputText("WarpTo expected at least one argument\n\n" +
						 "This is a command originally made by Ludosity, it moves the player\n" +
						 "to another map, similar to goto, which is our own version.");
		}
		else
		{
			string targetScene = args[0];
			string targetDoor = (args.Length <= 1) ? string.Empty : args[1];
			SceneDoor.StartLoad(targetScene, targetDoor, debugMenu._fadeData, null, null);
		}
	}

	//-------------------------------------
	// Attack modifiers
	//-------------------------------------
	//Atkmod command
	public void AtkMod(string[] args)
	{
		if (args.Length < 1)
		{
			OutputText("Error: 'atk' needs a modifier.\n\n" +
					 "Modifiers:\n" +
					 "-attack: Power up the EFCS\n" +
					 "-dynamite: Unlimited dynamite\n" +
					 "-fuse <float>: Set the timer of the dynamite to <float>\n" +
					 "-radius <float>: Set the radius of the dynamite explosion to <float>\n" +
					 "-ice: Unlimited ice blocks\n" +
					 "-icegrid: Allow placing ice blocks out of grid\n" +
					 "-proj <int>: Multiply ammount of projectiles per shot to <int>\n" +
					 "-range <float>: Increase melee range by <float>\n" +
					 "-reset: Reset everything back to default\n\n" +
					 "Example: atk -dynamite -attack");
			return;
		}
		float float_value;
		bool bool_value;
		int int_value;

		bool invalidValue;
		string output = "";

		//Super attack
		if (GetArg("-attack", out bool_value, args, out invalidValue))
		{
			superAttack = bool_value;
			output += "SUPER ATTACK: " + (superAttack ? "ENABLED" : "DISABLED") + "\n";
		}
		if (invalidValue)
		{
			superAttack = !superAttack;
			output += "SUPER ATTACK: " + (superAttack ? "ENABLED" : "DISABLED") + "\n";
		}

		//Super attack shake
		if (GetArg("-noshake", out bool_value, args, out invalidValue))
		{
			noShake = bool_value;
			output += "Super attack shake: " + (noShake ? "ENABLED" : "DISABLED") + "\n";
		}
		if (invalidValue)
		{
			noShake = !noShake;
			output += "Super attack shake: " + (noShake ? "ENABLED" : "DISABLED") + "\n";
		}


		//Super dynamite
		if (GetArg("-dynamite", out bool_value, args, out invalidValue))
		{
			superDynamite = bool_value;
			output += "SUPER DYNAMITE: " + (superDynamite ? "ENABLED" : "DISABLED") + "\n";
		}
		if (invalidValue)
		{
			superDynamite = !superDynamite;
			output += "SUPER DYNAMITE: " + (superDynamite ? "ENABLED" : "DISABLED") + "\n";
		}

		//Super ice
		if (GetArg("-ice", out bool_value, args, out invalidValue))
		{
			superIce = bool_value;
			output += "SUPER ICE: " + (superIce ? "ENABLED" : "DISABLED") + "\n";
		}
		if (invalidValue)
		{
			superIce = !superIce;
			output += "SUPER ICE: " + (superIce ? "ENABLED" : "DISABLED") + "\n";
		}

		//No gridlock
		if (GetArg("-icegrid", out bool_value, args, out invalidValue))
		{
			iceOffgrid = bool_value;
			output += "Iceblock not gridlocked: " + (iceOffgrid ? "On" : "Off") + "\n";
		}
		if (invalidValue)
		{
			iceOffgrid = !iceOffgrid;
			output += "Iceblock not gridlocked: " + (iceOffgrid ? "On" : "Off") + "\n";
		}

		//Fuse time
		if (GetArg("-fuse", out float_value, args, out invalidValue))
		{
			float old_dynamite_fuse = dynamite_fuse;
			dynamite_fuse = float_value;
			output += "Dynamite fuse changed from " + old_dynamite_fuse + " to " + dynamite_fuse + "\n";
		}
		if (invalidValue)
		{
			dynamite_fuse = 1.5f;
			output += "Dynamite fuse reset to default (" + dynamite_fuse + ")\n";
		}

		//Explosion radius
		if (GetArg("-radius", out float_value, args, out invalidValue))
		{
			float old_dynamite_radius = dynamite_radius;
			dynamite_radius = float_value;
			output += "Dynamite explosion radius changed from " + old_dynamite_radius + " to " + dynamite_radius + "\n";
		}
		if (invalidValue)
		{
			dynamite_radius = 1.7f;
			output += "Dynamite explosion radius reset to default (" + dynamite_radius + ")\n";
		}

		//Projectile number
		if (GetArg("-proj", out int_value, args, out invalidValue))
		{
			if (int_value < 1) { OutputText("Warning: 'proj' cannot be less than 1 (changed value to 1)"); }
			int old_projectile_count = projectile_count;
			projectile_count = int_value;
			output += "Projectile count changed from " + old_projectile_count + " to " + projectile_count + "\n";
		}
		if (invalidValue)
		{
			projectile_count = 1;
			output += "Projectile count changed to default (" + projectile_count + ")\n";
		}

		//Attack range
		if (GetArg("-range", out float_value, args, out invalidValue))
		{
			float old_extra_bulge = extra_bulge;
			extra_bulge = float_value;
			output += "Extra range changed from " + old_extra_bulge + " to " + extra_bulge + "\n";
		}
		if (invalidValue)
		{
			extra_bulge = 0f;
			output += "Extra range changed to default (" + extra_bulge + ")\n";
		}

		//Reset
		if (GetArg("-reset", args))
		{
			superAttack = false;
			superDynamite = false;
			dynamite_fuse = 1.5f;
			dynamite_radius = 1.7f;
			superIce = false;
			iceOffgrid = false;
			projectile_count = 1;
			extra_bulge = 0f;
			output += "All attacks reset to default";
		}

		SuperDynamite();
		SuperIce();
		SuperAttack();
		SuperProjectiles();
		SuperRange();

		OutputText(output);
	}

	//Super attack
	public bool superAttack;
	public bool noShake;
	private void SuperAttack()
	{
		GameObject efcs = player.transform.Find("Attacks").Find("EFCSSwitcher").gameObject;
		if (superAttack)
		{
			efcs.GetComponent<ChargeAction>()._chargeTime = 0f;
			player.transform.Find("Attacks").Find("EFCS").gameObject.GetComponent<AttackAction>()._attackTime = 0f;
			GameObject.Find("Cameras").transform.parent.gameObject.GetComponent<CameraShaker>().extra_shake = noShake ? 0f : 1f;
		}
		else
		{
			efcs.GetComponent<ChargeAction>()._chargeTime = 3f;
			player.transform.Find("Attacks").Find("EFCS").gameObject.GetComponent<AttackAction>()._attackTime = 1f;
			GameObject.Find("Cameras").transform.parent.gameObject.GetComponent<CameraShaker>().extra_shake = 0f;
		}
	}

	//Super Projectiles
	public int projectile_count = 1;
	private void SuperProjectiles()
	{
		ProjectileAttack[] projectile_attacks = new ProjectileAttack[]
		{
		  player.transform.Find("Attacks").Find("ForceWand").gameObject.GetComponent<ProjectileAttack>(),
		  player.transform.Find("Attacks").Find("ForceWand").Find("Lv2").gameObject.GetComponent<ProjectileAttack>(),
		  player.transform.Find("Attacks").Find("ForceWand").Find("Lv3").gameObject.GetComponent<ProjectileAttack>(),
		  player.transform.Find("Attacks").Find("ForceWand").Find("Lv4").gameObject.GetComponent<ProjectileAttack>(),
		  player.transform.Find("Attacks").Find("MeleeCh1").Find("Melee3Ch1").gameObject.GetComponent<ProjectileAttack>(),
		  player.transform.Find("Attacks").Find("MeleeCh2").Find("Melee3Ch2").gameObject.GetComponent<ProjectileAttack>(),
		  player.transform.Find("Attacks").Find("Melee3").gameObject.GetComponent<ProjectileAttack>(),
		  player.transform.Find("Attacks").Find("Melee4").gameObject.GetComponent<ProjectileAttack>()
		};
		foreach (ProjectileAttack projectile in projectile_attacks)
		{
			if (projectile != null) { projectile.debug_projectile_count = projectile_count; }
		}
	}

	//Super range
	public float extra_bulge = 0f;
	private void SuperRange()
	{
		MeleeAttack[] melee_attacks = new MeleeAttack[]
		{
		  player.transform.Find("Attacks").Find("IceRing").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("IceRing").Find("Lv2").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("IceRing").Find("Lv3").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("IceRing").Find("Lv4").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("Dynamite").Find("Lv4").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("MeleeCh1").Find("Melee1Ch1").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("MeleeCh1").Find("Melee2Ch1").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("MeleeCh1").Find("Melee3Ch1").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("MeleeCh2").Find("Melee1Ch2").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("MeleeCh2").Find("Melee2Ch2").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("MeleeCh2").Find("Melee3Ch2").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("MeleeCh3").Find("MeleeCh3Back").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("Melee1").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("Melee2").gameObject.GetComponent<MeleeAttack>(),
		  player.transform.Find("Attacks").Find("Melee3").gameObject.GetComponent<MeleeAttack>()
		};
		foreach (MeleeAttack melee in melee_attacks)
		{
			if (melee != null) { melee.extra_bulge = extra_bulge; }
		}
	}

	//Super dynamite
	public bool superDynamite;
	public float dynamite_fuse = 1.5f;
	public float dynamite_radius = 1.7f;
	private void SuperDynamite()
	{
		RoomObjectAttack[] dynamite = new RoomObjectAttack[]
		{
		  player.transform.Find("Attacks").Find("Dynamite").gameObject.GetComponent<RoomObjectAttack>(),
		  player.transform.Find("Attacks").Find("Dynamite").Find("Lv2").gameObject.GetComponent<RoomObjectAttack>(),
		  player.transform.Find("Attacks").Find("Dynamite").Find("Lv3").gameObject.GetComponent<RoomObjectAttack>(),
		  player.transform.Find("Attacks").Find("Dynamite").Find("Lv4").gameObject.GetComponent<RoomObjectAttack>()
		};
		foreach (RoomObjectAttack explosive in dynamite)
		{
			if (explosive != null)
			{
				explosive.SetInfinite(superDynamite);
				explosive.SetTimer(dynamite_fuse);
				explosive.SetExplosion(dynamite_radius);
			}
		}
	}

	//Super ice
	public bool superIce;
	public bool iceOffgrid;
	private void SuperIce()
	{
		RoomObjectAttack iceblock = player.transform.Find("Attacks").Find("IceRing").Find("Block").GetComponent<RoomObjectAttack>();
		if (iceblock != null)
		{
			iceblock.SetInfinite(superIce);
			iceblock.UnlockFromGrid(iceOffgrid);
		}
	}

	//-------------------------------------
	//GoTo
	//-------------------------------------
	Dictionary<string[], string[]> mapsAndSpawns;
	//Scenes dictionary setup
	void MakeMapsAndSpawns()
	{
		mapsAndSpawns = new Dictionary<string[], string[]>
	   {
			// Overworld
			{new string[] {"fluffyfields","fluffy","fields","ff"}, new string[] {"FluffyFields", "FF_VH1", "RestorePt1", "FF_FR1", "FF_FR4", "FF_CC1", "FF_SW1", "FF_SS1", "FF_SW3", "SunkenLabyrinthOutside", "DreamWorldOutside", "FF_SW4", "FF_FR2"}},
		  {new string[] {"candycoast","candy","sweetwatercoast","sweetwater","sweetwater","coast","cc"}, new string[] {"CandyCoast", "CC_FF1", "RestorePt1", "RestorePt2", "RestorePt3", "CC_SW2"}},
		  {new string[] {"fancyruins", "fancy", "ruins", "fr"}, new string[] {"FancyRuins", "FR_FF1", "RestorePt1", "FR_VH1", "FR_FF2", "FR_FF4", "FR_SW1", "FR_FC1"}},
		  {new string[] {"fancyhills", "fh", "fancyruins2", "fancy2", "ruins2", "fr2"}, new string[] {"FancyRuins2", "RestorePt1"}},
		  {new string[] {"starwoods", "star", "woods", "sw"}, new string[] {"StarWoods", "SW_FF1", "SW_FF3", "SW_FF4", "SW_FR1", "RestorePt1", "SW_CC1", "SW_CC2", "DarkHypostyleOutside"}},
		  {new string[] {"starcoast", "starwoods2", "star2", "woods2", "sw2"}, new string[] {"StarWoods2", ""}},
		  {new string[] {"slipperyslope", "slippery", "slope", "ss"}, new string[] {"SlipperySlope", "RestorePt2", "RestorePt1", "SS_LR1", "SS_VH1", "SS_CC1", "SS_FF1"}},
		  {new string[] {"vitaminhills", "vitamin", "hills", "vh", "pepperpainprairie", "pepperpain", "prairie", "pp"}, new string[] {"VitaminHills", "VH_SS1", "VH_FF1", "RestorePt1", "VH_FR1"}},
		  {new string[] {"pepperpaintrail", "trail", "pt", "vitaminhills2", "vitamin2", "hills2", "vh2", "pepperpainprairie2", "pepperpain2", "prairie2", "pp2"}, new string[] {"VitaminHills2", ""}},
		  {new string[] {"pepperpainmountain", "mountain", "pm", "vitaminhills3", "vitamin3", "hills3", "vh3", "pepperpainprairie3", "pepperpain3", "prairie3", "pp3"}, new string[] {"VitaminHills3", "RestorePt1", "RestorePt2"}},
		  {new string[] {"frozencourt", "frozen", "court", "fc"}, new string[] {"FrozenCourt", "RestorePt1", "RestorePt2", "FC_FR1"}},
		  {new string[] {"lonelyroad", "lonely", "road", "lr"}, new string[] {"LonelyRoad", "RestorePt1", "LR_SS1"}},
		  {new string[] {"forbiddenarea", "forbidden", "fa", "lonelyroad2", "lonely2", "road2", "lr2"}, new string[] {"LonelyRoad2", "TombOfSimulacrumOutside", "RestorePt1"}},
		  {new string[] {"dreamworld", "dw"}, new string[] {"DreamWorld", "DreamWorldInside", "DreamForceOutside", "DreamDynamiteOutside", "DreamAllOutside", "DreamFireChainOutside", "DreamIceOutside"}},
			// Caves
			{new string[] {"fluffyfieldscaves", "fluffycaves", "fieldscaves", "ffcaves"}, new string[] {"FluffyFieldsCaves", ""}},
		  {new string[] {"candycoastcaves", "candycaves", "coastcaves", "cccaves", "sweetwatercaves"}, new string[] {"CandyCoastCaves", ""}},
		  {new string[] {"fancyruinscaves", "fancycaves", "ruinscaves", "frcaves"}, new string[] {"FancyRuinsCaves", ""}},
		  {new string[] {"starwoodscaves", "starcaves", "woodscaves", "swcaves"}, new string[] {"StarWoodsCaves", ""}},
		  {new string[] {"slipperyslopecaves", "slipperycaves", "slopecaves", "sscaves"}, new string[] {"SlipperySlopeCaves", ""}},
		  {new string[] {"vitaminhillscaves", "vitamincaves", "hillscaves", "vhcaves", "pepperpainprairiecaves", "pepperpaincaves", "prairiecaves", "ppcaves"}, new string[] {"VitaminHillsCaves", ""}},
		  {new string[] {"frozencourtcaves", "frozencaves", "courtcaves", "fccaves"}, new string[] {"FrozenCourtCaves", ""}},
		  {new string[] {"lonelyroadcaves", "lonelycaves", "roadcaves", "lrcaves"}, new string[] {"LonelyRoadCaves", ""}},
			// Portal worlds
			{new string[] {"deep1", "autumnclimb", "ac"}, new string[] {"Deep1", "Deep1"}},
		  {new string[] {"deep2", "vault", "v"}, new string[] {"Deep2", "Deep2"}},
		  {new string[] {"deep3", "painfulplain", "painful"}, new string[] {"Deep3", "Deep3"}},
		  {new string[] {"deep4", "farthestshore", "shore", "fs"}, new string[] {"Deep4", "Deep4"}},
		  {new string[] {"deep5", "scrapyard", "scrap", "sy"}, new string[] {"Deep5", "Deep5"}},
		  {new string[] {"deep6", "brutaloasis", "oasis", "bo"}, new string[] {"Deep6", "Deep6"}},
		  {new string[] {"deep7", "formercolossus", "colossus"}, new string[] {"Deep7", "Deep7"}},
		  {new string[] {"deep8", "sandcrucible", "crucible"}, new string[] {"Deep8", "Deep8"}},
		  {new string[] {"deep9", "oceancastle", "ocean", "oc"}, new string[] {"Deep9", "Deep9"}},
		  {new string[] {"deep10", "promenadepath", "promenade"}, new string[] {"Deep10", "Deep10"}},
		  {new string[] {"deep11", "mazeofsteel", "steel", "mos"}, new string[] {"Deep11", "Deep11"}},
		  {new string[] {"deep12", "walloftext", "text", "wot"}, new string[] {"Deep12", "Deep12"}},
		  {new string[] {"deep13", "lostcityofavlopp", "avlopp", "loa"}, new string[] {"Deep13", "Deep13"}},
		  {new string[] {"deep14", "northernend", "ne"}, new string[] {"Deep14", "Deep14"}},
		  {new string[] {"deep15", "moongarden", "moon", "garden", "mg"}, new string[] {"Deep15", "Deep15"}},
		  {new string[] {"deep16", "nowhere"}, new string[] {"Deep16", "Deep16"}},
		  {new string[] {"deep17", "caveofmystery", "com"}, new string[] {"Deep17", "Deep17"}},
		  {new string[] {"deep18", "somewhere"}, new string[] {"Deep18", "Deep18"}},
		  {new string[] {"deep19", "testroom", "test", "tr"}, new string[] {"Deep19", "Deep19"}},
		  {new string[] {"deep19s", "promisedremedy", "remedy", "pr"}, new string[] {"Deep19s", "Deep19s", "RestorePt1", "RestorePt2"}},
		  {new string[] {"deep20", "ludocity", "ludosity", "lc"}, new string[] {"Deep20", "Deep20"}},
		  {new string[] {"deep21", "abyssalplain", "abyss", "ap"}, new string[] {"Deep21", "Deep21"}},
		  {new string[] {"deep22", "placefromyoungerdays", "youngerdays", "pfyd", "yd"}, new string[] {"Deep22", "Deep22"}},
		  {new string[] {"deep23", "abandonedhouse", "ah"}, new string[] {"Deep23", "Deep23"}},
		  {new string[] {"deep24", "houseofsecrets", "hos"}, new string[] {"Deep24", "Deep24"}},
		  {new string[] {"deep25", "darkroom", "prebaddream", "tobaddream"}, new string[] {"Deep25", "Deep25"}},
		  {new string[] {"deep26", "baddream", "bd", "41"}, new string[] {"Deep26", "Deep26"}},
			// Dungeons
			{new string[] {"pillowfort", "pf", "d1"}, new string[] {"PillowFort", "PillowFortInside", "RestorePt1"}},
		  {new string[] {"sandcastle", "sc", "d2"}, new string[] {"SandCastle", "SandCastleInside", "RestorePt1"}},
		  {new string[] {"artexhibit", "ae", "d3"}, new string[] {"ArtExhibit", "ArtExhibitInside", "RestorePt1"}},
		  {new string[] {"trashcave", "tc", "d4"}, new string[] {"TrashCave", "TrashCaveInside", "RestorePt1"}},
		  {new string[] {"floodedbasement", "fb", "d5"}, new string[] {"FloodedBasement", "FloodedBasementInside", "RestorePt1"}},
		  {new string[] {"potassiummine", "pm", "d6"}, new string[] {"PotassiumMine", "PotassiumMineInside", "RestorePt1"}},
		  {new string[] {"boilinggrave", "bg", "d7"}, new string[] {"BoilingGrave", "BoilingGraveInside", "RestorePt1"}},
		  {new string[] {"grandlibrary", "gl", "d8"}, new string[] {"GrandLibrary", "GrandLibraryInside"}},
		  {new string[] {"grandlibrary2", "gl2", "d82", "shiftingchamber"}, new string[] {"GrandLibrary2", "RestorePt1"}},
		  {new string[] {"sunkenlabyrinth", "sl", "s1"}, new string[] {"SunkenLabyrinth", "SunkenLabyrinthInside", "RestorePt1"}},
		  {new string[] {"machinefortress", "mf", "s2"}, new string[] {"MachineFortress", "MachineFortressInside", "RestorePt1"}},
		  {new string[] {"darkhypostyle", "dh", "s3"}, new string[] {"DarkHypostyle", "DarkHypostyleInside", "RestorePt1"}},
		  {new string[] {"tombofsimulacrum", "tos", "s4"}, new string[] {"TombOfSimulacrum", "TombOfSimulacrumInside", "RestorePt1"}},
		  {new string[] {"wizardrylab", "wl", "dw1", "dreamforce"}, new string[] {"DreamForce", "DreamForceInside"}},
		  {new string[] {"syncope", "s", "dw2", "dreamdynamite"}, new string[] {"DreamDynamite", "DreamDynamiteInside", "RestorePt1"}},
		  {new string[] {"antigram", "a", "dw3", "dreamfirechain"}, new string[] {"DreamFireChain", "DreamFireChainInside", "RestorePt1"}},
		  {new string[] {"bottomlesstower", "bt", "dw4", "dreamice"}, new string[] {"DreamIce", "DreamIceInside", "RestorePt1"}},
		  {new string[] {"quietus", "q", "dw5", "dreamall"}, new string[] {"DreamAll", "DreamAllInside", "RestorePt1", "RestorePt2"}},
			// Others
			{new string[] {"mainmenu", "menu", "title", "titlescreen", "home"}, new string[] {"MainMenu", ""}},
		  {new string[] {"splashscreen", "splash", "nicalissucks", "byenicalis"}, new string[] {"SplashScreen", ""}},
		  {new string[] {"intro", "prologue", "cutscene1"}, new string[] {"Intro", ""}},
		  {new string[] {"outro", "credits", "cutscene2"}, new string[] {"Outro", ""}}
	   };
	}

	//GoTo command
	public FadeEffectData gotoTransition;
	void GoTo(string[] args)
	{
		int spawn_index = 0;
		string map = "";
		string spawn = "";

		if (args.Length < 1)     // If no args given, load current map's default spawn (wrong warp)
		{

			map = ModMaster.GetMapName();
			spawn = map;
			OutputText("Now loading " + map + ", spawning at default...");
		}
		else // If args are given
		{
			if (args.Length > 1) { int.TryParse(args[1], out spawn_index); }
			string target_scenename = args[0].ToLower();
			//Loop through each entry in the dictionary
			foreach (KeyValuePair<string[], string[]> MaS in mapsAndSpawns)
			{
				//Loop through each string in the key array
				foreach (string scenename in MaS.Key)
				{
					if (scenename == target_scenename)
					{
						map = MaS.Value[0];
						if (args.Length == 1)
						{
							spawn = MaS.Value[1];
						}
						//If the index is bigger than the value string, use 0 value
						else if (args.Length == 2 && MaS.Value.Length > spawn_index && spawn_index > 0)
						{
							spawn = MaS.Value[spawn_index];
						}
						else
						{
							OutputText("Error: " + spawn_index + " is an invalid spawn for " + map);
							return;
						}
						//Exit strings foreach if the map was found
						break;
					}
				}
				//Exit dictionary foreach if the map was found
				if (map != "") { break; }
			}
			//If map was not found, return
			if (map == "")
			{
				OutputText("Error: Map " + args[0] + " is not a valid map");
				return;
			}
		}
		if (spawn != "")
		{
			OutputText("Now loading " + map + " at " + spawn + "...");
		}
		else
		{
			OutputText("Now loading " + map + "...");
		}

		// Save to file if not in cave
		if (ModMaster.GetMapType(map) != "Cave")
		{
			ModSaver.SaveStringToFile("start", "level", map);
			ModSaver.SaveStringToFile("start", "door", spawn, true);
		}

		foreach (SceneDoor door in Resources.FindObjectsOfTypeAll<SceneDoor>())
		{
			if (door._fadeData != null)
			{
				gotoTransition = door._fadeData;
			}
		}

		// Start load
		if (gotoTransition != null)
		{
			SceneDoor.StartLoad(map, spawn, gotoTransition, null, null);
		}
	}

	//-------------------------------------
	// NoClip
	//-------------------------------------
	public bool noclip;
	public void NoClip(string[] args)
	{
		if (args.Length > 0)
		{
			if (ParseBool(args[0], out bool result))
			{
				noclip = result;
			}
		}
		else
		{
			noclip = !noclip;
		}

		BC_ColliderAACylinderN collider = player.GetComponent<BC_ColliderAACylinderN>();
		collider.enabled = !noclip;

		if (noclip)    // If disabled
		{
			OutputText("Hitbox disabled. You can walk through stuff!");
			return;
		}
		OutputText("Hitbox enabled. You're back to being lame...");
	}

	//-------------------------------------
	//Load config
	//-------------------------------------
	
	private void LoadConfig(string[] args)
	{
		if (args.Length < 1)
		{
			OutputText("Error: txt file name needed\n" +
					 "Argument: filename <string>\n\n" +
					 "Parameters:\n" +
					 "-onload <string>: Run <string>.txt when loading a new scene\n" +
					 "-clearonload: Stop running the -onload file\n" +
					 "-showerrors: After finishing running the file, return all invalid commands\n\n" +
					 "Example: loadconfig helloworld");
			return;
		}
		string filename = args[0];
		bool invalidValue;

		if (GetArg("-clearonload", args))
		{
			OutputText("Cleared txt file on on loading a new scene");
            ModScriptHandler.Instance.OnNewSceneTxt = "";
			return;
		}
		else if (GetArg("-onload", out string s, args, out invalidValue))
		{
			OutputText("The file " + s + ".txt will be run on loading a new scene");
            ModScriptHandler.Instance.OnNewSceneTxt = s;
			return;
		}
		if (invalidValue)
		{
			OutputText("Error: Invalid file name");
			return;
		}

		if (!ModScriptHandler.Instance.ParseTxt(filename, out string erroroutpup))
		{
			OutputText("Error: " + filename + ".txt could not be loaded");
		}
		else if (GetArg("-showerrors", args))
		{
			if (erroroutpup == "") { erroroutpup = "No invalid commands found"; }
			OutputText(erroroutpup);
		}
	}

	//-------------------------------------
	//Echo
	//-------------------------------------
	private void Echo(string[] args)
	{
		string output = "";
        bool wrapText = true;
        if (args[0] != null && args[0] == "NOWRAP") wrapText = false;
        
		for (int i = 0; i < args.Length; i++)
        {
            if(!(!wrapText && i == 0)) output += args[i] + " ";
        }
		output = output.Replace("|", "\n");
		OutputText(output, wrapText);
        ModScriptHandler.Instance.OutputToConsole(output, wrapText);
	}

	//-------------------------------------
	// SetPos
	//-------------------------------------
	void SetPos(string[] args)
	{
		float posX;
		float posY;
		float posZ;
		Vector3 oldPos = player.transform.position;

		if (args.Length < 1)     // If no args given
		{
			OutputText("Ittle's current position is: " + player.transform.position);
			return;
		}

		if (args.Length == 1)    // If 1 arg given
		{
			if (args[0] == "save")   // If saving pos
			{
				// Save pos
				ModSaver.SaveFloatToPrefs("PosX", player.transform.position.x);
				ModSaver.SaveFloatToPrefs("PosY", player.transform.position.y);
				ModSaver.SaveFloatToPrefs("PosZ", player.transform.position.z);

				// Save scene & room
				ModSaver.SaveStringToPrefs("SavedScene", ModMaster.GetMapName());
				ModSaver.SaveStringToPrefs("SavedRoom", ModMaster.GetMapRoom());

				OutputText("Saved position! \n\n" + ModMaster.GetMapName() + "\nRoom " + ModMaster.GetMapRoom() + "\n" + player.transform.position);
				return;
			}

			if (args[0] == "load")   // If loading pos
			{
				// Load pos
				posX = ModSaver.LoadFloatFromPrefs("PosX");
				posY = ModSaver.LoadFloatFromPrefs("PosY");
				posZ = ModSaver.LoadFloatFromPrefs("PosZ");

				// If in unloaded room, unload past room and load new room
				if (ModMaster.GetMapName() == ModSaver.LoadStringFromPrefs("SavedScene"))
				{
					if (ModMaster.GetMapRoom() != ModSaver.LoadStringFromPrefs("SavedRoom"))
					{
						LevelRoom savedRoom = GameObject.Find("LevelRoot").transform.Find(ModSaver.LoadStringFromPrefs("SavedRoom")).GetComponent<LevelRoom>();
						LevelRoom prevRoom = GameObject.Find("LevelRoot").transform.Find(ModMaster.GetMapRoom()).GetComponent<LevelRoom>();
						CameraContainer roomCamChanger = GameObject.Find("Cameras").transform.parent.gameObject.GetComponent<CameraContainer>();

						savedRoom.SetRoomActive(true, false);
						prevRoom.SetRoomActive(false, false);
						roomCamChanger.SetRoom(savedRoom);
					}

					// Update debug view's CurrRoom value
					ModSaver.SaveStringToPrefs("CurrRoom", ModSaver.LoadStringFromPrefs("SavedRoom"));

					// Teleport player
					player.transform.position = new Vector3(posX, posY, posZ);

					OutputText("Loaded position! \n\n" + ModMaster.GetMapName() + "\nRoom " + ModMaster.GetMapRoom() + "\n" + player.transform.position);
					return;
				}

				// If saved position is not for current map
				OutputText("Error: The position you attempted to load is for " + ModSaver.LoadStringFromPrefs("SavedScene") + ".\nUsing it here would likely spawn you out of bounds!");
				return;
			}

			// If not saving or loading, error
			OutputText("Error: Must either be saving or loading position");
			return;
		}

		if (args.Length == 2)    // If 2 args given
		{
			if (args[0].ToLower() == "x" && float.TryParse(args[1], out float posXOut))     // If X and number
			{
				posY = player.transform.position.y;
				posZ = player.transform.position.z;
				player.transform.position = new Vector3(posXOut, posY, posZ);
				OutputText("Moved player from " + oldPos + " to " + player.transform.position);
				return;
			}

			if (args[0].ToLower() == "y" && float.TryParse(args[1], out float posYOut))     // If Y and number
			{
				posX = player.transform.position.x;
				posZ = player.transform.position.z;
				player.transform.position = new Vector3(posX, posYOut, posZ);
				OutputText("Moved player from " + oldPos + " to " + player.transform.position);
				return;
			}

			if (args[0].ToLower() == "z" && float.TryParse(args[1], out float posZOut))     // If Z and number
			{
				posX = player.transform.position.x;
				posY = player.transform.position.y;
				player.transform.position = new Vector3(posX, posY, posZOut);
				OutputText("Moved player from " + oldPos + " to " + player.transform.position);
				return;
			}

			// If not valid, error
			OutputText("Error: Must specify axis (x, y, or z) and number\nExample: 'pos y 5'");
			return;
		}

		if (args.Length == 3)    // If 3 args given
		{
			if (float.TryParse(args[0], out float posXOut) && float.TryParse(args[1], out float posYOut) && float.TryParse(args[2], out float posZOut)) // If numbers
			{
				player.transform.position = new Vector3(posXOut, posYOut, posZOut);
				OutputText("Moved player from " + oldPos.ToString() + " to " + player.transform.position.ToString());
				return;
			}

			// If not numbers, error
			OutputText("Error: All 3 arguments (posX, posY, posZ) must be numbers");
			return;
		}

		// If more than 3 args given, error
		OutputText("Error: Cannot specify more than 3 arguments");
		return;
	}

	//-------------------------------------
	// SetSize
	//-------------------------------------
	public Vector3 resizeSelfScale = Vector3.one;
	public Vector3 resizeEnemiesScale = Vector3.one;
	bool hasResizedEnemies;
	bool hasResizedSelf;
	public void SetSize(string[] args)
	{
		Vector3 newScale;
		Vector3 oldScale = Vector3.one;

		if (args.Length > 0 && args.Length < 5) // If 2-4 args given
		{
			if (args[0].ToLower() == "self" || args[0].ToLower() == "player")     // If self
			{
				GameObject ittle = GameObject.Find("Ittle");
				oldScale = ittle.transform.localScale;

				if (args.Length == 2 && ParseVector3(args[1], out newScale))     // If 2 args and number
				{
					hasResizedSelf = true;
					OutputText("Set Ittle's scale from " + oldScale.ToString() + " to " + newScale.ToString());
				}
				else if (args.Length == 4 && ParseVector3(args, out newScale, 1))     // If 4 args and numbers
				{
					hasResizedSelf = true;
					OutputText("Set Ittle's scale from " + oldScale.ToString() + " to " + newScale.ToString());
				}
				else
				{
					hasResizedSelf = false;
					newScale = Vector3.one;
					OutputText("Set Ittle's scale back to normal");
				}

				ittle.transform.localScale = newScale;
				player.transform.Find("Hittable").GetComponent<Knockbackable>().RefreshKnockbackScale(player.GetComponent<Entity>());
				resizeSelfScale = ittle.transform.localScale;
				return;
			}

			if (args[0].ToLower() == "enemy" || args[0].ToLower() == "enemies")   // If enemies
			{

				if (args.Length == 2 && ParseVector3(args[1], out newScale))     // If 2 args and number
				{
					hasResizedEnemies = true;
					OutputText("Set all enemies' scales from " + oldScale.ToString() + " to " + newScale.ToString());
				}
				else if (args.Length == 4 && ParseVector3(args, out newScale, 1))     // If 4 args and numbers
				{
					hasResizedEnemies = true;
					OutputText("Set all enemies' scales from " + oldScale.ToString() + " to " + newScale.ToString());
				}
				else
				{
					hasResizedEnemies = false;
					newScale = new Vector3(1, 1, 1);
					OutputText("Set enemies's scale back to normal");
				}
				foreach (Entity ent in Resources.FindObjectsOfTypeAll<Entity>())
				{
					if (ent.name != "PlayerEnt" && ent.name != "FishbunRide")  // Do not include player
					{
						ent.transform.localScale = newScale;
					}
				}

				resizeEnemiesScale = newScale;
				return;
			}

			// If invalid, error
			OutputText("Error: Must give object (self/enemy) and size (float)\nExample: 'size self 5', 'size enemy 2.5");
			return;
		}

		// If args count invalid
		OutputText("Error: Must give object (self/enemy) and size (float)\nExample: 'size self 5', 'size enemy 2.5");
		return;
	}

	//-------------------------------------
	// Time stuff
	//-------------------------------------
	public float timeflow = 4f;
	public void Time(string[] args)
	{
		GameObject levelData = GameObject.Find("BaseLevelData");
		LevelTime timeData = levelData.GetComponent<LevelTime>();
		LevelTimeUpdater timeFlowData = levelData.transform.Find("TimeStuff").GetComponent<LevelTimeUpdater>();

		if (levelData == null || timeData == null || timeFlowData == null)    // If any are not found, error
		{
			if (levelData == null)
			{
				OutputText("Error: 'levelData' was not found!");
				return;
			}

			if (timeData == null)
			{
				OutputText("Error: 'timeData' was not found!");
				return;
			}

			if (timeFlowData == null)
			{
				OutputText("Error: 'timeFlowData' was not found!");
				return;
			}
		}

		string currentTime = ModMaster.GetCurrentTime();
		float timeFlow = timeFlowData._hoursPerMinute;

		if (args.Length < 1)     // If no args
		{
			string output = "Current time: " + currentTime + "\n";
			output += "Time passes at: " + timeFlow + " hours/minute" + "\n";
			if (timersEnabled)
			{
				output += "Timers are enabled";
			}
			else
			{
				output += "Timers are disabled";
			}
			output += "\n\n" +
					"time timers: Trigger all the timers in the scene\n" +
					"time settime <float>: Set time of the day\n" +
					"time setflow <float>: Set flow of time. Enter no arguments to reset to default\n";
			OutputText(output);
			return;
		}

		if (args.Length == 1)    // If 1 arg
		{
			if (args[0] == "timers") // If setting timers
			{
				SetTimers();
				return;
			}

			if (args[0] == "setflow")
			{
				timeflow = 4;
				SetFlow(timeFlowData, 4);
				return;
			}

			OutputText("Error: 'timers'");
			return;
		}

		if (args.Length == 2)    // If 2 args
		{
			if (args[0] == "settime" && float.TryParse(args[1], out float time))  // If setting time and number
			{
				SetTime(timeData, Mathf.Repeat(time + 12f, 24f));
				return;
			}

			if (args[0] == "setflow" && float.TryParse(args[1], out float flow))  // If setting flow and number
			{
				timeflow = flow;
				SetFlow(timeFlowData, flow);
				return;
			}

			OutputText("Error: 'settime' or 'setflow' must be followed by a number");
			return;
		}

		OutputText("Error: You dun goofed");
		return;
	}

	void SetTime(LevelTime timeData, float num)
	{
		timeData.GetTimer("currTime").currTime = num;
		string time = ModMaster.GetCurrentTime();
		OutputText("Set time to " + time);
		return;
	}

	float savedTimeFlow = 4;
	void SetFlow(LevelTimeUpdater timeFlowData, float num)
	{
		timeFlowData._hoursPerMinute = num;
		savedTimeFlow = timeFlowData._hoursPerMinute;
		OutputText("Time now flows at " + num + " hours/minute");
		return;
	}

	public bool timersEnabled = true;
	void SetTimers()
	{
		timersEnabled = !timersEnabled;
		GameObject[] gos = Resources.FindObjectsOfTypeAll<GameObject>();
		List<TimerTrigger> timers = new List<TimerTrigger>();
		foreach (GameObject go in gos)
		{
			if (go.GetComponent<TimerTrigger>() != null)
			{
				timers.Add(go.GetComponent<TimerTrigger>());
			}
		}
		if (timers.Count == 0)   // If no timers found, error
		{
			OutputText("Error: No timers found in this scene.");
			return;
		}
		if (timersEnabled)  // If timers enabled
		{
			OutputText("Timers are enabled.");
		}
		else // If timers disabled
		{
			foreach (TimerTrigger timer in timers)
			{
				timer.timer = 0f;
			}
			OutputText("Timers are disabled!");
		}
		return;
	}

	//-------------------------------------
	// Set HP
	//-------------------------------------
	public void SetHP(string[] args)
	{
		if (args.Length < 1)
		{
			OutputText("Error: hp needs parameters\n" +
				    "Parameters:\n" +
					   "-full: Heal the player fully\n" +
					   "-addhp <float>: Heal the player by the chosen ammount (negative to hurt instead)\n" +
				    "-sethp <float>: Sets the player's current HP to the chosen value\n" +
				    "-addmaxhp <float>: Adds to the player the chosen ammount of HP (negative to substract)\n" +
				    "-setmaxhp <float>: Sets the player's max HP to the chosen value\n\n" +
				    "Example: hp -addhp 4");
			return;
		}
		string output = "";

		Killable entityComponent = player.GetComponent<Entity>().GetEntityComponent<Killable>();
		if (entityComponent != null)
		{
			float float_value;
			bool invalidValue;

			//Max HP
			if (GetArg("-setmaxhp", out float_value, args, out invalidValue))
			{
				output += "Setting max HP from " + entityComponent.MaxHp + " to " + float_value + "\n";
				entityComponent.MaxHp = float_value;
				if (entityComponent.MaxHp < entityComponent.CurrentHp) { entityComponent.CurrentHp = entityComponent.MaxHp; }
			}
			if (invalidValue) { output += "Incorrect -setmaxhp float value\n"; }
			if (GetArg("-addmaxhp", out float_value, args, out invalidValue))
			{
				output += "Adding " + float_value + " max HP\n";
				float newHPValue = entityComponent.MaxHp + float_value;
				if (newHPValue < 1) { newHPValue = 1; }
				entityComponent.MaxHp = newHPValue;
				if (entityComponent.MaxHp < entityComponent.CurrentHp) { entityComponent.CurrentHp = entityComponent.MaxHp; }
			}
			if (invalidValue) { output += "Incorrect -addmaxhp float value\n"; }

			//Current HP
			if (GetArg("-sethp", out float_value, args, out invalidValue))
			{
				output += "Setting current HP from " + entityComponent.CurrentHp + " to " + float_value + "\n";
				entityComponent.CurrentHp = float_value;
			}
			if (invalidValue) { output += "Incorrect -sethp float value\n"; }
			if (GetArg("-addhp", out float_value, args, out invalidValue))
			{
				output += "Adding " + float_value + " HP\n";
				entityComponent.CurrentHp += float_value;
			}
			if (invalidValue) { output += "Incorrect -addhp float value\n"; }
			if (GetArg("-full", args))
			{
				output += "You are fully healed!\n";
				entityComponent.CurrentHp = entityComponent.MaxHp;
			}

			// Save to file
			ModSaver.SaveFloatToFile("player", "hp", entityComponent.CurrentHp);
			ModSaver.SaveFloatToFile("player", "maxHp", entityComponent.MaxHp);

			OutputText(output);
			return;
		}
		OutputText("Killable not found!");
	}

	//-------------------------------------
	// ShowHUD
	//-------------------------------------
	public bool showHUD = true;
	GameObject overlayCamera;
	public void ShowHUD(string[] args)
	{
		if (args.Length > 0)
		{
			if (ParseBool(args[0], out bool result))
			{
				showHUD = result;
			}
		}
		else
		{
			showHUD = !showHUD;
		}

		OutputText(showHUD ? "HUD enabled" : "HUD disabled");

		if (showHUD == false) { return; }
		foreach (Transform child in overlayCamera.transform)
		{
			switch (child.name)
			{
				case "Outlines":
				case "World":
				case "Faders":
				case "PauseOverlay_anchor":
					break;
				default:
					child.localScale = Vector3.one;
					break;
			}
		}
	}

	string[] prefNames = new string[]
	    {
		  "GameBeats",
		  "DamageGiven",
		  "DamageTaken",
		  "Kills",
		  "Deaths",
		  "ChestsOpened",
		  "LightningsUsed",
		  "HeartsCollected",
		  "HealthHealed",
		  "BestRollTime",
		  "MeleeAttacks",
		  "ForceAttacks",
		  "DynamiteAttacks",
		  "IceAttacks",
		  "ComsUsed"
	    };

	void Stats(string[] args)
	{
		float gameBeatCount = ModSaver.LoadFloatFromPrefs("GameBeats");
		string gameBeatText = "Beaten game: " + gameBeatCount + " time(s)";
		float damageGivenCount = ModSaver.LoadFloatFromPrefs("DamageGiven");
		string damageGivenText = "Damage given: " + damageGivenCount;
		float damageTakenCount = ModSaver.LoadFloatFromPrefs("DamageTaken");
		string damageTakenText = "Damage taken: " + damageTakenCount;
		float enemiesKilledCount = ModSaver.LoadFloatFromPrefs("Kills");
		string enemiesKilledText = "Kills: " + enemiesKilledCount;
		float selfKilledCount = ModSaver.LoadFloatFromPrefs("Deaths");
		string selfKilledText = "Deaths: " + selfKilledCount;
		float chestsOpenedCount = ModSaver.LoadFloatFromPrefs("ChestsOpened");
		string chestsOpenedText = "Chests opened: " + chestsOpenedCount;
		float lightningsUsedCount = ModSaver.LoadFloatFromPrefs("LightningsUsed");
		string lightningsUsedText = "Lightnings used: " + lightningsUsedCount;
		float comsUsedCount = ModSaver.LoadFloatFromPrefs("ComsUsed");
		float heartsCollectedCount = ModSaver.LoadFloatFromPrefs("HeartsCollected");
		float healthHealedCount = ModSaver.LoadFloatFromPrefs("HealthHealed");
		string healthString = "Hearts collected: " + heartsCollectedCount + " (Healed " + healthHealedCount + " HP)";
		float bestRollTimeCount = ModSaver.LoadFloatFromPrefs("BestRollTime");
		string bestRollTimeText = "Longest roll: " + bestRollTimeCount + " second(s)";
		float meleeAttackCount = ModSaver.LoadFloatFromPrefs("MeleeAttacks");
		float forceAttackCount = ModSaver.LoadFloatFromPrefs("ForceAttacks");
		float dynamiteAttackCount = ModSaver.LoadFloatFromPrefs("DynamiteAttacks");
		float iceAttackCount = ModSaver.LoadFloatFromPrefs("IceAttacks");
		string attackText = "Melee attacks: " + meleeAttackCount + " / Force Wand attacks: " + forceAttackCount + "\n" + "Dynamite attacks: " + dynamiteAttackCount + " / Ice Ring attacks: " + iceAttackCount;
		string comsUsedText = "Commands used: " + comsUsedCount;

		string output = gameBeatText + "\n";
		output += damageGivenText + "\n";
		output += damageTakenText + "\n";
		output += enemiesKilledText + "\n";
		output += selfKilledText + "\n";
		output += chestsOpenedText + "\n";
		output += lightningsUsedText + "\n";
		output += healthString + "\n";
		output += bestRollTimeText + "\n";
		output += attackText + "\n";
		output += comsUsedText;

		OutputText(output);
	}

	void ClearPrefs(string[] args)
	{
		if (args.Length < 1)     // If no args given
		{
			string output = "Clears data saved by debugger (stats, progress, etc.). CANNOT BE UNDONE!\n";
			output += "Give a <string> argument to specify which preference to delete.\n";
			output += "Examples: 'clearprefs BestRollTime', 'clearprefs DamageGiven', 'clearprefs all'";
			OutputText(output);
			return;
		}

		if (args.Length > 0)     // If args given
		{
			if (args[0] == "all")
			{
				DeletePrefs("all");
				return;
			}

			foreach (string pref in prefNames)
			{
				if (args[0] == pref)
				{
					DeletePrefs(pref);
					return;
				}
			}

			OutputText("Error: " + args[0] + " is not a valid stat reference");
		}
	}

	void DeletePrefs(string prefToDel)
	{
		if (prefToDel == "all")
		{
			foreach (string pref in prefNames)
			{
				ModSaver.DelPref(pref);
			}

			OutputText("All stats have been reset! o:");
			return;
		}

		ModSaver.DelPref(prefToDel);
		OutputText("Stat '" + prefToDel + "' has been reset!");
	}

	void Progress(string[] args)
	{
		// Description
		string description = "Shows your completion progress for various aspects of the game\n\n";
		string output = description;

		CommonFunctions comFuncs = GameObject.Find("CommonFunctions").GetComponent<CommonFunctions>();
		OutputText(output + comFuncs.UpdateProgress(true, ""));
	}

	void DebugView(string[] args)
	{
		DebugViewManager dvm = DebugViewManager.Instance;
		bool doEnable = !GameObject.Find("DebugView");

		// Help
		if (args.Length < 1)
		{
			string description = "Enables/disables the display of a debug overlay.\nVery useful for debugging and doing nerd stuff! :)";
			string arguments = "• <color=grey><string></color> OR <color=grey><int></color> mode name to debug OR page number";
			string usage = "<i>debug</i> | <i>debug 2</i> | <i>debug chaos</i>";

			dvm.EnableDebugView(doEnable, 1);

			if (doEnable)
			{
				OutputText(ModMaster.GetCommandHelp(description, arguments, usage) + "\n\n" + "<color=green>Debug view enabled! Now debugging page 1!</color>");
				return;
			}

			OutputText(ModMaster.GetCommandHelp(description, arguments, usage) + "\n\n" + "<color=green>Debug view disabled.</color>");
			return;
		}

		doEnable = string.IsNullOrEmpty(dvm.activePage) || (!string.IsNullOrEmpty(dvm.activePage) && !dvm.activePage.Contains(args[0]));

		if (int.TryParse(args[0], out int pageNum))
		{
			dvm.EnableDebugView(doEnable, pageNum);

			if (doEnable)
			{
				OutputText("Debug view enabled! Now debugging page " + args[0] + "!");
				return;
			}

			OutputText("Debug view disabled.");
			return;
		}

		dvm.EnableDebugView(doEnable, 1, args[0]);

		if (doEnable)
		{
			OutputText("Debug view enabled! Now debugging " + args[0] + " mode!");
			return;
		}

		OutputText("Debug view disabled.");
		return;
	}

	ObjectUpdater updatesystem;
	//float realSpeed;
	//Vector2 oldPos;
	private void Update()
	{
		//Make cursor visible if game is paused
		if (updatesystem != null)
		{
			if (updatesystem.IsPaused()) { Cursor.visible = true; }
		}
		else
		{
			updatesystem = GameObject.Find("UpdateSystem").GetComponent<ObjectUpdater>();
		}

		//Hide (scale down) hud if showhud is false
		if (showHUD) { return; }
		foreach (Transform child in overlayCamera.transform)
		{
			switch (child.name)
			{
				case "Outlines":
				case "World":
				case "Faders":
				case "PauseOverlay_anchor":
					break;
				default:
					child.localScale = Vector3.zero;
					break;
			}
		}
	}

	private void LateUpdate()
	{
		if (bighead)
		{
			if (ittle_head != null)
			{
				ittle_head.localScale = Vector3.one * 2.5f;
			}
			else if (ittle_head == null && player != null)
			{
				ittle_head = player.transform.Find("Ittle").Find("ittle").Find("Armature").Find("root").Find("chest").Find("head");
			}
		}
	}

	// Updates Ittle's outfit
	public void SetOutfit(string[] args)
	{
		if (args.Length < 1)
		{
			string description = "Sets the outfit Ittle is currently wearing. Works instantly!";
			string arguments = "<color=grey><int></color> outfit number OR <color=grey><string></color> <i>random</i>";
			string usage = "<i>outfit 10</i> (sets outfit to Jenny Berry) | <i>outfit random</i> (randomizes outfit on\nevery load)";
			string values1 = "0 = Ittle, 1 = Tippsie, 2 = ID1, 3 = Jenny, 4 = swimsuit, 5 = fierce diety\n";
			string values2 = "6 = Alt Ittle, 7 = delinquent, 8 = frog, 9 = That Guy, 10 = Jenny Berry";

			OutputText(ModMaster.GetCommandHelp(description, arguments, usage, values1 + values2));
			return;
		}

		// If randomizing
		if (args[0] == "random")
		{
			if (comFuncs.ChangeOutfit(0, true))
			{
				OutputText("Outfit will be randomzied on every load!");
				return;
			}
		}

		if (int.TryParse(args[0], out int outfitNum))
		{
			if (comFuncs.ChangeOutfit(outfitNum))
			{
				OutputText("Outfit changed! Lookin' stylish!");
				return;
			}
		}

		OutputText("Error: " + args[0] + " is not a valid outfit number. Must be 0 - 10");
	}

	// Set flags on save file
	void SetFlags(string[] args)
	{
		// Help
		if (args.Length < 1)
		{
			string description = "Sets specified flag on save file. Only takes affect after map reload\nSupports: doors, gates, chests, dungeon keys, and breakable walls\n<color=maroon>You must first save an object to memory by using <i>find objName -save</i></color>";
			string arguments = "• <color=grey><string></color> <i>save</i> OR <i>load</i>\n• <color=grey><int></color>/<color=grey><float></color>/<color=grey><bool></color>/<color=grey><string></color> value";
			string usage1 = "Step 1: <i>find puzzledoor_locked -save</i> | <i>find puzzledoor_red - save</i>\n";
			string usage2 = "Step 2: <i>setflags save 1</i> | <i>setflags load</i>";

			OutputText(ModMaster.GetCommandHelp(description, arguments, usage1 + usage2));
			return;
		}

		// If saving or loading data
		if (args.Length > 0 && args[0] == "save" || args[0] == "load")
		{
			string saver = "";
			string saveName = "";
			string value = "";

			List<string> levelFlags = new List<string>()
			{
				"door", // Boss door, locked doors, enemy doors, puzzle doors, trap doors
				"gate", // Red gates
				"enemychest", // Red chests
				"puzzlechest", // Green chests
				"keychest", // Dungeon keys
				"breakablewall", // Breakable walls
			};

			// If 2 args
			if (args.Length == 2)
			{
				// If saved reference is found
				if (savedFind != null)
				{
					value = args[1];

					// If level flag
					if (levelFlags.Any(savedFind.name.ToLower().Contains))
					{
						saver = "levels/" + ModMaster.GetMapName() + "/" + ModMaster.GetMapRoom();
						saveName = savedFind.GetComponent<DummyAction>()._saveName;
					}
				}
				else // If saved refrence not found, error
				{
					OutputText("Error: You must save an object reference using 'find'.");
					return;
				}
			}
			else // If not enough or too many args, error
			{
				OutputText("Error: Too many or too few args.\nType command again to see usage instructions");
				return;
			}

			// If saving
			if (args[0] == "save" && !string.IsNullOrEmpty(value))
			{
				if (int.TryParse(value, out int out1)) // If int
				{
					// Save int to file
					ModSaver.SaveIntToFile(saver, saveName, out1, true);
				}
				else if (float.TryParse(value, out float out2)) // If float
				{
					// Save float to file
					ModSaver.SaveFloatToFile(saver, saveName, out2, true);
				}
				else if (bool.TryParse(value, out bool out3)) // If bool
				{
					// Save bool to file
					ModSaver.SaveBoolToFile(saver, saveName, out3, true);
				}
				else // If string
				{
					// Save string to file
					ModSaver.SaveStringToFile(saver, saveName, value, true);
				}

				OutputText(PlayerPrefs.GetString("test"));
				return;
			}

			// If loading
			if (args[0] == "load")
			{
				ModSaver.LoadIntFromFile(saver, saveName, true);
				OutputText(PlayerPrefs.GetString("test"));
				return;
			}
		}
	}

	// Kills player or all nearby enemies
	public void Kill(string[] args)
	{
		if (args.Length < 1)
		{
			string description = "Kills all nearby enemies... or yourself, if you wish...";
			string arguments = "<color=grey><string></color> <i>self</i> OR <i>enemies</i>";
			string usage = "<i>kill self</i> (commits suicide) | <i>kill enemies</i> (kills enemies)";

			OutputText(ModMaster.GetCommandHelp(description, arguments, usage));
			return;
		}

		// If killing self
		if (args.Length > 0 && args[0] == "self")
		{
			OutputText("You have given up on life. You found life to be too difficult. Goodbye... :'(");
			player.transform.Find("Hittable").GetComponent<Killable>().SignalDeath();
			return;
		}

		if (args.Length > 0 && args[0] == "enemies")
		{
			foreach (Killable killable in GameObject.FindObjectsOfType(typeof(Killable)))
			{
				if (killable.transform.parent.name != "PlayerEnt")
				{
					killable.SignalDeath();
				}
			}

			OutputText("You have killed so many helpless creatures! How could you?!?\nYou are EEEEEEVVVVVVIIIIIILLLLLLLLLL!!!!!");
		}
	}

	// Makes Ittle invisible
	bool isInvisible;
	void Invisible(string[] args)
	{
		string description = "Makes Ittle invisible/visible! Type this again to change it.";

		GameObject anim = GameObject.Find("Ittle").transform.Find("ittle").gameObject;
		GameObject dust = player.transform.Find("MoveEffect").gameObject;
		SkinnedMeshRenderer mesh = anim.transform.Find("Cube").GetComponent<SkinnedMeshRenderer>();
		MeshRenderer shadow = null;

		foreach (MeshRenderer shadow2 in GameObject.FindObjectsOfType<MeshRenderer>())
		{
			float shadowX = shadow2.transform.position.x;
			float shadowZ = shadow2.transform.position.z;
			float playerX = player.transform.position.x;
			float playerZ = player.transform.position.z;

			if (shadowX == playerX && shadowZ == playerZ)
			{
				shadow = shadow2;
			}
		}

		anim.SetActive(!anim.activeSelf);
		dust.SetActive(!dust.activeSelf);
		mesh.enabled = !mesh.enabled;
		shadow.enabled = !shadow.enabled;

		if (!mesh.enabled)
		{
			isInvisible = true;
			OutputText(description + "\n\n<color=green>Ittle is now invisible!</color>");
			return;
		}
		isInvisible = false;
		OutputText(description + "\n\n<color=green>Ittle is now visible!</color>");
	}

	// Randomizes all sound effects
	Dictionary<SoundClip, AudioClip> vanillaSounds = new Dictionary<SoundClip, AudioClip>();
	bool hasRandomizedSounds;
	void RandomSounds(string[] args)
	{
		string description = "Randomizes most sound effects! Type again to reset them.";

		if (!hasRandomizedSounds)
		{
			List<AudioClip> sounds = new List<AudioClip>();

			// Get list of all available sounds
			foreach (AudioClip sound in Resources.FindObjectsOfTypeAll<AudioClip>())
			{
				// Only get short sound effects
				if (sound.length < 1)
				{
					sounds.Add(sound);
				}
			}

			// Randomizes sounds
			foreach (SoundClip sound in Resources.FindObjectsOfTypeAll<SoundClip>())
			{
				int num = UnityEngine.Random.Range(0, sounds.Count - 1);

				if (sound._rawSound.length > 2)
				{
					continue;
				}

				vanillaSounds.Add(sound, sound._rawSound);
				sound._rawSound = sounds[num];

				// Set each variation the same
				if (sound._variations.Length > 0)
				{
					for (int i = 0; i < sound._variations.Length; i++)
					{
						sound._variations[i]._rawSound = sounds[num];
					}
				}
			}

			OutputText(description + "\n\n<color=green>All sounds are now randomized! Enjoy these dank tunes!</color>");
			hasRandomizedSounds = true;
		}
		else
		{
			foreach (SoundClip sound in Resources.FindObjectsOfTypeAll<SoundClip>())
			{
				if (vanillaSounds.TryGetValue(sound, out AudioClip clip))
				{
					sound._rawSound = clip;
					vanillaSounds.Remove(sound);
				}
			}

			OutputText(description + "\n\n<color=green>Sounds are back to defaults. Didn't enjoy the dank tunes?</color>");
			hasRandomizedSounds = false;
		}
	}

	/* COMMENTED OUT TO WORK OUT ISSUES. MAIN ISSUE: CAN'T REFERENCE GAMEOBJECT FROM MATERIAL AND MATS HAVE DUMB NAMES.
	// Swaps textures
	string textureChangedTo = "";
	void TextureSwap(string[] args)
	{
		if (args.Length < 1)
		{
			string description = "Replace Ittle's texture with one of an NPC's!";
			string arguments = "• <color=grey><string></color> objName";
			string usage = "<i>textureswap safetyjenny</i> | <i>textureswap </i> | <i></i>";

			OutputText(ModMaster.GetCommandHelp(description, arguments, usage));
			return;
		}

		SkinnedMeshRenderer ittleMesh = ModMaster.FindNestedChild(player, "Cube", "ittle").GetComponent<SkinnedMeshRenderer>();
		string name = args[0];

		foreach (Material mat in Resources.FindObjectsOfTypeAll<Material>())
		{
			if (mat.name.ToLower() == name)
			{
				ittleMesh.materials = new Material[]
				{
					ittleMesh.materials[0],
					mat,
					ittleMesh.materials[2],
				};

				textureChangedTo = name;
				OutputText("Changed Ittle's texture to " + mat.name + "!");
				return;
			}
		}

		OutputText("Could not find mesh for " + name);
	}
	*/

	/* Swap models (COMMENTED OUT TO FINALIZE IT)
	public GameObject swapModelGfx = null;
	void ModelSwap(string[] args)
	{
		if (args.Length < 0)
		{
			OutputText("NPC name needed!");
			return;
		}
		if (args[0] == "reset")
		{
			if (swapModelGfx != null) { Destroy(swapModelGfx); }
			OutputText("Model set to default. Re-enter the scene to apply changes");
			return;
		}
		GameObject[] entities = FindEntityAndController(args[0]);
		if (entities[0] == null)
		{
			OutputText("NPC " + args[0] + " not found!");
			return;
		}
		GameObject gfx;
		if (entities[0].GetComponent<EntityGraphics>() != null)
		{ gfx = entities[0].GetComponent<EntityGraphics>().gameObject; }
		else
		{ gfx = entities[0].GetComponentInChildren<EntityGraphics>().gameObject; }

		if (gfx == null)
		{
			OutputText("GFX not found!");
			return;
		}

		if (swapModelGfx != null) { Destroy(swapModelGfx); }
		swapModelGfx = Instantiate(gfx);
		GameObject ittle = player.transform.Find("Ittle").gameObject;
		Transform smallittle = ittle.transform.Find("ittle");
		Animation originalAnims = smallittle.GetComponent<Animation>();
		Animation newAnims = swapModelGfx.GetComponent<Animation>();
		newAnims.AddClip(originalAnims.GetClip("idle"), "idle");
		newAnims.AddClip(originalAnims.GetClip("run"), "run");
		newAnims.AddClip(originalAnims.GetClip("sword"), "sword");
		newAnims.AddClip(originalAnims.GetClip("forcewand"), "forcewand");
		newAnims.AddClip(originalAnims.GetClip("roll"), "roll");
		newAnims.AddClip(originalAnims.GetClip("rollstart"), "rollstart");
		newAnims.AddClip(originalAnims.GetClip("rollloop"), "rollloop");
		newAnims.AddClip(originalAnims.GetClip("rollend"), "rollend");
		newAnims.AddClip(originalAnims.GetClip("chargeefcs"), "chargeefcs");
		newAnims.AddClip(originalAnims.GetClip("efcs"), "efcs");
		newAnims.AddClip(originalAnims.GetClip("death"), "death");
		newAnims.AddClip(originalAnims.GetClip("pushing"), "pushing");
		newAnims.AddClip(originalAnims.GetClip("holefall"), "holefall");
		newAnims.AddClip(originalAnims.GetClip("warp"), "warp");
		newAnims.AddClip(originalAnims.GetClip("strongidle"), "strongidle");
		swapModelGfx.name = "Ittle";

		OutputText("Set Ittle's model to " + args[0]);

		DontDestroyOnLoad(swapModelGfx);
	}*/

	// Play selected song or random song
	void PlaySong(string[] args)
	{
		if (args.Length < 1)
		{
			string description1 = "Plays the specified song. Persists through loading zones; resets on main menu.\n";
			string description2 = "<color=maroon>You must enter scenes to get their songs. Once you do, you keep the song\nfor rest of session.</color>";
			string description3 = "<color=maroon> To get song name, use <i>playsong getnames # (songIndex)</i>\nand use that name!</color>";
			string arguments = "• <color=grey><string></color> songName/<i>random</i>/<i>getcount</i>\n• <color=grey><string></color><i>getname</i> <color=grey><int></color> songIndex";
			string usage = "<i>playsong boss</i> | <i>playsong deepmadness</i> | <i>playsong random</i> | <i>playsong getcount</i>\n<i>playsong getnames 2</i>";

			OutputText(ModMaster.GetCommandHelp((description1 + description2 + description3), arguments, usage));
			return;
		}

		CommonFunctions comFuncs = GameObject.Find("CommonFunctions").GetComponent<CommonFunctions>();
		string arg = args[0];

		if (args[0] == "getname")
		{
			if (comFuncs.loadedSongs.Count < int.Parse(args[1]))
			{
				MusicSong song = comFuncs.loadedSongs[int.Parse(args[1])];
				OutputText("Found song " + song._songClip._clip.name);
			}
			else
			{
				OutputText("Error: Only " + comFuncs.loadedSongs.Count + " songs were found.");
			}

			return;
		}

		if (args[0] == "getcount")
		{
			OutputText("Found " + comFuncs.loadedSongs.Count + " songs");
			return;
		}

		// If random
		if (arg == "random")
		{
			comFuncs.ChangeSong(arg, 2);
			OutputText("Now playing a random song!");
			return;
		}

		if (comFuncs.ChangeSong(args[0], 2))
		{
			OutputText("Now playing " + args[0]);
			return;
		}

		OutputText("Error: Did not find song " + args[0] + " out of " + comFuncs.loadedSongs.Count + " songs");
	}

	// Triggers specified weather event
	public void Weather(string[] args)
	{
		if (args.Length < 1)
		{
			string description1 = "Starts the specified weather event. Loading will stop it.\n";
			string description2 = "<color=maroon>You must enter scenes to get their weather events. Once you do, you keep\nthe reference for rest of session</color>";
			string arguments = "• <color=grey><string></color> weatherName";
			string usage = "<i>weather eruption</i> | <i>weather rain</i> | <i>weather snow</i>";
			string values = "volcano/eruption/meteor | rain | snow | sun/sunny | monochrome/nightcave";

			OutputText(ModMaster.GetCommandHelp((description1 + description2), arguments, usage, values));
			return;
		}

		else if (comFuncs.ChangeWeather(args[0].ToLower()))
		{
			OutputText("Current weather changed to " + args[0] + "!");
			return;
		}

		OutputText("Error: Weather event " + args[0] + " not found.\nIt might not be loaded or not exist at all.\nType '<i>weather</i>' for info on how to use!");
		return;
	}

	//-------------------------------------
	//Test 
	//-------------------------------------
	private void Test(string[] args)
	{
        string output = "";
        float.TryParse(args[0], out float x);
        float.TryParse(args[1], out float y);

        Vector2 arraySize = new Vector2(x, y);
        Vector2 btnScale = new Vector2(1f, 1f);
        float btnLength = 1f;
        float xSeparation = 3.4f * btnScale.x * btnLength;
        float ySeparation = -1.6f * btnScale.y;
        float initialX = xSeparation * (-0.5f) * (arraySize.x - 1);
        float initialY = 1.6f;
        Vector2 backgroundScale = new Vector2(0.75f * btnLength, 1.5f);
        //Instantiate array
        for (int i = 0; i < (int)(arraySize.x * arraySize.y); i++)
        {
            float buttonXPos = initialX + ((float)(i % (int)arraySize.x)) * xSeparation;
            output += i + "//X pos: " + buttonXPos.ToString();
            float buttonYPos = initialY + ((float)(i / ((int)arraySize.x))) * ySeparation;
            output += "/ Y pos: " + buttonYPos.ToString() + "\n";
        }
        OutputText(output);
    }
    UIScrollMenu myBar;

    public void TestButton()
    {
        Spawn(new string[] { "Fishbun" });
    }

    private void Test2(string[] args)
	{
        return;
        int x = 0;
        float y = 0f;
        Transform uiParent = GameObject.Find("Debug").transform;
        if (args.Length >= 2)
        {
            if (!(int.TryParse(args[0], out x) && float.TryParse(args[1], out y)))
            {
                OutputText("Invalid values!");
                return;
            }
        }
        else
        {
            OutputText("More args required!");
            return;
        }

        switch(x)
        {
            case 0:
                myBar.CanvasHeight = y;
                break;
            case 1:
                myBar.CanvasWindow = y;
                break;
            case 2:
                myBar.EmptySpace = y;
                break;
            default:
                OutputText("Invalid int!");
                break;
        }
    }

	public void Anticheat(IDataSaver saver)
	{
		ModMaster.SetNewGameData("mod/anticheat", "1", saver);
		PlayerPrefs.SetString("test", "Anticheat is active! Hoorah!");
	}

	// Activates a status effect to Ittle or nearest enemy
	// Format: "status cold 0" = stop for ittle | "status cold 15 ent/enemy"
	void SetStatus(string[] args)
	{
		StatusManager sm = StatusManager.Instance;
		if (!sm.hasSetStatus) { sm.GetStatuses(); }

		// Help
		if (args.Length == 0)
		{
			string description = "Activates a status buff/debuff on an entity. Courage, Curse and\nFortune are scrapped statuses and do not have a visual effect";
			string arguments = "• <color=grey><string></color> name | <color=grey><float/string></color> duration/x = infinite (optional) | <color=grey><string></color> enemy";
			string usage = "<i>status cold</i> | <i>status direhit 60</i> | <i>status fortune 0</i> | <i>status fear 47.018 enemy</i>";
			string values = "cold | courage | curse | direhit | fear | fortune\nfragile | hearty | mighty | tough | weak";

			OutputText(ModMaster.GetCommandHelp(description, arguments, usage, values));
			return;
		}

		string wantedStatusName = args[0];
		bool isValidName = !string.IsNullOrEmpty(sm.GetNameOfStatus(wantedStatusName));
		bool applyToEnt = false;
		bool isInfinite = false;
		bool doStop = false;
		float duration = 0.0f;
		string output;

		// Check if status name exists
		if (isValidName)
		{
			string realStatusName = sm.GetNameOfStatus(wantedStatusName);

			// Status info
			if (args.Length == 1)
			{
				Dictionary<string, string> statusInfo = new Dictionary<string, string>()
				{
					{ "cold", "Cold status\n\nWhen applied to Ittle, slows Ittle movement by 25%.\nWhen applied to enemies, slows enemy movement by 25% and prevents\nmost from shooting projectiles." },
					{ "courage", "Courage status\n\nThis status was scrapped during development.\nWe believe it was replaced with Tome, as it has similar effect.\nWhen applied to Ittle, it prevents negative status effects from all\ndamage sources.\nNo effect on enemies." },
					{ "curse", "Curse status\n\nThis status was scrapped during development.\nWhen applied to Ittle, it prevents all enemy drops.\nNo effect on enemies." },
					{ "direhit", "Dire Hit status\n\nWhen applied to Ittle, it gives Ittle's melee attacks a 25% chance to\nkill an enemy with 16 HP or less remaining.\nNo effect on enemies." },
					{ "fear", "Fear status\n\nWhen applied to Ittle or enemies, it prevents them from attacking.\nApplying to enemies can cause some interesting results!" },
					{ "fortune", "Fortune status\n\nThis status was scrapped during development.\nWe believe it was replaced with the Hearty status and sunny day weather.\nWhen applied to Ittle, all enemies will always have drops when killed.\nNo effect on enemies." },
					{ "fragile", "Fragile status\n\nWhen applied to Ittle, makes all neutral damage sources do +1 extra damage.\nNo effect on enemies." },
					{ "hearty", "Hearty status\n\nWhen applied to Ittle, makes all enemies drop blue or yellow hearts when killed.\nNo effect on enemies." },
					{ "mighty", "Mighty status\n\nWhen applied to Ittle, makes all of Ittle's netural attacks do +1 extra damage.\nNo effect on enemies." },
					{ "tough", "Tough status\n\nWhen applied to Ittle, makes all damage sources do -1 less damage.\nNo effect on enemies." },
					{ "weak", "Weak status\n\nWhen applied to Ittle, makes all of Ittle's attacks do half damage.\nNo effect on enemies." }
				};

				if (statusInfo.TryGetValue(args[0].ToLower(), out string description))
				{
					OutputText(description);
				}
				// This should not happen
				else { OutputText("ERROR: '" + wantedStatusName + "' is not a valid status name.\nuhhhh you should not be seeing this... Please report!"); }

				return;
			}

			// Check if duration given
			if (args.Length > 1 && float.TryParse(args[1], out float time))
			{
				// Check if stopping status
				if (time <= 0) { doStop = true; }
				else { duration = time; }
			}
			// Check if infinite duration
			else if (args.Length > 1 && args[1] == "x") { isInfinite = true; }
			else if (args.Length > 1) { OutputText("ERROR: '" + args[1] + "' is not 'x'"); return; }

			// Check if applying to ent
			if (args.Length > 2 && (args[2] == "ent" || args[2] == "enemy")) { applyToEnt = true; }
			else if (args.Length > 2) { OutputText("ERROR: '" + args[2] + "' is not 'ent' or 'enemy'"); return; }

			// Apply status

			// If applying to ent
			if (applyToEnt)
			{
				// Check if there is saved object
				if (savedFind != null)
				{
					// Check if stopping status and specified status is active
					if (doStop && !sm.SetStatus(realStatusName, isInfinite, duration, savedFind))
					{
						output = "Removing " + realStatusName + " status to " + savedFind.name;
					}
					else
					{
						sm.SetStatus(realStatusName, isInfinite, duration, savedFind);
						output = "Applying " + realStatusName + " status to " + savedFind.name;

						// Check for durations
						if (isInfinite) { output += " indefinitely!"; }
						else if (duration > 0) { output += " for " + duration + " seconds!"; }
					}
				}
				// If there is no saved object
				else
				{
					List<GameObject> statusableObjs = new List<GameObject>();

					foreach (EntityStatusable statusActivator in Resources.FindObjectsOfTypeAll<EntityStatusable>())
					{
						statusableObjs.Add(statusActivator.gameObject);
					}

					// Find which object is closest to Ittle
					GameObject applyToThis = ModMaster.FindClosestObject(player, statusableObjs);

					// Check if stopping status and specified status is active
					if (doStop)
					{
						sm.StopStatus(sm.GetStatusType(realStatusName), applyToThis);
						output = "Removing " + realStatusName + " status from " + applyToThis.name;
					}
					else
					{
						sm.SetStatus(realStatusName, isInfinite, duration, applyToThis);
						output = "Applying " + realStatusName + " status to " + applyToThis.name;

						// Check for durations
						if (isInfinite) { output += " indefinitely!"; }
						else if (duration > 0) { output += " for " + duration + " seconds!"; }
					}
				}
			}
			// If applying to Ittle
			else
			{
				// Check if stopping status and specified status is active
				if (doStop)
				{
					sm.StopStatus(sm.GetStatusType(realStatusName));
					output = "Removing " + realStatusName + " status from Ittle";
				}
				else
				{
					sm.SetStatus(realStatusName, isInfinite, duration, player);
					output = "Applying " + realStatusName + " status to Ittle";

					// Check for durations
					if (isInfinite) { output += " indefinitely!"; }
					else if (duration > 0) { output += " for " + duration + " seconds!"; }
				}
			}
		}
		// If name is not valid
		else { output = "ERROR: '" + wantedStatusName + "' is not a valid status name"; }

		OutputText(output);
	}
}