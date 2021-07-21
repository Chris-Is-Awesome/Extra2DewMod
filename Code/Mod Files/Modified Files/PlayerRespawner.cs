using System;
using System.Collections.Generic;
using UnityEngine;
using ModStuff;

public class PlayerRespawner : UpdateBehaviour, IUpdatable, IBaseUpdateable
{
     static List<PlayerRespawner> allRespawners = new List<PlayerRespawner>();

     [SerializeField]
     float _deathTime;

     [SerializeField]
     float _weakDeathTime;

     [SerializeField]
     Vector3 _deathOffset = Vector3.zero;

     [SerializeField]
     bool _doDeathOffset = true;

     [SerializeField]
     string _bgFadeCamera;

     [SerializeField]
     Color _bgFadeColor = Color.white;

     [SerializeField]
     float _bgFadeTime = 0.5f;

     [SerializeField]
     float _releaseCamTime = 1f;

     [SerializeField]
     FadeEffectData _fadeOut;

     [SerializeField]
     FadeEffectData _fadeIn;

     Entity ent;

     PlayerController controller;

     LevelCamera levelCam;

     FollowTransform followCam;

     EntityHUD hud;

     EntityObjectAttacher.Attacher attacher;

     EntityObjectAttacher.AttachTag attachTag;

     EntityLocalVarOverrider varOverrider;

     float timer;

     Vector3 spawnPos;

     Vector3 spawnDir;

     ObjectUpdater.PauseTag localPauseTag;

     Vector3 roomSpawnPos;

     Vector3 roomSpawnDir;

     bool useRoomSpawn;

     bool mayChangeLevel;

     float fadeTimer;

     float fadeTimeScale;

     Camera backCam;

     Color startFadeColor;

     LevelRoot levelRoot;

     public SaverOwner gameSaver;

     bool waitForDespawn;

     bool noFadeOut;

     bool inForceRespawn;

     bool overrideRoomSpawn;

     void Awake()
     {
          PlayerRespawner.allRespawners.Add(this);
          if (!string.IsNullOrEmpty(this._bgFadeCamera))
          {
               this.backCam = TransformUtility.GetByName<Camera>(this._bgFadeCamera);
               if (this.backCam != null)
               {
                    this.startFadeColor = this.backCam.backgroundColor;
               }
          }
     }

     void OnDestroy()
     {
          PlayerRespawner.allRespawners.Remove(this);
          if (this.ent != null)
          {
               this.UnregEvents(this.ent);
          }
          if (this.backCam != null && this.fadeTimer > 0f)
          {
               this.backCam.backgroundColor = this.startFadeColor;
          }
          if (this.localPauseTag != null)
          {
               this.localPauseTag.Release();
               this.localPauseTag = null;
          }
     }

     void RegEvents(Entity player)
     {
          player.LocalEvents.DeathListener += this.PlayerDied;
          player.LocalEvents.RoomChangeListener += this.RoomChangeStart;
          player.LocalEvents.RoomChangeDoneListener += this.RoomChanged;
     }

     void UnregEvents(Entity player)
     {
          player.LocalEvents.DeathListener -= this.PlayerDied;
          player.LocalEvents.RoomChangeListener -= this.RoomChangeStart;
          player.LocalEvents.RoomChangeDoneListener -= this.RoomChanged;
     }

     public void Init(Entity player, PlayerController controller, LevelCamera levelCam, FollowTransform followCam, EntityHUD hud, SaverOwner gameSaver, EntityObjectAttacher.Attacher attacher, EntityObjectAttacher.AttachTag attachTag, EntityLocalVarOverrider varOverrider, Vector3 P, Vector3 D)
     {
          this.ent = player;
          this.controller = controller;
          this.RegEvents(player);
          this.levelCam = levelCam;
          this.followCam = followCam;
          this.spawnPos = P;
          this.roomSpawnPos = P;
          this.spawnDir = D;
          this.roomSpawnDir = D;
          this.hud = hud;
          this.gameSaver = gameSaver;
          this.attacher = attacher;
          this.attachTag = attachTag;
          this.varOverrider = varOverrider;
          base.enabled = false;
          IDataSaver saver = gameSaver.GetSaver("/local/start", true);
          Vector3 vector;
          Vector3 vector2;
          if (saver != null && saver.LoadData("level") == Utility.GetCurrentSceneName() && SceneDoor.GetSpawnPoint(saver.LoadData("door"), out vector, out vector2))
          {
               this.spawnPos = vector;
               this.spawnDir = vector2;
          }
     }

     public void UpdateSpawnPoint(Vector3 pos, Vector3 dir, SceneDoor door, bool dontOverride)
     {
        // Game Options: Fluffy warp fix
       if (ModStuff.Options.GameOptions.Instance.FluffyWarpFix && ModMaster.GetMapName() == "FluffyFields") { return; }

          this.spawnPos = pos;
          this.spawnDir = dir;
          if (door != null && this.gameSaver != null)
          {
               if (dontOverride && this.gameSaver.GetSaver("/local/start", true) != null)
               {
                    return;
               }
               door.SaveStartPos(this.gameSaver, door.name, Utility.GetCurrentSceneName());
               this.gameSaver.SaveLocal(true, true);
          }
     }

