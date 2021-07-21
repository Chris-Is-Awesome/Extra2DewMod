using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ModStuff.Options;

namespace ModStuff
{
	public static class GameStateNew
	{
		// Scene events
		public delegate void _OnRoomChange(LevelRoom room);
		public static event _OnRoomChange OnRoomChange;
		public delegate void _OnSceneLoad(Scene scene, bool isGameplayScene);
		public static event _OnSceneLoad OnSceneLoad;

		// Player events
		public delegate void _OnPlayerDeath(bool preAnim);
		public static event _OnPlayerDeath OnPlayerDeath;
		public delegate void _OnPlayerSpawn(bool isRespawn);
		public static event _OnPlayerSpawn OnPlayerSpawn;

		// Enemy events
		public delegate void _OnEntSpawn(Entity ent);
		public static event _OnEntSpawn OnEntSpawn;

		// Attack events
		public delegate void _OnDamageDone(float dmg, Entity toEnt);
		public static event _OnDamageDone OnDamageDone;
		public delegate void _OnEnemyKill(Entity ent);
		public static event _OnEnemyKill OnEnemyKill;

		// Game events
		public delegate void _OnCollision(BC_Collider outgoingCollider, BC_Collider incomingCollider, BC_Collider.EventMode collisionType = BC_Collider.EventMode.Enter, bool isTrigger = false);
		public static event _OnCollision OnCollision;
		public delegate void _OnDungeonComplete(Scene dungeon);
		public static event _OnDungeonComplete OnDungeonComplete;
		public delegate void _OnFileLoad(bool isNew, string fileName = "", string filePath = "", RealDataSaver saver = null);
		public static event _OnFileLoad OnFileLoad;
		public delegate void _OnGameQuit();
		public static event _OnGameQuit OnGameQuit;
		public delegate void _OnGameStart();
		public static event _OnGameStart OnGameStart;
		public delegate void _OnItemGot(Item item);
		public static event _OnItemGot OnItemGot;
		public delegate void _OnSaveFlagSet(string flagName);
		public static event _OnSaveFlagSet OnSaveFlagSet;

		// Vars
		static public bool isPlayerDead;
		static public bool isWarping;

		// --------------- SCENE EVENTS --------------- \\

		// Runs when a room has changed ( listens to LevelRoom.SetCurrentActiveRoom() and LevelRoom.FinishTransition() )
		public static void OnRoomChanged(LevelRoom room)
		{
			//PlayerPrefs.SetString("test", "Rooms changed! Current room: '" + room.RoomName + "'!");

			// Invoke events
			OnRoomChange?.Invoke(room);
		}

		// Runs when a scene is loaded ( listens to LevelData.Start(), and MainMenu.Start() )
		public static void OnSceneLoaded(Scene scene, bool isGameplayScene)
		{
			//PlayerPrefs.SetString("test", "A scene was loaded! Current scene: '" + scene.name + "'!");

			if (!isGameplayScene && scene.name == "MainMenu") ModeControllerNew.DeactivateModes();

			OnSceneLoad?.Invoke(scene, isGameplayScene);
		}

		// ---------------PLAYER EVENTS --------------- \\

		// Runs when player dies ( listens to PlayerRespawner.PlayerDied() and PlayerRespawner.DoRespawn() )
		public static void OnPlayerDied(bool preAnim)
		{
			//PlayerPrefs.SetString("test", "The player has died! x.x");

			// If death animation finished
			if (!preAnim)
			{
				//
			}
			else
			{
				//
			}

			// Invoke events
			OnPlayerDeath?.Invoke(preAnim);

			isPlayerDead = false;
		}

		// Runs when player spawns ( listens to PlayerSpawner.DoSpawn() and PlayerRespawner.DoRespawn() )
		public static void OnPlayerSpawned(bool isRespawn)
		{
			//Scene scene = SceneManager.GetActiveScene();
			//PlayerPrefs.SetString("test", "The player has spawned in " + scene.name + "!");

			// Invoke events
			OnPlayerSpawn?.Invoke(isRespawn);
		}

		// --------------- ENEMY EVENTS --------------- \\

		// Runs when an Entity spawns ( listens to EntitySpawner.DoSpawn() )
		public static void OnEntSpawned(Entity ent)
		{
			//PlayerPrefs.SetString("test", "Entity " + ent.name + " spawned!");

			// Invoke events
			OnEntSpawn?.Invoke(ent);
		}

		// --------------- ATTACK EVENTS --------------- \\

		// Runs when damage is done ( listens to Killable.HandleHit() and Killable.ForceDeath() )
		public static void OnDamageDealt(float dmg, Entity toEnt)
		{
			bool toIttle = toEnt.gameObject.name == "PlayerEnt"; // Damage to player?
													   //PlayerPrefs.SetString("test", "Damage has been done! '" + dmg + "' DMG was done to '" + toEnt.name + "'!");

			// Invoke events
			OnDamageDone?.Invoke(dmg, toEnt);
		}

