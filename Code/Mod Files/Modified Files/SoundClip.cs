using System;
using UnityEngine;
using UnityEngine.Audio;

[PersistentScriptableObject]
public class SoundClip : ScriptableObject
{
	public AudioClip _rawSound;

	[SerializeField]
	public string _rawSoundName;

	[SerializeField]
	public float _volume = 1f;

	[SerializeField]
	public Range _pitch;

	[SerializeField]
	public SoundClip.Variation[] _variations;

	[SerializeField]
	public bool _loop;

	[SerializeField]
	public SoundClip.ExclusiveMode _exclusive;

	[SerializeField]
	public SoundClip.ExclusiveMode _contextExclusive = SoundClip.ExclusiveMode.Ignore;

	[SerializeField]
	public SoundClip[] _overrides;

	[SerializeField]
	public float _masterVolume = 1f;

	[SerializeField]
	public float _masterPitch = 1f;

	[SerializeField]
	public int _skipPercentage;

	[SerializeField]
	public bool _emptyIsValid;

	[SerializeField]
	public float _cooldown = 0.08f;

	[SerializeField]
	public float _maxDistance;

	[SerializeField]
	public bool _3dSound;

	[SerializeField]
	public AudioMixerGroup _mixerGroup;

	public float timestamp;

	public void OnEnable ()
	{
		this.timestamp = this._cooldown;
	}

	public void UpdateSounds (AudioClip main, AudioClip[] vars)
	{
		if (main != null)
		{
			this._rawSoundName = main.name;
		}
		int num = 0;
		while (num < this._variations.Length && num < vars.Length)
		{
			if (vars[num] != null)
			{
				this._variations[num].SetClip(vars[num]);
			}
			num++;
		}
	}

	public string RawSoundName
	{
		get
		{
			if (this._rawSound != null)
			{
				return this._rawSound.name;
			}
			return this._rawSoundName;
		}
	}

	public SoundClip.Variation[] GetVariations ()
	{
		return this._variations;
	}

	public bool Overrides (SoundClip other)
	{
		return Array.IndexOf<SoundClip>(this._overrides, other) != -1;
	}

	public bool HasOverrides
	{
		get
		{
			return this._overrides != null && this._overrides.Length > 0;
		}
	}

	public AudioClip RawSound
	{
		get
		{
			if (this._rawSound != null)
			{
				return this._rawSound;
			}
			return SoundLoader.GetClip(this._rawSoundName);
		}
	}

	public bool Loop
	{
		get
		{
			return this._loop;
		}
	}

	public SoundClip.ExclusiveMode Exclusive
	{
		get
		{
			return this._exclusive;
		}
	}

	public SoundClip.ExclusiveMode ContextExclusive
	{
		get
		{
			return this._contextExclusive;
		}
	}

	public float MaxDistance
	{
		get
		{
			return this._maxDistance;
		}
	}

	public bool Is3DSound
	{
		get
		{
			return this._3dSound;
		}
	}

	public AudioMixerGroup MixerGroup
	{
		get
		{
			return this._mixerGroup;
		}
	}

	public bool EmptyIsValid
	{
		get
		{
			return this._emptyIsValid;
		}
	}

	public bool Equals (AudioClip clip)
	{
		if (this.RawSoundName == clip.name)
		{
			return true;
		}
		for (int i = 0; i < this._variations.Length; i++)
		{
			SoundClip.Variation variation = this._variations[i];
			if (variation.ClipName == clip.name)
			{
				return true;
			}
		}
		return false;
	}

	public float GetBaseVolume (AudioClip forClip)
	{
		if (this._variations != null)
		{
			for (int i = 0; i < this._variations.Length; i++)
			{
				SoundClip.Variation variation = this._variations[i];
				if (forClip == variation.Clip)
				{
					return variation.BaseVolume * this._masterVolume;
				}
			}
		}
		return this._volume * this._masterVolume;
	}

	public AudioClip GetClip ()
	{
		if (this._cooldown > 0f)
		{
			float num = Time.realtimeSinceStartup - this.timestamp;
			if (num > 0f && num < this._cooldown)
			{
				return null;
			}
		}
		if (UnityEngine.Random.value * 100f < (float)this._skipPercentage)
		{
			return null;
		}
		this.timestamp = Time.realtimeSinceStartup;
		if (this._variations == null || this._variations.Length <= 0)
		{
			return this.RawSound;
		}
		float num2 = UnityEngine.Random.Range(0f, (float)(this._variations.Length + 1));
		if (num2 >= (float)this._variations.Length)
		{
			return this.RawSound;
		}
		return this._variations[(int)num2].Clip;
	}

	public AudioClip GetClipCertain ()
	{
		if (this._cooldown > 0f)
		{
			float num = Time.realtimeSinceStartup - this.timestamp;
			if (num > 0f && num < this._cooldown)
			{
				return null;
			}
		}
		this.timestamp = Time.realtimeSinceStartup;
		if (this._variations == null || this._variations.Length <= 0)
		{
			return this.RawSound;
		}
		float num2 = UnityEngine.Random.Range(0f, (float)(this._variations.Length + 1));
		if (num2 >= (float)this._variations.Length)
		{
			return this.RawSound;
		}
		return this._variations[(int)num2].Clip;
	}

	public float GetPitch (AudioClip forClip)
	{
		if (this._variations != null)
		{
			for (int i = 0; i < this._variations.Length; i++)
			{
				SoundClip.Variation variation = this._variations[i];
				if (forClip == variation.Clip)
				{
					return variation.Pitch * this._masterPitch;
				}
			}
		}
		float randomValue = this._pitch.randomValue;
		return randomValue * this._masterPitch;
	}

	public enum ExclusiveMode
	{
		Normal,
		Exclusive,
		Override,
		Ignore
	}

	[Serializable]
	public class Variation
	{
		[SerializeField]
		public AudioClip _rawSound;

		[SerializeField]
		public string _rawSoundName;

		[SerializeField]
		public float _baseVolume = 1f;

		[SerializeField]
		public Range _pitch;

		public AudioClip Clip
		{
			get
			{
				if (this._rawSound != null)
				{
					return this._rawSound;
				}
				return SoundLoader.GetClip(this._rawSoundName);
			}
		}

		public string ClipName
		{
			get
			{
				if (this._rawSound != null)
				{
					return this._rawSound.name;
				}
				return this._rawSoundName;
			}
		}

		public float BaseVolume
		{
			get
			{
				return this._baseVolume;
			}
		}

		public float Pitch
		{
			get
			{
				return this._pitch.randomValue;
			}
		}

		public void SetClip (AudioClip clip)
		{
			this._rawSoundName = clip.name;
		}
	}
}