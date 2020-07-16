using UnityEngine;
using System.Collections;

namespace ModStuff
{
	public class HealthRegen : UpdateBehaviour, IUpdatable
	{
		Killable hp = null;
		float startTimer = 0;
		float hpToHealTo = 0;
		float repeatTimer = 0;
		float repeater = 0;

		// Setup
		public void DoHealthRegen(Killable hpData, float startTime, float hpToHeal, float repeatTime)
		{
			hp = hpData;
			startTimer = startTime;
			hpToHealTo = hpToHeal;
			repeatTimer = repeatTime;
		}

		// Stop health regen
		public void StopHealthRegen()
		{
			hp = null;
		}

		// Do health regen
		void IUpdatable.UpdateObject()
		{
			if (hp != null)
			{
				// Delay health regen
				if (startTimer > 0) { startTimer -= Time.deltaTime; }
				// Do health regen
				else
				{
					if (repeater > 0) { repeater -= Time.deltaTime; }
					else
					{
						if (hp.CurrentHp < hp.MaxHp)
						{
							hp.CurrentHp = Mathf.Clamp(hp.CurrentHp + hpToHealTo, 1, hp.MaxHp);
							repeater = repeatTimer;
						}
					}
				}
			}
		}
	}
}