     public void UpdateRoomSpawnPoint(Vector3 pos, Vector3 dir)
     {
          this.overrideRoomSpawn = true;
          this.roomSpawnPos = pos;
          this.roomSpawnDir = dir;
     }

     public void ForceRespawn()
     {
          if (this.inForceRespawn)
          {
               return;
          }
          this.inForceRespawn = true;
          if (this.WillChangeLevels())
          {
               this.localPauseTag = ObjectUpdater.Instance.RequestPause(null);
               Killable entityComponent = this.ent.GetEntityComponent<Killable>();
               if (entityComponent != null)
               {
                    entityComponent.CurrentHp = entityComponent.MaxHp;
               }
               this.ChangeLevelRespawn();
               return;
          }
          ObjectUpdater.PauseTag pauseTag = ObjectUpdater.Instance.RequestPause(null);
          OverlayFader.OnDoneFunc onDoneFunc = delegate ()
          {
               this.inForceRespawn = false;
               pauseTag.Release();
               Killable entityComponent2 = this.ent.GetEntityComponent<Killable>();
               if (entityComponent2 != null)
               {
                    Killable.DeathData deathData = new Killable.DeathData(true);
                    entityComponent2.ForceDeath(0f, deathData, true);
               }
          };
          if (this._fadeOut != null)
          {
               Vector3 value = CoordinateTransformer.ToViewport("Main Camera", this.ent.WorldPosition);
               OverlayFader.StartFade(this._fadeOut, true, onDoneFunc, new Vector3?(value));
               return;
          }
          onDoneFunc();
     }

     public void DoRespawn()
     {
          // Invoke OnPlayerDeath (post-anim) events
          if (GameStateNew.isPlayerDead) { GameStateNew.OnPlayerDied(false); }
          // Invoke OnPlayerSpawn (respawn) events
          else if (!GameStateNew.isWarping) { GameStateNew.OnPlayerSpawned(true); }

          this.inForceRespawn = false;
          if (this._doDeathOffset && this.levelRoot != null)
          {
               this.levelRoot.gameObject.SetActive(true);
          }
          ProjectileFactory.Instance.DeactivateAll();
          Vector3 vector = (!this.useRoomSpawn) ? this.spawnPos : this.roomSpawnPos;
          Vector3 dir = (!this.useRoomSpawn) ? this.spawnDir : this.roomSpawnDir;

          // Boss Rush:
          if (BossRush.Instance.IsActive)
          {
               BossRush brm = BossRush.Instance;
               vector = brm.GetEndPosition();
               dir = brm.GetFacingDirection();
            ModText.QuickText("Called playerREEEEEspawner");

        }

        this.ent.RealTransform.position = vector;
          this.ent.Activate();
          this.ent.TurnTo(dir, 0f);
          if (this.varOverrider != null)
          {
               this.varOverrider.Apply(this.ent);
          }
          this.RegEvents(this.ent);
          base.enabled = false;
          this.roomSpawnPos = vector;
          this.roomSpawnDir = dir;
          this.controller.ControlEntity(this.ent);
          if (this.attachTag != null)
          {
               this.attachTag.Free();
          }
          this.attachTag = null;
          if (this.attacher != null)
          {
               this.attachTag = this.attacher.Attach(this.ent);
          }
          if (this.followCam != null)
          {
               this.followCam.ClearFollowScale();
          }

          LevelRoom roomForPosition = LevelRoom.GetRoomForPosition(roomSpawnPos, null);
          if (roomForPosition != null)
          {
               roomForPosition.SetImportantPoint(vector);
               LevelRoom.SetCurrentActiveRoom(roomForPosition, true);
               if (this.levelCam != null)
               {
                    this.levelCam.SetRoom(roomForPosition);
               }
          }
          if (this.hud != null)
          {
               this.hud.Observe(this.ent, this.controller);
          }
          if (this._fadeIn != null || this._fadeOut != null)
          {
               FadeEffectData data = this._fadeIn ?? this._fadeOut;
               Vector3 value = new Vector3(0f, 0f, 0f);
               OverlayFader.StartFade(data, false, null, new Vector3?(value));
          }

          // Trigger spawn events
          GameState gameState = Singleton<GameState>.Instance;
          gameState.hasPlayerSpawned = true;
          gameState.OnPlayerSpawn(true);
     }

