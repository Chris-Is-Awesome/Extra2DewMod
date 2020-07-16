using System;
using System.Collections.Generic;
using Sound;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
	private static SoundPlayer instance;

	[SerializeField]
	private int _maxSources = 128;

	[SerializeField]
	private float _startSoundVolume = 1f;

	[SerializeField]
	private float _defMaxDistance = 9f;

	private LinkedList<AudioSource> allSources = new LinkedList<AudioSource>();

	private List<TransformSound> allTransformSounds = new List<TransformSound>();

	private List<ISound> createdSounds = new List<ISound>();

	private List<ISound> swapSounds = new List<ISound>();

	private Dictionary<object, SoundPlayer.Context> sndContexts = new Dictionary<object, SoundPlayer.Context>();

	private SoundPlayerData.State savedPlayerData;

	private AudioListener savedListener;

	public static SoundPlayer Instance
	{
		get
		{
			if (SoundPlayer.instance == null)
			{
				SoundPlayer.instance = SystemRoot.FindOrMakeSystemObject<SoundPlayer>("SoundPlayer", false);
			}
			return SoundPlayer.instance;
		}
	}

	public static SoundPlayer WeakInstance
	{
		get
		{
			return SoundPlayer.instance;
		}
	}

	private SoundPlayerData.State PlayerData
	{
		get
		{
			if (this.savedPlayerData == null || this.savedPlayerData.Data == null)
			{
				SoundPlayerData soundPlayerData = Resources.Load<SoundPlayerData>("SoundPlayerData");
				if (soundPlayerData != null)
				{
					this.savedPlayerData = soundPlayerData.MakeState();
				}
			}
			return this.savedPlayerData;
		}
	}

	public AudioListener Listener
	{
		get
		{
			if (this.savedListener == null)
			{
				this.savedListener = UnityEngine.Object.FindObjectOfType<AudioListener>();
			}
			return this.savedListener;
		}
	}

	public AudioSource GetExclusiveSource (SoundClip forClip)
	{
		foreach (AudioSource audioSource in this.allSources)
		{
			if (audioSource.isPlaying && forClip.Equals(audioSource.clip))
			{
				if (forClip.Exclusive == SoundClip.ExclusiveMode.Override)
				{
					return this.GetSource(audioSource);
				}
				return null;
			}
		}
		return this.GetFreeSource();
	}

	private AudioSource GetSource (AudioSource s)
	{
		this.swapSounds.Clear();
		for (int i = this.createdSounds.Count - 1; i >= 0; i--)
		{
			ISound sound = this.createdSounds[i];
			if (!sound.FreeSource(s, false))
			{
				this.swapSounds.Add(sound);
			}
		}
		Utility.Swap<List<ISound>>(ref this.swapSounds, ref this.createdSounds);
		s.gameObject.SetActive(true);
		AudioListener listener = this.Listener;
		s.transform.position = ((!(listener != null)) ? base.transform.position : this.Listener.transform.position);
		return s;
	}

	public AudioSource GetFreeSource ()
	{
		foreach (AudioSource audioSource in this.allSources)
		{
			if (!audioSource.isPlaying)
			{
				return this.GetSource(audioSource);
			}
		}
		if (this.allSources.Count < this._maxSources)
		{
			AudioSource audioSource2 = new GameObject("AudioSource " + (this.allSources.Count + 1))
			{
				transform =
				{
					parent = base.transform
				}
			}.AddComponent<AudioSource>();
			audioSource2.playOnAwake = false;
			audioSource2.rolloffMode = AudioRolloffMode.Linear;
			audioSource2.minDistance = 5f;
			audioSource2.maxDistance = 9f;
			audioSource2.dopplerLevel = 0f;
			audioSource2.spread = 180f;
			this.allSources.AddLast(audioSource2);
			return audioSource2;
		}
		LinkedListNode<AudioSource> first = this.allSources.First;
		this.allSources.RemoveFirst();
		this.allSources.AddLast(first);
		return first.Value;
	}

	public TransformSound GetFreeTransformSound ()
	{
		for (int i = this.allTransformSounds.Count - 1; i >= 0; i--)
		{
			TransformSound transformSound = this.allTransformSounds[i];
			if (transformSound.IsFree)
			{
				return transformSound;
			}
		}
		TransformSound transformSound2 = new GameObject("TransformSound " + (this.allTransformSounds.Count + 1))
		{
			transform =
			{
				parent = base.transform
			}
		}.AddComponent<TransformSound>();
		this.allTransformSounds.Add(transformSound2);
		return transformSound2;
	}

	public void ReleaseSource (AudioSource source)
	{
		source.Stop();
		source.gameObject.SetActive(false);
	}

	public SoundPlayer.Context GetContext (object ctx)
	{
		if (ctx == null)
		{
			return null;
		}
		SoundPlayer.Context context;
		if (!this.sndContexts.TryGetValue(ctx, out context))
		{
			context = new SoundPlayer.Context();
			this.sndContexts.Add(ctx, context);
		}
		return context;
	}

	public ISound CreateSound (SoundClip clip, bool play = true, object ctx = null)
	{
		BasicSound basicSound = new BasicSound(this, clip, ctx);
		if (play)
		{
			basicSound.Play();
		}
		this.createdSounds.Add(basicSound);
		return basicSound;
	}

	public ISound PlayPositionedSound (SoundClip clip, Vector3 pos, object ctx = null)
	{
		BasicSound basicSound = new BasicSound(this, clip, ctx);
		basicSound.SetPosition(pos);
		basicSound.Play();
		this.createdSounds.Add(basicSound);
		return basicSound;
	}

	public ISound PlayPositionedSoundMod (SoundClip clip, Vector3 pos, float pitch, object ctx = null)
	{
		BasicSound basicSound = new BasicSound(this, clip, ctx);
		basicSound.SetPosition(pos);
		basicSound.Pitch = pitch;
		basicSound.PlayMod();
		this.createdSounds.Add(basicSound);
		return basicSound;
	}

	public ISound CreateTransformSound (SoundClip clip, Transform follow, bool play = true, object ctx = null)
	{
		TransformingSound transformingSound = new TransformingSound(this, clip, follow, ctx);
		if (play)
		{
			transformingSound.Play();
		}
		this.createdSounds.Add(transformingSound);
		return transformingSound;
	}

	public float DefaultMaxDistance
	{
		get
		{
			return this._defMaxDistance;
		}
	}

	public float SoundVolume
	{
		get
		{
			return AudioListener.volume;
		}
		set
		{
			AudioListener.volume = value;
		}
	}

	public float GetChannelVolume (string ch)
	{
		return this.PlayerData.GetChannelVolume(ch);
	}

	public void SetChannelVolume (string ch, float value)
	{
		this.PlayerData.SetChannelVolume(ch, value);
	}

	public string[] GetAllChannelNames ()
	{
		return this.PlayerData.Data.GetChannelList();
	}

	public void StopAllSounds ()
	{
		for (int i = this.createdSounds.Count - 1; i >= 0; i--)
		{
			ISound sound = this.createdSounds[i];
			sound.FreeSource(null, true);
		}
		this.createdSounds.Clear();
		foreach (AudioSource audioSource in this.allSources)
		{
			audioSource.Stop();
			audioSource.gameObject.SetActive(false);
		}
		SoundLoader.ClearAll();
	}

	public void StopContext (object ctx)
	{
		SoundPlayer.Context context;
		if (this.sndContexts.TryGetValue(ctx, out context))
		{
			context.StopAll();
			this.sndContexts.Remove(ctx);
		}
	}

	private void SoundMuteChanged (bool mute)
	{
		if (mute)
		{
			this.SoundVolume = 0f;
		}
		else
		{
			this.SoundVolume = this._startSoundVolume;
		}
	}

	private void Awake ()
	{
		this.SoundVolume = this._startSoundVolume;
		this.PlayerData.ResetChannels();
	}

	public void Load (IDataSaver saver)
	{
		this.SoundVolume = saver.LoadFloat("soundVol");
		SoundPlayerData.State playerData = this.PlayerData;
		playerData.ResetChannels();
		if (saver.HasLocalSaver("channels"))
		{
			IDataSaver localSaver = saver.GetLocalSaver("channels");
			string[] allDataKeys = localSaver.GetAllDataKeys();
			for (int i = 0; i < allDataKeys.Length; i++)
			{
				playerData.SetChannelVolume(allDataKeys[i], localSaver.LoadFloat(allDataKeys[i]));
			}
		}
	}

	public void Save (IDataSaver saver)
	{
		saver.SaveFloat("soundVol", this.SoundVolume);
		SoundPlayerData.State playerData = this.PlayerData;
		string[] channelList = playerData.Data.GetChannelList();
		if (channelList.Length > 0)
		{
			IDataSaver localSaver = saver.GetLocalSaver("channels");
			for (int i = 0; i < channelList.Length; i++)
			{
				localSaver.SaveFloat(channelList[i], playerData.GetChannelVolume(channelList[i]));
			}
		}
	}

	public class Context
	{
		private List<ISound> sounds = new List<ISound>();

		private List<ISound> exclSounds = new List<ISound>();

		private static void AddToList (ISound snd, List<ISound> list)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (!list[i].IsPlaying)
				{
					list[i] = snd;
					return;
				}
			}
			list.Add(snd);
		}

		private static void StopList (List<ISound> list)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				list[i].Stop();
			}
			list.Clear();
		}

		private static void StopOverrideList (List<ISound> list, SoundClip overrider)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (overrider.Overrides(list[i].CurrentClip))
				{
					list[i].Stop();
					list.RemoveAt(i);
				}
			}
		}

		public void AddSound (ISound snd)
		{
			SoundPlayer.Context.AddToList(snd, this.sounds);
		}

		public void AddExclSound (ISound snd)
		{
			SoundPlayer.Context.AddToList(snd, this.exclSounds);
		}

		public void StopAll ()
		{
			SoundPlayer.Context.StopList(this.sounds);
			SoundPlayer.Context.StopList(this.exclSounds);
		}

		public void StopExcl ()
		{
			SoundPlayer.Context.StopList(this.exclSounds);
		}

		public void StopOverrides (SoundClip clip)
		{
			SoundPlayer.Context.StopOverrideList(this.sounds, clip);
			SoundPlayer.Context.StopOverrideList(this.sounds, clip);
		}

		public bool ExclEmpty
		{
			get
			{
				for (int i = this.exclSounds.Count - 1; i >= 0; i--)
				{
					if (!this.exclSounds[i].IsPlaying)
					{
						this.exclSounds.RemoveAt(i);
					}
				}
				return this.exclSounds.Count == 0;
			}
		}
	}
}
