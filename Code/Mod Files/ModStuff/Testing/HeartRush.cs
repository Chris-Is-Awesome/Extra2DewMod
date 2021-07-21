using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace ModStuff
{
    //Add somewhere later
    /*
        // Setup BossRush & DungeonRush
    if (mc.isBossRush || mc.isDungeonRush)
    {
	    ModMaster.SetNewGameData("settings/hideCutscenes", "1", realDataSaver);
	    ModMaster.SetNewGameData("settings/showTime", "1", realDataSaver);
    }
    
    */

    public class HeartRush : E2DGameModeSingleton<HeartRush>
	{
        void Awake()
        {
            Mode = ModeControllerNew.ModeType.HeartRush;
            Title = "Heart Rush";
            QuickDescription = "Damage reduces your maximum HP. The save file is deleted after getting defeated.";
            Description = "You start with a lot of HP, but every hit you take reduces your maximum HP. Getting a crayon adds some to your max HP. Run out of HP and it's game over!";
            FileNames = new List<string> { "hearts", "heartrush", "heart rush" };
            Restrictions = new List<ModeControllerNew.ModeType>();
        }

		Killable playerHp;

		// HP: 1 heart = 4
		public float startingHp = 120f;
		float hpToAddFromCrayon = 16f;
		public bool isDead = false;

        int[] initialHPArray = new int[] { 6 * 120, 3 * 120, 2 * 120, 120, 60, 30, 1 };
        int initialHPDifficulty;
        public int InitialHP { get { return initialHPArray[initialHPDifficulty]; } }

        override public void SetupSaveFile(RealDataSaver saver)
		{
            if (saver == null) return;

			// Save data to file
			ModSaverNew.SaveToNewSaveFile("player/vars/easyMode", "0", saver);
			ModSaverNew.SaveToNewSaveFile("settings/easyMode", "0", saver);
			ModSaverNew.SaveToNewSaveFile("settings/hideCutscenes", "1", saver);
			ModSaverNew.SaveToNewSaveFile("settings/showTIme", "0", saver);
			ModSaverNew.SaveToNewSaveFile("settings/hideMapHints", "0", saver);

            ModMaster.SetNewGameData("player/maxHp", InitialHP.ToString(), saver);
            ModMaster.SetNewGameData("player/hp", InitialHP.ToString(), saver);

            //If the most difficult option was selected, switch to frog mode
            if (initialHPDifficulty == initialHPArray.Length - 1)
            {
                ModMaster.SetNewGameData("player/vars/danger", "1", saver);
                ModMaster.SetNewGameData("player/vars/outfit", "8", saver);
            }

            Activate();
		}

		override public void Activate()
		{
			// Subscribe to events
			GameStateNew.OnDamageDone += OnDamageTaken;
			GameStateNew.OnItemGot += OnItemGet;
			GameStateNew.OnPlayerDeath += OnPlayerDeath;
            IsActive = true;
        }

        override public void Deactivate()
		{
			// Unsubscribe from events
			GameStateNew.OnDamageDone -= OnDamageTaken;
			GameStateNew.OnItemGot -= OnItemGet;
			GameStateNew.OnPlayerDeath -= OnPlayerDeath;
            IsActive = false;
        }

        public override void CreateOptions()
        {
            GameObject output = new GameObject();

            //Heart rush difficulty
            UIListExplorer hsDifficulty = UIFactory.Instance.CreateListExplorer(0, 0f, output.transform, "Difficulty");
            hsDifficulty.ScaleTitleBackground(1.6f);
            hsDifficulty.ExplorerArray = new string[] { "I cannot see", "Very easy", "Easy", "Default", "Hard", "Very Hard", "ReallyJoel's Dad" };
            hsDifficulty.IndexValue = 3;
            initialHPDifficulty = 3;
            hsDifficulty.onInteraction += delegate (bool rightarrow, string text, int index)
            {
                initialHPDifficulty = index;
            };

            MenuGo = output;
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
 