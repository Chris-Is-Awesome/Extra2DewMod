using System;
using UnityEngine;

namespace Sound
{
	public struct BasicSoundData
	{
		public BasicSoundData (float volume = 1f, float pitch = 1f)
		{
			this.clip = null;
			this.sndContext = null;
			this.volume = volume;
			this.baseVolume = volume;
			this.pitch = pitch;
			this.basePitch = pitch;
			this.clipFrequency = 1f;
			this.clipLength = 0f;
			this.source = null;
			this.nextPos = Vector3.zero;
			this.hasPos = false;
		}

		public void Play (ISound thisSnd, SoundPlayer owner, ISound afterSound = null)
		{
			if (this.source == null)
			{
				if (owner != null)
				{
					AudioClip audioClip = this.clip.GetClip();
					if (audioClip == null && !this.clip.EmptyIsValid)
					{
						return;
					}
					if (this.clip.ContextExclusive != SoundClip.ExclusiveMode.Ignore)
					{
						SoundPlayer.Context context = owner.GetContext(this.sndContext);
						if (context != null)
						{
							if (this.clip.ContextExclusive == SoundClip.ExclusiveMode.Exclusive)
							{
								if (!context.ExclEmpty)
								{
									return;
								}
							}
							else if (this.clip.ContextExclusive == SoundClip.ExclusiveMode.Override)
							{
								context.StopExcl();
							}
							if (this.clip.HasOverrides)
							{
								context.StopOverrides(this.clip);
							}
							if (this.clip.ContextExclusive != SoundClip.ExclusiveMode.Normal)
							{
								context.AddExclSound(thisSnd);
							}
							else
							{
								context.AddSound(thisSnd);
							}
						}
					}
					if (audioClip == null)
					{
						return;
					}
					if (this.clip.Exclusive == SoundClip.ExclusiveMode.Normal)
					{
						this.source = owner.GetFreeSource();
					}
					else
					{
						this.source = owner.GetExclusiveSource(this.clip);
					}
					if (this.source == null)
					{
						return;
					}
					if (this.hasPos)
					{
						this.source.transform.position = this.nextPos;
						this.hasPos = false;
					}
					this.source.clip = audioClip;
					this.basePitch = this.clip.GetPitch(audioClip);
					this.source.pitch = this.basePitch * this.pitch;
					if (this.clip.MaxDistance > 0f)
					{
						this.source.maxDistance = this.clip.MaxDistance;
					}
					else
					{
						this.source.maxDistance = owner.DefaultMaxDistance;
					}
					this.source.spatialBlend = ((!this.clip.Is3DSound) ? 0f : 1f);
					this.source.outputAudioMixerGroup = this.clip.MixerGroup;
					this.clipFrequency = 1f / (float)audioClip.frequency;
					this.clipLength = audioClip.length;
					this.source.loop = this.clip.Loop;
					this.baseVolume = this.clip.GetBaseVolume(audioClip);
					this.source.volume = this.baseVolume * this.volume;
					this.source.enabled = true;
					if (afterSound != null)
					{
						this.source.PlayDelayed(afterSound.ClipLength - afterSound.CurrentTime);
					}
					else
					{
						this.source.Play();
					}
				}
			}
			else
			{
				this.source.Play();
			}
		}

		public void PlayMod (ISound thisSnd, SoundPlayer owner, ISound afterSound = null)
		{
			if (this.source == null)
			{
				if (owner != null)
				{
					AudioClip audioClip = this.clip.GetClipCertain();
					if (audioClip == null && !this.clip.EmptyIsValid)
					{
						return;
					}
					if (this.clip.ContextExclusive != SoundClip.ExclusiveMode.Ignore)
					{
						SoundPlayer.Context context = owner.GetContext(this.sndContext);
						if (context != null)
						{
							if (this.clip.ContextExclusive == SoundClip.ExclusiveMode.Exclusive)
							{
								if (!context.ExclEmpty)
								{
									return;
								}
							}
							else if (this.clip.ContextExclusive == SoundClip.ExclusiveMode.Override)
							{
								context.StopExcl();
							}
							if (this.clip.HasOverrides)
							{
								context.StopOverrides(this.clip);
							}
							if (this.clip.ContextExclusive != SoundClip.ExclusiveMode.Normal)
							{
								context.AddExclSound(thisSnd);
							}
							else
							{
								context.AddSound(thisSnd);
							}
						}
					}
					if (audioClip == null)
					{
						return;
					}
					if (this.clip.Exclusive == SoundClip.ExclusiveMode.Normal)
					{
						this.source = owner.GetFreeSource();
					}
					else
					{
						this.source = owner.GetExclusiveSource(this.clip);
					}
					if (this.source == null)
					{
						return;
					}
					if (this.hasPos)
					{
						this.source.transform.position = this.nextPos;
						this.hasPos = false;
					}
					this.source.clip = audioClip;
					this.basePitch = this.clip.GetPitch(audioClip);
					this.source.pitch = this.basePitch * this.pitch;
					if (this.clip.MaxDistance > 0f)
					{
						this.source.maxDistance = this.clip.MaxDistance;
					}
					else
					{
						this.source.maxDistance = owner.DefaultMaxDistance;
					}
					this.source.spatialBlend = ((!this.clip.Is3DSound) ? 0f : 1f);
					this.source.outputAudioMixerGroup = this.clip.MixerGroup;
					this.clipFrequency = 1f / (float)audioClip.frequency;
					this.clipLength = audioClip.length;
					this.source.loop = this.clip.Loop;
					this.baseVolume = this.clip.GetBaseVolume(audioClip);
					this.source.volume = this.baseVolume * this.volume;
					this.source.enabled = true;
					if (afterSound != null)
					{
						this.source.PlayDelayed(afterSound.ClipLength - afterSound.CurrentTime);
					}
					else
					{
						this.source.Play();
					}
				}
			}
			else
			{
				this.source.Play();
			}
		}

		public void Pause ()
		{
			if (this.source != null)
			{
				this.source.Pause();
			}
		}

		public void Stop (SoundPlayer owner)
		{
			if (owner != null && this.source != null)
			{
				AudioSource audioSource = this.source;
				this.source = null;
				owner.ReleaseSource(audioSource);
			}
		}

		public void SetPos (Vector3 pos)
		{
			if (this.source != null)
			{
				this.source.transform.position = pos;
			}
			else
			{
				this.nextPos = pos;
				this.hasPos = true;
			}
		}

		public bool FreeSource (AudioSource src, bool forced)
		{
			if (forced || this.source == src)
			{
				this.source = null;
			}
			return this.source == null;
		}

		public float Volume
		{
			get
			{
				return this.volume;
			}
			set
			{
				this.volume = value;
				if (this.source != null)
				{
					this.source.volume = this.volume * this.baseVolume;
				}
			}
		}

		public float Pitch
		{
			get
			{
				return this.pitch;
			}
			set
			{
				this.pitch = value;
				if (this.source != null)
				{
					this.source.pitch = this.pitch * this.basePitch;
				}
			}
		}

		public float CurrentTime
		{
			get
			{
				return (float)this.source.timeSamples * this.clipFrequency;
			}
		}

		public float ClipLength
		{
			get
			{
				return this.clipLength;
			}
		}

		public bool IsPlaying
		{
			get
			{
				return this.source != null && this.source.isPlaying;
			}
		}

		public SoundClip clip;

		public object sndContext;

		public float baseVolume;

		public float volume;

		public float basePitch;

		public float pitch;

		public float clipFrequency;

		public float clipLength;

		public AudioSource source;

		private Vector3 nextPos;

		private bool hasPos;
	}
}
