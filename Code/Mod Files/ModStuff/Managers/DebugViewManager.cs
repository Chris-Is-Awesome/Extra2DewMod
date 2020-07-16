using UnityEngine;
using System.Collections.Generic;

namespace ModStuff
{
	public class DebugViewManager : Singleton<DebugViewManager>
	{
		// Obj vars
		GameObject debugObj;
		Vector3 debugPos = new Vector3(-5f, 2f, 0f);
		TextMesh debugText;
		Font cutsceneFont;
		Material fontMat;
		// Text vars
		float charSize = 0.018f;
		int size = 80;
		// Logic
		bool doEnable;
		int page;
		string mode;
		int delay = 4; // Delay updates by this many frames
		public string activePage;

		// Initialization
		void Initialize()
		{
			if (doEnable)
			{
				currDelay = delay;
				GetReferences();
				GameStateNew.OnPlayerSpawn += OnPlayerSpawn;
			}
		}

		// Enables debug view
		public void EnableDebugView(bool enable, int pageNum, string modeName = "")
		{
			doEnable = enable;
			page = 0;
			mode = "";

			if (enable)
			{
				page = pageNum;
				mode = modeName;
				Initialize();
				MakeDebugObject();
			}
			else
			{
				activePage = "";
				Destroy(debugObj);
			}
		}

		// Makes the debug view object
		void MakeDebugObject()
		{
			if (debugObj == null)
			{
				debugObj = new GameObject("DebugView");
				debugObj.transform.parent = GameObject.Find("OverlayCamera").transform;
				debugObj.transform.localPosition = debugPos;
				debugObj.transform.localEulerAngles = Vector3.zero; // Needed for proper text scaling
				debugObj.layer = 9; // Set to UI layer
			}

			// Deactivate map and hint buttons to free up more space
			GameObject.Find("OverlayCamera").transform.Find("MapButtonIcon_anchor").gameObject.SetActive(!doEnable);
			GameObject.Find("OverlayCamera").transform.Find("InfoButtonIcon_anchor").gameObject.SetActive(!doEnable);

			MakeDebugText();
		}

		// Makes the debug view text
		void MakeDebugText()
		{
			if (debugText == null)
			{
				if (cutsceneFont == null || fontMat == null)
				{
					foreach (Font font in Resources.FindObjectsOfTypeAll(typeof(Font)) as Font[])
					{
						if (font.name == "Cutscene")
						{
							cutsceneFont = font;
							fontMat = FontMaterialMap.LookupFontMaterial(font);
							break;
						}
					}
				}

				debugText = debugObj.AddComponent<TextMesh>();
				debugText.font = cutsceneFont;
				debugText.GetComponent<UnityEngine.MeshRenderer>().sharedMaterial = fontMat;
				debugText.alignment = TextAlignment.Left;
				debugText.color = Color.white;
				debugText.characterSize = charSize;
				debugText.fontSize = size;
			}

			UpdateDebugText();
		}

		// Updates the debug text
		void UpdateDebugText()
		{
			if (page > 0) { debugText.text = GetText(page); }
			if (!string.IsNullOrEmpty(mode)) { debugText.text = GetText(mode); }
		}

		string GetText(int page)
		{
			switch (page)
			{
				case 1:
					return Page1();
				case 2:
					return Page2();
			}

			return null;
		}

		string GetText(string mode)
		{
			switch (mode)
			{
				case "chaos":
					return ModeChaos();
				case "rando":
					return ModeRando();
			}

			return null;
		}

		string ModeChaos()
		{
			string output = "Debugging of chaos mode coming soon...";

			// Mode: Chaos
			// Active effects + duration + result?

			//output += "--- CHAOS MODE ---" + "\n";
			// Check if active or not

			activePage = "ModeChaos";
			return output;
		}

		string ModeRando()
		{
			string output = "Debugging of entrance randomizer coming soon...";

			// Mode: Entrance randomizer
			// Nearest entrance leads to... ?

			activePage = "ModeRando";
			return output;
		}

