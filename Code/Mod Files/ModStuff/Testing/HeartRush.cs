using UnityEngine;
using System.Collections;
using System.IO;

namespace ModStuff
{
	public class HeartRush : Singleton<HeartRush>, IModeFuncs
	{
		Killable playerHp;

		// HP: 1 heart = 4
		public float startingHp = 120f;
		float hpToAddFromCrayon = 16f;
		public bool isDead = false;
		int difficulty = -1;

		public void Initialize(bool activate, RealDataSaver saver = null, int _difficulty = -1)
		{
			if (activate && saver != null)
			{
				string hp = startingHp.ToString();

				// Save data to file
				ModSaverNew.SaveToNewSaveFile("start/level", "FluffyFields", saver);
				ModSaverNew.SaveToNewSaveFile("start/door", "", saver);
				ModSaverNew.SaveToNewSaveFile("player/maxHp", hp, saver);
				ModSaverNew.SaveToNewSaveFile("player/hp", hp, saver);
				ModSaverNew.SaveToNewSaveFile("player/vars/easyMode", "0", saver);
				ModSaverNew.SaveToNewSaveFile("settings/easyMode", "0", saver);
				ModSaverNew.SaveToNewSaveFile("settings/hideCutscenes", "1", saver);
				ModSaverNew.SaveToNewSaveFile("settings/showTIme", "0", saver);
				ModSaverNew.SaveToNewSaveFile("settings/hideMapHints", "0", saver);

				difficulty = _difficulty;

				Activate();
			}
		}

		public void Activate()
		{
			// Set difficulty
			if (difficulty < 0)
			{
				string className = GetType().ToString().Split('.')[1];
				string value = ModSaverNew.LoadFromSaveFile("mod/modes/active/" + className + "/difficulty");

				if (!string.IsNullOrEmpty(value)) difficulty = int.Parse(value);
			}

			// Subscribe to events
			GameStateNew.OnDamageDone += OnDamageTaken;
			GameStateNew.OnItemGot += OnItemGet;
			GameStateNew.OnPlayerDeath += OnPlayerDeath;
		}

		public void Deactivate()
		{
			// Unsubscribe from events
			GameStateNew.OnDamageDone -= OnDamageTaken;
			GameStateNew.OnItemGot -= OnItemGet;
			GameStateNew.OnPlayerDeath -= OnPlayerDeath;
		}

		void OnDamageTaken(float dmg, Entity toEnt)
		{
			if (playerHp == null) { playerHp = ModMaster.GetEntComp<Killable>("PlayerEnt"); }

			// When Ittle takes damage, reduce max HP until 0
			if (toEnt.name == "PlayerEnt")
			{
				if (playerHp.MaxHp - dmg <= 0) { playerHp.CurrentHp = 0; }
				else { playerHp.MaxHp -= dmg; }
			}
		}

		void OnItemGet(Item item)
		{
			// When you get crayon, add some hearts
			if (item.name.Contains("PieceOfPaper"))
			{
				if (playerHp == null) { playerHp = ModMaster.GetEntComp<Killable>("PlayerEnt"); }

				playerHp.MaxHp += hpToAddFromCrayon;
				playerHp.CurrentHp = playerHp.MaxHp;
			}
		}

		void OnPlayerDeath(bool preAnim)
		{
			// Game over. Return to title screen and delete file
			if (preAnim)
			{
				isDead = true;
				ModMaster.LoadScene("MainMenu", "");
				DeleteFile();
			}
		}

		void DeleteFile()
		{
			// Deletes Heart Rush file
			DataIOBase io = DataFileIO.GetCurrentIO();
			string filePath = ModSaver.LoadStringFromFile("mod", "filePath");
			string thumbPath = filePath.Remove(filePath.Length - 3, 3) + "png";
			
			if (File.Exists(io.FinalizePath(filePath))) io.DeleteFile(filePath);
			if (File.Exists(io.FinalizePath(thumbPath))) { io.DeleteFile(thumbPath); }

			ModSaver.GetOwner().ResetLocalSaver();
		}
	}
}