     void PlayerDied(Entity player)
     {
          Killable entityComponent = player.GetEntityComponent<Killable>();
          bool flag = entityComponent != null && (entityComponent.CurrentHp > 0f || entityComponent.SilentDeath);
          this.mayChangeLevel = !flag;
          this.UnregEvents(player);
          base.enabled = true;
          this.timer = this._deathTime;
          player.SaveState();
          if (!flag)
          {
               // Invoke OnPlayerDeath (pre-anim) events
               if (!GameStateNew.isWarping) { GameStateNew.OnPlayerDied(true); GameStateNew.isPlayerDead = true; }

               ProjectileFactory.Instance.DeactivateAll();
               if (this._doDeathOffset)
               {
                    LevelRoom roomForPosition = LevelRoom.GetRoomForPosition(player.WorldTracePosition, null);
                    this.levelCam.ReleaseRoom(this._releaseCamTime);
                    player.RealTransform.position = player.WorldPosition + this._deathOffset;
                    if (roomForPosition != null)
                    {
                         this.levelRoot = roomForPosition.LevelRoot;
                         LevelRoom.SetCurrentActiveRoom(null, false);
                         if (this.levelRoot != null)
                         {
                              this.levelRoot.gameObject.SetActive(false);
                         }
                    }
               }
               if (this.backCam != null)
               {
                    this.fadeTimer = this._bgFadeTime;
                    this.fadeTimeScale = 1f / this.fadeTimer;
               }
               this.useRoomSpawn = false;
          }
          else
          {
               this.timer = this._weakDeathTime;
               this.fadeTimer = 0f;
               this.useRoomSpawn = true;
          }
          if (this.followCam != null)
          {
               this.followCam.SetFollowScale(new Vector3(0.9f, 0.25f, 0.9f));
          }
          this.noFadeOut = false;
          if (entityComponent != null && entityComponent.SilentDeath)
          {
               this.mayChangeLevel = true;
               this.useRoomSpawn = false;
               this.noFadeOut = true;
               this.timer = 0f;
          }
     }

     void RoomChangeStart(Entity ent, LevelRoom to, LevelRoom from, EntityEventsOwner.RoomEventData data)
     {
          this.overrideRoomSpawn = false;
     }

     void RoomChanged(Entity ent, LevelRoom ro, LevelRoom from, EntityEventsOwner.RoomEventData data)
     {
          if (!this.overrideRoomSpawn)
          {
               this.roomSpawnPos = data.targetPos;
               this.roomSpawnDir = data.targetDir;
          }
          ProjectileFactory.Instance.DeactivateAll();
     }

     void StartSpawnWait()
     {
          this.waitForDespawn = true;
          base.enabled = true;
     }

     bool WillChangeLevels()
     {
          if (this.gameSaver == null)
          {
               return false;
          }
          IDataSaver saver = this.gameSaver.GetSaver("/local/start", true);
          if (saver == null)
          {
               return false;
          }
          if (saver.LoadData("level") != Utility.GetCurrentSceneName())
          {
               return true;
          }
          LevelData currentData = LevelData.GetCurrentData();
          return currentData != null && currentData.AlwaysReload;
     }

     bool ShouldChangeLevels()
     {
          return this.mayChangeLevel && this.WillChangeLevels();
     }

     void ChangeLevelRespawn()
     {
          IDataSaver saver = this.gameSaver.GetSaver("/local/start", false);
          string targetScene = saver.LoadData("level");
          string targetDoor = saver.LoadData("door");
          SceneDoor.StartLoad(targetScene, targetDoor, this._fadeOut, this.gameSaver, null);
     }

     void IUpdatable.UpdateObject()
     {
          if (this.waitForDespawn)
          {
               if (!this.ent.gameObject.activeInHierarchy)
               {
                    base.enabled = false;
                    this.waitForDespawn = false;
                    this.DoRespawn();
               }
               return;
          }
          this.timer -= Time.deltaTime;
          if (this.backCam != null && this.fadeTimer > 0f)
          {
               this.fadeTimer -= Time.deltaTime;
               float t = Mathf.Max(0f, this.fadeTimer * this.fadeTimeScale);
               this.backCam.backgroundColor = Color.Lerp(this.startFadeColor, this._bgFadeColor, t);
          }
          if (this.timer <= 0f)
          {
               if (this.backCam != null)
               {
                    this.backCam.backgroundColor = this.startFadeColor;
               }
               base.enabled = false;
               if (this.ShouldChangeLevels())
               {
                    this.ChangeLevelRespawn();
                    return;
               }
               if (this._fadeOut != null && !this.noFadeOut)
               {
                    Vector3 value = CoordinateTransformer.ToViewport("Main Camera", this.ent.WorldPosition);
                    OverlayFader.StartFade(this._fadeOut, true, new OverlayFader.OnDoneFunc(this.StartSpawnWait), new Vector3?(value));
                    return;
               }
               this.DoRespawn();
          }
     }

     public static PlayerRespawner GetActiveInstance()
     {
          for (int i = PlayerRespawner.allRespawners.Count - 1; i >= 0; i--)
          {
               PlayerRespawner playerRespawner = PlayerRespawner.allRespawners[i];
               if (playerRespawner.ent != null && !playerRespawner.ent.InactiveOrDead && !playerRespawner.inForceRespawn)
               {
                    return playerRespawner;
               }
          }
          return null;
     }

     public static PlayerRespawnInhibit.InhibitTag InhibitRespawn()
     {
          return PlayerRespawnInhibit.InhibitRespawn();
     }

     public static bool RespawnInhibited()
     {
          return PlayerRespawnInhibit.IsInhibited;
     }
}
