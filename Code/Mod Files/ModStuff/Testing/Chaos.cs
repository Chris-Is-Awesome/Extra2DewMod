using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace ModStuff
{
	public class Chaos : Singleton<Chaos>
	{
		public enum EffectSeverity
		{
			Normal,
			Moderate,
			Severe,
			WTF
		}

		public enum EffectCategory
		{
			Player,
			Visual,
			Camera,
		}

		public class EffectData
		{
			public string name;
			public EffectTime time;
			public List<EffectCategory> categories;
			public EffectSeverity severity;
			public EffectConditions conditions;

			public EffectData(string effectName, EffectTime timeData, List<EffectCategory> tags, EffectSeverity howSevere, EffectConditions effectConds)
			{
				name = effectName;
				time = timeData;
				time.duration = timeData.duration;
				time.maxDuration = timeData.maxDuration;
				categories = tags;
				severity = howSevere;
				conditions = effectConds;
				conditions.dontApplyWithThese = effectConds.dontApplyWithThese;
				conditions.cancelOnLoad = effectConds.cancelOnLoad;
				conditions.resetOnLoad = effectConds.resetOnLoad;
				conditions.resetOnRoomLoad = effectConds.resetOnRoomLoad;
			}
		}

		public class EffectTime
		{
			public float duration; // Also acts as min duration if maxDuration given
			public float maxDuration;
			public float timeUntilRepeat;

			public EffectTime(float time, float maxTime = 0, float waitToRepeat = 30f)
			{
				duration = time;
				maxDuration = maxTime;
				timeUntilRepeat = waitToRepeat;
			}
		}

		public class EffectConditions
		{
			public List<EffectCategory> dontApplyWithThese;
			public bool cancelOnLoad;
			public bool resetOnLoad;
			public bool resetOnRoomLoad;

			public EffectConditions(List<EffectCategory> dontApplyWithTheseEffects = null, bool doCancelOnLoad = false, bool doResetOnLoad = false, bool doResetOnRoomLoad = false)
			{
				dontApplyWithThese = dontApplyWithTheseEffects;
				cancelOnLoad = doCancelOnLoad;
				resetOnLoad = doResetOnLoad;
				resetOnRoomLoad = doResetOnRoomLoad;
			}
		}

		public class EffectSeverityData
		{
			public EffectSeverity severity;
			public int maxEffectsAllowed;
			public int rng;

			public EffectSeverityData(EffectSeverity howSevere, int allowThisManyEffects, int rngValue)
			{
				severity = howSevere;
				maxEffectsAllowed = allowThisManyEffects;
				rng = rngValue;
			}
		}

		public class MethodArgs
		{
			public EffectTime time;
			public bool doStop;

			public MethodArgs(EffectTime timeData, bool stop = false)
			{
				time = timeData;
				doStop = stop;
			}
		}

		List<EffectData> effectPool = new List<EffectData>();
		public List<EffectData> activeEffects = new List<EffectData>();
		List<EffectData> disabledEffects = new List<EffectData>();
		Dictionary<EffectSeverity, int> severityMax = new Dictionary<EffectSeverity, int>()
		{
			{ EffectSeverity.Moderate, 4 },
			{ EffectSeverity.Severe, 2 },
			{ EffectSeverity.WTF, 1 }
		};
		ChaosEffects chaosDoer; // Bringer of DETSTRUCTION!!!
		int maxEffects = 6;
		int maxEffectsForSeverity = 6;
		bool hasPlayerSpawned = false;

		// Initialization
		public void Initialize()
		{
			FillEffectPool();
			chaosDoer = ChaosEffects.Instance;
			maxEffectsForSeverity = maxEffects;

			// Event subscriptions
			GameStateNew.OnSceneLoad += OnSceneLoad;
			GameStateNew.OnPlayerSpawn += OnPlayerSpawn;
			GameStateNew.OnRoomChange += OnRoomLoad;
		}

		// Fills the effect pool
		void FillEffectPool()
		{
			effectPool = new List<EffectData>()
			{
				new EffectData(
					"Player/Invisible",
					new EffectTime(15f, 30f, 60f),
					new List<EffectCategory> { EffectCategory.Player, EffectCategory.Visual },
					EffectSeverity.Moderate,
					new EffectConditions(new List<EffectCategory> { EffectCategory.Player }, true )),
			};
		}

		// On scene load
		void OnSceneLoad(Scene scene, bool isGameplayScene)
		{
			if (isGameplayScene && activeEffects.Count > 0)
			{
				for (int i = 0; i < activeEffects.Count; i++)
				{
					EffectData effect = activeEffects[i];

					if (effect.conditions.cancelOnLoad) { StopEffect(effect); }
				}
			}
		}

		// On player spawn
		void OnPlayerSpawn(bool isRespawn)
		{
			hasPlayerSpawned = true;

			if (activeEffects.Count > 0)
			{
				for (int i = 0; i < activeEffects.Count; i++)
				{
					EffectData effect = activeEffects[i];
					EffectConditions conds = effect.conditions;

					if (conds.resetOnLoad) { StartEffect(effect); }
				}
			}
		}

		// On room load
		void OnRoomLoad(LevelRoom room)
		{
			if (activeEffects.Count > 0)
			{
				for (int i = 0; i < activeEffects.Count; i++)
				{
					EffectData effect = activeEffects[i];

					if (effect.conditions.resetOnRoomLoad) { StartEffect(effect); }
				}
			}
		}

		// Updates effects
		void LateUpdate()
		{
			if (hasPlayerSpawned)
			{
				UpdateTimers();

				if (activeEffects.Count < maxEffectsForSeverity)
				{
					EffectData effect = GetEffect();

					if (effect != null && CanActivateEffect(effect))
					{
						StartEffect(effect);
					}
				}
			}
		}

		// Countdown each effect's timers and stop effect when timer reaches 0
		void UpdateTimers()
		{
			for (int i = 0; i < activeEffects.Count; i++)
			{
				EffectTime timer = activeEffects[i].time;

				// Get random time
				if (timer.duration < timer.maxDuration)
				{
					float randTime = Random.Range(timer.duration, timer.maxDuration);
					timer.duration = randTime;
				}

				if (timer.duration > 0) { timer.duration -= Time.deltaTime; }
				else
				{
					timer.duration = 0;
					StopEffect(activeEffects[i]);
				}
			}

			for (int i = 0; i < disabledEffects.Count; i++)
			{
				EffectTime timer = activeEffects[i].time;

				if (timer.timeUntilRepeat > 0) { timer.timeUntilRepeat -= Time.deltaTime; }
				else
				{
					timer.timeUntilRepeat = 0;
					disabledEffects.RemoveAt(i);
				}
			}
		}

		// Starts the effect
		void StartEffect(EffectData effect)
		{
			string effectName = effect.name.Replace("/", "");
			chaosDoer.SendMessage(effectName, new MethodArgs(effect.time, false));
			activeEffects.Add(effect);
		}

		// Stops the effect
		void StopEffect(EffectData effect)
		{
			// Stop effect
			string effectName = effect.name.Replace("/", "");
			chaosDoer.SendMessage(effectName, new MethodArgs(effect.time, true));
			activeEffects.Remove(effect);
			
			// Prevent repeat until x amount of time has passed
			if (effect.time.timeUntilRepeat > 0) { disabledEffects.Add(effect); }

			// Reset max effects count
			int count = 0;
			for (int i = 0; i < activeEffects.Count; i++)
			{
				if (severityMax.ContainsKey(activeEffects[i].severity)) { count++; }
			}
			if (count < 2) { maxEffectsForSeverity = maxEffects; }
		}

		// Returns a random effect or effect by name
		EffectData GetEffect(string name = "")
		{
			// Return specific effect
			if (!string.IsNullOrEmpty(name))
			{
				for (int i = 0; i < effectPool.Count; i++)
				{
					if (effectPool[i].name == name) { return effectPool[i]; }
				}

				// If name was invalid, return null
				return null;
			}

			// Return random effect
			int randIndex = Random.Range(0, effectPool.Count - 1);
			return effectPool[randIndex];
		}

		// Clears and stops all active effects
		void ClearActiveEffects()
		{
			if (activeEffects.Count > 0)
			{
				for (int i = 0; i < activeEffects.Count; i++)
				{
					StopEffect(activeEffects[i]);
				}
			}

			// Reset max effects count
			maxEffectsForSeverity = maxEffects;
		}

		// Checks logic conditions
		bool CanActivateEffect(EffectData effect)
		{
			// Check for severity maxes
			if (severityMax.TryGetValue(effect.severity, out int max))
			{
				if (activeEffects.Count >= max) { maxEffectsForSeverity = max;  return false; }
			}

			// Check for disallowed combinations
			if (effect.conditions.dontApplyWithThese.Count > 0)
			{
				List<EffectCategory> disallowedCombos = effect.conditions.dontApplyWithThese;
				for (int i = 0; i < disallowedCombos.Count; i++)
				{
					for (int j = 0; j < activeEffects.Count; j++)
					{
						if (activeEffects[i].categories.Contains(disallowedCombos[i])) { return false; }
					}
				}
			}

			bool belowMax = activeEffects.Count < maxEffects;
			bool isActive = activeEffects.Contains(effect);
			bool isDisabled = disabledEffects.Contains(effect);

			return belowMax && !isActive && !isDisabled;
		}
		// For debugging/testing purposes, clear effect list and play only the current effect for the specified amount of time
		// Call this from DebugCommands.Test()
		public string TestEffect(string effectName, float duration = 30f)
		{
			// Start effect
			EffectData effect = GetEffect(effectName);

			if (effect != null)
			{
				effect.time.duration = duration;
				effect.time.maxDuration = 0f;

				ClearActiveEffects(); // Clear all effects so we only play the one we want to test
				StartEffect(effect);
				return "Applied effect " + effectName + " for " + duration + " seconds!";
			}
			
			return "ERROR: No effect named " + effectName + " was found.\nA typo perhaps?";
		}
	}
}