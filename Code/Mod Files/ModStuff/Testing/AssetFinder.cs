using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModStuff
{
	public class AssetFinder : Singleton<AssetFinder>
	{
		public List<Component> comps = new List<Component>();
		public List<ScriptableObject> sos = new List<ScriptableObject>();

		// Fills component pool
		public void GetComps<T>(float interval) where T: Component
		{
			StartCoroutine(LoadCompsOfType<T>(interval));
		}

		// Fills component pool
		public List<T> GetCompsTest<T>(float interval) where T: Component
		{
			StartCoroutine(LoadCompsOfType<T>(interval));
			return comps as List<T>;
		}

		// Fills Scriptable Object pool
		public void GetSOs<T>(float interval) where T: ScriptableObject
		{
			StartCoroutine(LoadSOsOfType<T>(interval));
		}

		// Loads components of type T
		IEnumerator LoadCompsOfType<T>(float interval) where T: Component
		{
			foreach (Component comp in Resources.FindObjectsOfTypeAll<Component>())
			{
				if (comp is T) { comps.Add(comp); }
				yield return interval;
			}
		}

		// Loads Scriptable Objects of type T
		IEnumerator LoadSOsOfType<T>(float interval) where T: ScriptableObject
		{
			foreach (ScriptableObject so in Resources.FindObjectsOfTypeAll<ScriptableObject>())
			{
				if (so is T && !sos.Contains(so)) { sos.Add(so); }
				yield return interval;
			}
		}
	}
}