using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace ModStuff
{
	public class ExpertMode : Singleton<ExpertMode>
	{
		public enum Difficulty
		{
			Normal,
			Hard,
			VeryHard,
			ReallyJoelsDad
		}

		public Difficulty difficulty;

		public void StartOfGame(RealDataSaver saver)
		{
			// Set settings
			ModMaster.SetNewGameData("mod/isexpert", "1", saver);
			ModMaster.SetNewGameData("settings/hideCutscenes", "1", saver);
			ModMaster.SetNewGameData("settings/hideMapHint", "1", saver);

			// Event subscriptions
			GameStateNew.OnSceneLoad += OnSceneLoad;
			GameStateNew.OnItemGot += OnItemGet;
			GameStateNew.OnEnemyKill += OnEnemyKilled;
			GameStateNew.OnPlayerDeath += OnPlayerDeath;
			GameStateNew.OnRoomChange += OnRoomLoad;
			GameStateNew.OnDamageDone += OnDamageDone;

			switch (difficulty)
			{
				case Difficulty.Hard:

					// Player HP to 20 (5 hearts)
					// 3 lives

					break;
				case Difficulty.VeryHard:

					// Player HP to 20 (5 hearts)
					// 1 life

					break;
				case Difficulty.ReallyJoelsDad:

					// Player HP to 1 (1/4 heart)
					// 1 life

					break;
				default:

					// Player HP to 20 (5 hearts)
					// 3 lives

					break;
			}
		}

		void OnSceneLoad(Scene scene, bool isGameplayScene)
		{
			if (isGameplayScene)
			{
				// Disable drops from breakable objects, the S4 ice cream, and yellow hearts from chests
				// Add text UI near health bar indicating how many lives you have remaining (maybe add heart icon too!)

				// Make red screen transitions
				foreach (SceneDoor door in Resources.FindObjectsOfTypeAll<SceneDoor>())
				{
					FadeEffectData fadeData = ModMaster.MakeFadeEffect(new Color32(100, 10, 10, 1), "circle");
					door._fadeData = fadeData;
					door._fadeInData = fadeData;
				}

				switch (difficulty)
				{
					case Difficulty.Hard:

						// No overworld warps
						// Max HP of enemies x2

						break;
					case Difficulty.VeryHard:

						// Volcanic eruption in overworld only
						// Max HP of enemies x2.5
						// x2 enemies (overworld)
						// No overworld warps
						// No checkpoints

						break;
					case Difficulty.ReallyJoelsDad:

						// Volcanic eruption in overworld AND dungeons
						// x2 enemies (including bosses)
						// Max HP of enemies x5
						// No checkpoints
						// No overworld warps

						break;
					default:

						// Normal stuff

						break;
				}
			}
		}

		void OnItemGet(Item item)
		{
			switch (difficulty)
			{
				case Difficulty.Hard:

					// Crayons give +1 life

					break;
				case Difficulty.VeryHard:

					// Crayons double CURRENT HP (adds more hearts)!

					break;
				case Difficulty.ReallyJoelsDad:

					// Crayons, Tomes, Amulets, and Headbands all do nothing

					break;
				default:

					// Crayons give +1 life

					break;
			}
		}

		void OnEnemyKilled(Entity ent)
		{
			// Disable drops
			foreach (Item item in Resources.FindObjectsOfTypeAll<Item>())
			{
				if (item.ItemId == null) { Destroy(item); }
			}

			switch (difficulty)
			{
				case Difficulty.Hard:

					// Hard stuff

					break;
				case Difficulty.VeryHard:

					// Very hard stuff

					break;
				case Difficulty.ReallyJoelsDad:

					// Impossible stuff

					break;
				default:

					// Normal stuff

					break;
			}
		}

		void OnPlayerDeath(bool preAnim)
		{
			// -1 life, or if last life, game over (same as Heart Rush)

			switch (difficulty)
			{
				case Difficulty.Hard:

					// Hard stuff

					break;
				case Difficulty.VeryHard:

					// Respawn at Fluffy lake

					break;
				case Difficulty.ReallyJoelsDad:

					// Impossible stuff

					break;
				default:

					// Normal stuff

					break;
			}
		}

		void OnDamageDone(float dmg, Entity toEnt)
		{
			// No healing from checkpoints

			// Stop health regen
			if (toEnt.GetComponent<HealthRegen>() != null) { toEnt.GetComponent<HealthRegen>().StopHealthRegen(); }

			switch (difficulty)
			{
				case Difficulty.Hard:

					// Enemy health regen (slow)

					break;
				case Difficulty.VeryHard:

					// Enemy health regen (faster)
					// Random negative debuff

					break;
				case Difficulty.ReallyJoelsDad:

					// Enemy health regen (fastest)

					break;
				default:

					// Enemy health regen (slowest)
					if (toEnt.name != "PlayerEnt" && toEnt.transform.Find("Killable").GetComponent<Killable>().CurrentHp - dmg > 0)
					{
						HealthRegen regen;
						if (toEnt.GetComponent<HealthRegen>() == null) { regen = toEnt.gameObject.AddComponent<HealthRegen>(); }
						else { regen = toEnt.gameObject.GetComponent<HealthRegen>(); }

						float delayStart = 3f; float hpToHeal = 10f; float repeatInterval = 1f;
						Killable hpData = toEnt.transform.Find("Killable").GetComponent<Killable>();
						regen.DoHealthRegen(hpData, delayStart, hpToHeal, repeatInterval);
					}

					break;
			}
		}

		void OnRoomLoad(LevelRoom room)
		{
			switch (difficulty)
			{
				case Difficulty.Hard:

					// Hard stuff

					break;
				case Difficulty.VeryHard:

					// Very hard stuff

					break;
				case Difficulty.ReallyJoelsDad:

					// Impossible stuff

					break;
				default:

					// Normal stuff

					break;
			}
		}
	}
}