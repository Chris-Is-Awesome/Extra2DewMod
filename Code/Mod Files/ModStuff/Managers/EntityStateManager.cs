using UnityEngine;
using System.Collections.Generic;

namespace ModStuff
{
	public class EntityStateManager : Singleton<EntityStateManager>
	{
		public string SetStateForEnt(GameObject ent, string stateName, int indexIfMultiple = 0)
		{
			Entity toEnt;
			AIController controller;
			List<AIState> states;

			if (ent.GetComponent<Entity>() != null) { toEnt = ent.GetComponent<Entity>(); }
			else { return "ERROR: Entity component for " + ent.name + " was not found. This should not happen!"; }
			if (GameObject.Find(ent.name + "Controller(Clone)") != null) { controller = GameObject.Find(ent.name + "Controller(Clone)").GetComponent<AIController>(); }
			else { return "ERROR: " + ent.name + "'s controller obj was not found. An exception perhaps?"; }

			states = controller.states;

			if (states.Count > 0)
			{
				List<AIState> statesWithSameName = new List<AIState>();

				for (int i = 0; i < states.Count; i++)
				{
					// If found wanted state
					if (states[i].Name.ToLower() == stateName)
					{
						statesWithSameName.Add(states[i]);
					}
				}

				if (statesWithSameName.Count > 0)
				{
					controller.SwitchToState(statesWithSameName[indexIfMultiple], toEnt, null); // Change state
				}

				controller.SwitchToState(statesWithSameName[0], toEnt, null); // Change state
				return "Changed state for " + ent.name + " to " + stateName + "!";
			}
			else { return "ERROR: No states were found for " + ent.name; }
		}
	}
}