		// Runs when an enemy is killed ( listens to Killable.DoDie() and Killable.ForceDeath() )
		public static void OnEnemyKilled(Entity ent)
		{
			//PlayerPrefs.SetString("test", "'" + ent.name + "' was killed! RIP");

			// Invoke events
			OnEnemyKill?.Invoke(ent);
		}

		// --------------- GAME EVENTS --------------- \\

		// Runs when a collision is registered ( listens to BC_Collider.SendCollisionEvent() and BC_Collider.SendTriggerEvent() )
		public static void OnCollided(BC_Collider outgoingCollider, BC_Collider incomingCollider, BC_Collider.EventMode collisionType = BC_Collider.EventMode.Enter, bool isTrigger = false)
		{
			/*
			string outputStart = isTrigger ? "A trigger collision happened!\n" : "A collision happened!\n";

			switch (collisionType)
			{
				case BC_Collider.EventMode.Enter:
					PlayerPrefs.SetString("test", outputStart + outgoingCollider.name + " entered collision with " + incomingCollider.name + "!");
					break;
				case BC_Collider.EventMode.Stay:
					PlayerPrefs.SetString("test", outputStart + outgoingCollider.name + " is continuing to collide with " + incomingCollider.name + "!");
					break;
				case BC_Collider.EventMode.Exit:
					PlayerPrefs.SetString("test", outputStart + outgoingCollider.name + " exited collision with " + incomingCollider.name + "!");
					break;
			} */

			// Invoke events
			OnCollision?.Invoke(outgoingCollider, incomingCollider, collisionType, isTrigger);
		}

		// Runs when a dungeon is completed ( listens to GA_SetSaveVariable.DoExecute() )
		public static void OnDungeonCompleted(Scene dungeon)
		{
			//PlayerPrefs.SetString("test", "Dungeon '" + dungeon.name + "' was completed!");

			// Invoke events
			OnDungeonComplete?.Invoke(dungeon);
		}

		// Runs when a file is created or loaded ( listens to MainMenu.FileStartScreen.ClickedStart() and MainMenu.NewGameScreen.EnterNameDone() )
		public static void OnFileLoaded(bool isNew, string fileName = "", string filePath = "", RealDataSaver saver = null)
		{
			//string modifier = isNew ? "created" : "loaded";
			//PlayerPrefs.SetString("test", "A file was " + modifier + "!");

			// If starting new mode file, start mode
			if (isNew && !string.IsNullOrEmpty(filePath) && saver != null)
			{
				ModMaster.SetNewGameData("mod/filePath", filePath, saver);
				ModeControllerNew.ActivateModesByFileName(fileName, saver);
			}

			// If loading mode file, resume mode
			if (!isNew)
			{
				List<string> saveKeys = MainMenu.GetSortedSaveKeys(ModSaver.GetOwner(), "/local/mod/modes/active");

				for (int i = 0; i < saveKeys.Count; i++)
				{
					ModeControllerNew.ResumeMode(saveKeys[i]);
				}
			}

			// Invoke events
			OnFileLoad?.Invoke(isNew, fileName, filePath, saver);
		}

		// Runs when the game quits ( listens to PauseMenu.ClickedQuit() )
		public static void OnGameQuitted()
		{
			// Invoke events
			OnGameQuit?.Invoke();
		}

		// Runs when the game starts ( listens to LudositySplash.Awake() )
		public static void OnGameStarted()
		{
			GameObject.DontDestroyOnLoad(new GameObject("ModeController").AddComponent<ModeController>());
			GameObject.DontDestroyOnLoad(new GameObject("CommonFunctions").AddComponent<CommonFunctions>());
			//ModOptionsOld.Initialization();
			GameOptions.Instance.Initialize();

			ModSaverNew.DeletePref("test");

			OptionsSingleton<Options.GameOptions>.Instance.SubscribeToEvents();

			// Invoke events
			OnGameStart?.Invoke();
		}

		// Runs when an item is obtained ( listens to Item.OnPickup() )
		public static void OnItemGotten(Item item)
		{
			string itemName = item.name;

			// Update name based on naming convention
			if (itemName.Contains("(Clone)")) { itemName = item.name.Substring(0, item.name.IndexOf('(')); }
			if (itemName.Contains("Item_")) { itemName = itemName.Substring(5); }
			else if (itemName.Contains("Dungeon_")) { itemName = itemName.Substring(8); }

			//PlayerPrefs.SetString("test", "Got item " + itemName + "!");

			// Invoke events
			OnItemGot?.Invoke(item);
		}

		// Runs when a save flag gets set ( listens to RoomAction.StoreSave() )
		public static void OnSaveFlagSaved(string flagName)
		{
			//PlayerPrefs.SetString("test", "Save flag '" + flagName + "' has been updated!");

			// Invoke events
			OnSaveFlagSet?.Invoke(flagName);
		}
	}
}