		string Page1()
		{
			string output = "";

			// Player stuff
			Transform playerTrans = player.transform;
			Transform ittleTrans = ittle.transform;

			output += "--- PLAYER ---" + "\n";
			output += "Pos: " + playerTrans.position.ToString() + "\n";
			output += "Angle: " + Mathf.Floor(playerTrans.localEulerAngles.y) + "\n";
			if (ittleTrans.localScale != Vector3.one) { output += "Scale: " + ittleTrans.localScale.ToString() + "\n"; }
			output += "Speed: " + speed + "\n";

			// Map stuff
			string sceneName = ModMaster.GetMapName();
			string roomName = ModMaster.GetMapRoom();
			string spawnName = ModMaster.GetMapSpawn();

			output += "--- MAP ---" + "\n";
			output += "Scene: "+ sceneName + "\n";
			output += "Room: " + roomName + "\n";
			output += "Spawn: " + spawnName;
			if (!spawnName.Contains(sceneName)) { output += " (" + sceneName + ")" + "\n"; }
			else { output += "\n"; }

			// World stuff
			output += "--- WORLD ---" + "\n";
			output += "Time: " + ModMaster.GetCurrentTime() + " (" + timeHandler._hoursPerMinute + " h/m)" + "\n";
			foreach (LevelEvent levelEvent in Resources.FindObjectsOfTypeAll<LevelEvent>())
			{
				if (levelEvent.Timer > 0)
				{
					if (levelEvent.name == "VolcanoEvent") { output += "Volcanic eruption: " + levelEvent.StartTime + "\n"; }
					else if (levelEvent.name == "RainEvent") { output += "Rain: " + levelEvent.Timer + "\n"; }
					else if (levelEvent.name == "BoogalooEvent") { output += "Sunny day: " + levelEvent.Timer + "\n"; }
					else if (levelEvent.name == "CaveSpiderEvent") { output += "Spooky night: " + levelEvent.Timer + "\n"; }
				}
			} // Weather
			if (sceneName == "VitaminHills3")
			{
				GameObject cowUfo = GameObject.Find("Countdown");

				if (cowUfo != null)
				{
					TimerTrigger cowUfoTimer = cowUfo.GetComponent<TimerTrigger>();

					if (cowUfoTimer != null && cowUfoTimer.timer > 0 && cowUfoTimer.timer < cowUfoTimer._time)
					{
						output += "Cow UFO: " + cowUfoTimer.timer + "\n";
					}
				}
			} // Cow UFO timer
			if (sceneName == "Deep13")
			{
				GameObject remedyWaiter = GameObject.Find("TimedWarper(Clone)");

				if (remedyWaiter != null)
				{
					TimedTouchTrigger warpTimer = remedyWaiter.transform.Find("Logic").GetComponent<TimedTouchTrigger>();

					if (warpTimer != null && warpTimer.isInTrigger)
					{
						float time = warpTimer.colliders[0].timer;

						if (time > 0 && time < warpTimer._time)
						{
							output += "Remedy warp: " + warpTimer.colliders[0].timer + "\n";
						}
					}
				}
			} // Remedy warp timer

			// Mode stuff
			if (!ModeControllerNew.IsVanilla)
			{
				output += "--- ACTIVE MODES ---" + "\n";

				for (int i = 0; i < ModeControllerNew.activeModes.Count; i++)
				{
					output += ModeControllerNew.activeModes[i].ToString() + "\n";
				}
			}

			// Cheat stuff
			if (debugger.godmode || debugger.likeABoss || debugger.noclip)
			{
				output += "--- ACTIVE CHEATS ---" + "\n";
				if (debugger.godmode) { output += "God" + "\n"; }
				if (debugger.likeABoss) { output += "LikeABoss" + "\n"; }
				if (debugger.noclip) { output += "NoClip" + "\n"; }
			}

			activePage = "Page1";
			return output;
		}

		string Page2()
		{
			string output = "Page 2 debugging (drop table stuff) coming soon...";

			// Droptable stuff
			// Total (out of 30)
			// Tier 1 stuff
			// Tier 1 count
			// Tier 1 next drop
			// Tier 2 stuff
			// Tier 2 count
			// Tier 2 next drop
			// Tier 3 stuff
			// Tier 3 count
			// Tier 3 next drop
			// Tier 4 stuff
			// Tier 4 count
			// Tier 4 next drop

			activePage = "Page2";
			return output;
		}

		GameObject player;
		GameObject ittle;
		DebugCommands debugger;
		LevelTimeUpdater timeHandler;
		void GetReferences()
		{
			if (player == null) { player = GameObject.Find("PlayerEnt"); }
			if (ittle == null) { ittle = GameObject.Find("Ittle"); }
			if (debugger == null) { debugger = DebugCommands.Instance; }
			if (timeHandler == null) { timeHandler = GameObject.Find("BaseLevelData").transform.Find("TimeStuff").GetComponent<LevelTimeUpdater>(); }
		}

		int currDelay;
		void LateUpdate()
		{
			GetPlayerSpeed();

			if (currDelay > 0) { currDelay--; }
			else
			{
				UpdateDebugText();
				currDelay = delay;
			}
		}

		Vector2 oldPos = Vector2.zero;
		float speed = 0;
		void GetPlayerSpeed()
		{
			// Calculate real player speed
			if (player != null)
			{
				Transform playerTrans = player.transform;
				Vector2 currPos = new Vector2(playerTrans.position.x, playerTrans.position.z);
				if (oldPos != currPos)
				{
					float distance = Vector2.Distance(oldPos, currPos);
					speed = distance / Time.deltaTime;
					oldPos = currPos;
				}
				else { speed = 0; }
			}
		}

		void OnPlayerSpawn(bool isRespawn)
		{
			if (!isRespawn)
			{
				GetReferences();
				MakeDebugObject();
			}
		}
	}
}