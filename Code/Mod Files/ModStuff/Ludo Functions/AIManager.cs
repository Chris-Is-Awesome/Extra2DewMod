using System.Collections.Generic;
using UnityEngine;

namespace ModStuff.LudoFunctions
{
	public class AIManager : Singleton<AIManager>
	{
		public void SetState(GameObject go, string stateName)
		{
			// Get Entity
			Entity ent = ModMaster.GetEntComp<Entity>(go.name);

			// Get AI Controller
			AIController controller = ModMaster.GetEntComp<AIController>(go.name + "Controller(Clone)");
			PlayerPrefs.SetString("test", "Starting to set state for " + go.name + "...");

			// Get AI states
			if (controller != null)
			{
				List<AIState> states = controller.states;
				PlayerPrefs.SetString("test", "Found " + states.Count + " states on " + go.name);

				for (int i = 0; i < states.Count; i++)
				{
					if (states[i].Name.ToLower() == stateName.ToLower())
					{
						controller.SwitchToState(states[i], ent, null);
						PlayerPrefs.SetString("test", "Set state " + states[i].Name + " for Entity " + ent.name);
						return;
					}
				}

				PlayerPrefs.SetString("test", "Did not find state " + stateName + " on Entity " + ent.name);
			}
			else
			{
				PlayerPrefs.SetString("test", "No AIController found for " + go.name);
			}
		}
	}
}