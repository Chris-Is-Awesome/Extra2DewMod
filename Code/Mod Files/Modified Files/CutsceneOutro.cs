using System;
using UnityEngine;
using ModStuff;

public class CutsceneOutro : CutsceneBase
{
	public Camera cam;

	private int loop3;

	private int loop4;

	private int loop6;

	private int temp;

	private int quick4;

	private int faster4;

	public AudioSource seaSound;

	public AudioSource raftSound;

	public AudioSource forestSound;

	public AudioSource tippsieWingFlapSound;

	public AudioSource mullerSound;

	public CutsceneOutro ()
	{
	}

	// Update stats
	void Start()
	{
		ModMaster.UpdateStats("GameBeats");
		ModSaver.SaveIntToFile("mod", "hasBeatenGame", 1);
	}

	protected override void doFrame (int f)
	{
		if (f == 0)
		{
			this.cam.transform.position = new Vector3(0f, 0f, -18f);
		}
		if (f % 3 == 0)
		{
			this.quick4++;
			if (this.quick4 > 3)
			{
				this.quick4 = 0;
			}
			if (f < 2000)
			{
				base.setFrame("Fishbun0", this.quick4);
			}
		}
		if (f % 5 == 0)
		{
			this.faster4++;
			if (this.faster4 > 3)
			{
				this.faster4 = 0;
			}
			base.setFrame("Mapman3", this.faster4);
		}
		if (f % 8 == 0)
		{
			this.loop3++;
			if (this.loop3 > 2)
			{
				this.loop3 = 0;
			}
			this.loop4++;
			if (this.loop4 > 3)
			{
				this.loop4 = 0;
			}
			this.loop6++;
			if (this.loop6 > 5)
			{
				this.loop6 = 0;
			}
			base.setFrame("Ittle0", this.loop3);
			base.setFrame("Tippsie0", this.loop4);
			base.setFrame("Raft1", this.loop3);
			base.setFrame("Raft3", this.loop3);
			base.setFrame("Lenny3", this.loop3);
			base.setFrame("Biadlo3", this.loop3);
			base.setFrame("Cyber3", this.loop3);
			base.setFrame("Fishbun3", this.loop4);
			base.setFrame("Safety3", this.loop3);
			base.setFrame("Passel3", this.loop3);
			base.setFrame("Simulacrum3", this.loop3);
		}
		if (f == 0)
		{
			base.playSound("Forest", 0.85f, false);
			base.playSound("WingFlaps", 0.1f, false);
		}
		if (f == 5)
		{
			base.playSound("Crash", 0.5f, false);
		}
		if (f == 24)
		{
			base.playSound("Fishbun", 0.7f, false);
		}
		if (f == 240)
		{
			base.stopSound("Forest");
			base.stopSound("WingFlaps");
			base.playSound("Sea", 0.85f, false);
			base.playSound("Raft", 0.85f, false);
		}
		if (f == 530)
		{
			base.playSound("Charge", 0.6f, false);
		}
		if (f == 570)
		{
			base.playSound("Explosion", 0.6f, false);
			base.playSound("Muller", 0.6f, false);
		}
		if (f > 800 && f < 1050)
		{
			this.mullerSound.volume = Mathf.Lerp(0.6f, 0f, (float)(f - 800) / 250f);
		}
		if (f == 1051)
		{
			base.stopSound("Muller");
		}
		if (f == 1050)
		{
			base.playSound("Song", 1f, true);
		}
		if (f == 8240)
		{
			base.playSound("Fin", 1f, true);
		}
	}
}