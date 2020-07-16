using UnityEngine;
using System.Collections.Generic;

namespace ModStuff
{
	public class EntityActionManager : Singleton<EntityActionManager>
	{
		public string SetStateForEnt(GameObject ent, string stateName)
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
				for (int i = 0; i < states.Count; i++)
				{
					// If found wanted state
					if (states[i].Name.ToLower() == stateName)
					{
						controller.SwitchToState(states[i], toEnt, null); // Change state
						return "Changed state for " + ent.name + " to " + stateName + "!";
					}
				}
			}
			else { return "ERROR: No states were found for " + ent.name; }

			return "ERROR: " + stateName + " is not a valid state for " + ent.name;
		}

		// TODO: Animate using Entity.PlayAnimation(string animName, int variation)
	}
}