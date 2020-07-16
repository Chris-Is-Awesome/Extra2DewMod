using UnityEngine;
using System.Collections.Generic;

namespace ModStuff
{
	public class ChaosEffects : Singleton<ChaosEffects>
	{
		public void PlayerInvisible(Chaos.MethodArgs args)
		{
			if (!args.doStop)
			{
				PlayerPrefs.SetString("test", "Don't stop!");
				return;
			}
		}
	}
}