using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff
{
	public class DoorRandomizerGM : E2DGameModeSingleton<DoorRandomizerGM>
	{

        void Awake()
        {
            Mode = ModeControllerNew.ModeType.DoorRandomizer;
            Title = "Door Randomizer";
            QuickDescription = "EXPERT PLAYERS ONLY\nRandomize most scene transitions (dungeons, caves, overworld connections).";
            Description = "Randomizes all scene transitions while still making the game beatable. Randomization affects caves, dungeons, overworld connections and some secret places. Hidden entrances to caves will be randomized, such as the grass patch in fancy ruins and jenny berry's home.\n\nConnections left out of the randomization: The grand library, all mechanic dungeons, the dream world, the secret remedy and any one way connection. A cheat sheet can be found in the 'extra2dew\\randomizer' folder. Don't use this mode with item randomizer when it starts stick-less or roll-less";
            FileNames = new List<string> { /*"doorrandomizer", "crazydoors", "doorr"*/ };
            Restrictions = new List<ModeControllerNew.ModeType> { ModeControllerNew.ModeType.DungeonRush, ModeControllerNew.ModeType.BossRush };
        }

        bool fileCreated;

        override public void SetupSaveFile(RealDataSaver saver)
        {
            if (saver == null) return;

            ModMaster.SetNewGameData("mod/randomizer_seed", RNGSeed.CurrentSeed, saver);
            fileCreated = true;
            Activate();
            DoorsRandomizer.Instance.PrintSheetTxt();
        }

        override public void Activate()
		{
            string seed = fileCreated ? RNGSeed.CurrentSeed : ModSaver.LoadStringFromFile("mod", "randomizer_seed");
            DoorsRandomizer.Instance.RandomizeWithSeed(seed);
            fileCreated = false;
            IsActive = true;
        }

        override public void Deactivate()
		{
            DoorsRandomizer.Instance.ResetRooms();
            fileCreated = false;
            IsActive = false;
        }

        UITextFrame seed;
        public override void CreateOptions()
        {
            GameObject output = new GameObject();

            //Seed
            seed = UIFactory.Instance.CreateTextFrame(0f, 0.50f, output.transform);
            seed.UIName = RNGSeed.CurrentSeed;

            //Reroll seed
            UIButton rerollSeed = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, -1.5f, -0.5f, output.transform, "New seed");
            rerollSeed.ScaleBackground(new Vector2(0.8f, 1f), Vector2.one * 0.7f);
            rerollSeed.onInteraction += delegate ()
            {
                RNGSeed.ReRollSeed();
                seed.UIName = RNGSeed.CurrentSeed;
            };

            //Paste seed
            UIButton pasteSeed = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 1.5f, -0.5f, output.transform, "Paste seed");
            pasteSeed.ScaleBackground(new Vector2(0.8f, 1f), Vector2.one * 0.7f);
            pasteSeed.onInteraction += delegate ()
            {
                RNGSeed.SetSeed(GUIUtility.systemCopyBuffer);
                seed.UIName = RNGSeed.CurrentSeed;
            };

            MenuGo = output;
        }

        public override void OnOpenMenu()
        {
            seed.UIName = RNGSeed.CurrentSeed;
        }
    }
}