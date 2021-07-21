//For changes, refer to FPSmodesetup documentation

using System;
using System.Collections.Generic;
using UnityEngine;
using ModStuff;

public class PlayerController : ControllerBase, IUpdatable, IBaseUpdateable, IPauseListener
{
	[SerializeField]
	private UpdatableLayer _updateLayer;

	[SerializeField]
	private Entity _player;

	[SerializeField]
	private Camera _mainCam;

	[SerializeField]
	private float _touchDeadZone = 0.1f;

	[SerializeField]
	private MappedInput _input;

	[SerializeField]
	private InputButton _xAxis;

	[SerializeField]
	private InputButton _yAxis;

	[SerializeField]
	private float _deadZone = 0.35f;

	[SerializeField]
	private InputButton _left;

	[SerializeField]
	private InputButton _right;

	[SerializeField]
	private InputButton _up;

	[SerializeField]
	private InputButton _down;

	[SerializeField]
	private InputButton _rollButton;

	[SerializeField]
	private PlayerController.AttackButtonData[] _attacks;

	private List<PlayerController.RelaseAction> releaseActions = new List<PlayerController.RelaseAction>();
	private Vector3 lastDir;
	private bool isMoving;
	private PrioMouseHandler.Tag mouseTag;
	private bool isTouched;
	private bool forceTouch;
	private Vector2 touchdir;
	private EntityAction rollAction;
	private EntityAction.ActionData savedData;
	private List<PlayerController.BtnState> btnSignals = new List<PlayerController.BtnState>();

	[Serializable]
	public class AttackButtonData
	{
		public string attackName;

		public InputButton button;

		public AttackButtonData ()
		{
		}
	}

	private struct RelaseAction
	{
		public EntityAction action;

		public InputButton button;

		public RelaseAction (EntityAction action, InputButton button)
		{
			this.action = action;
			this.button = button;
		}
	}

	private struct BtnState
	{
		public InputButton btn;

		public bool down;

		public BtnState (InputButton btn)
		{
			this.btn = btn;
			this.down = true;
		}
	}

	public PlayerController ()
	{
	}

    public bool AllowStick { get; set; } = true;
    public bool AllowRoll { get; set; } = true;

    protected override UpdatableLayer GetUpdatableLayer ()
	{
		return this._updateLayer;
	}

	public override void ControlEntity (Entity ent)
	{
		this._player = ent;
		this.releaseActions = new List<PlayerController.RelaseAction>();
	}

	public override void ReleaseEntity (Entity ent)
	{
		if (this._player == ent)
		{
			this._player = null;
			this.releaseActions.Clear();
		}
	}

	public override bool HasEntity (Entity ent)
	{
		return this._player == ent;
	}

	public void SetEnabled (bool enable)
	{
		base.enabled = enable;
		if (!enable && this.isMoving)
		{
			this.isMoving = false;
			this._player.StopMoving();
		}
	}

	public bool GetKeyForAction (string name, out InputButton key)
	{
		if (name == "roll")
		{
			key = this._rollButton;
			return true;
		}
		for (int i = 0; i < this._attacks.Length; i++)
		{
			if (this._attacks[i].attackName == name)
			{
				key = this._attacks[i].button;
				return true;
			}
		}
		key = null;
		return false;
	}

	private void Awake ()
	{
		if (this._player == null)
		{
			this._player = base.GetComponent<Entity>();
		}
		if (this._mainCam == null)
		{
			GameObject gameObject = GameObject.Find("Main Camera");
			if (gameObject != null)
			{
				this._mainCam = gameObject.GetComponent<Camera>();
			}
		}
		if (PlatformInfo.Current.AllowMouseInput)
		{
			this.mouseTag = PrioMouseHandler.GetHandler(this._input).GetListener(0, new PrioMouseHandler.MouseDownFunc(this.MouseDown), null, new PrioMouseHandler.MouseUpFunc(this.MouseUp));
		}
	}

	private void OnDestroy ()
	{
		if (this.mouseTag != null)
		{
			this.mouseTag.Stop();
			this.mouseTag = null;
		}
	}

	private static Vector3 ScreenToViewport (Vector3 P)
	{
		P.x /= (float)Screen.width;
		P.y /= (float)Screen.height;
		return P;
	}

	private static Vector3 WorldToViewport (Camera cam, Vector3 P)
	{
		return cam.WorldToViewportPoint(P);
	}

	private bool MouseDown (int btn, Vector2 P)
	{
		if (!this.isTouched)
		{
			this.isTouched = true;
			return true;
		}
		return false;
	}

	private void MouseUp (int btn)
	{
		this.isTouched = false;
	}

	public void SignalMouseDir (Vector2 D)
	{
		this.touchdir = D;
		this.forceTouch = true;
	}

	public void SignalMouseUp ()
	{
		this.forceTouch = false;
	}

	private bool GetTouchDir (out Vector2 dir)
	{
		if (this.isTouched)
		{
			Vector3 a = PlayerController.ScreenToViewport(this.mouseTag.Pos);
			Vector3 b = PlayerController.WorldToViewport(this._mainCam, this._player.WorldPosition);
			dir = a - b;
			return true;
		}
		dir = Vector2.zero;
		return false;
	}

	private bool GetMoveDir (out Vector2 dir)
	{
		if (this.forceTouch)
		{
			dir = this.touchdir;
			if (dir.sqrMagnitude > 1f)
			{
				dir = dir.normalized;
			}
			this.forceTouch = false;
			return true;
		}
		Vector2 vector;
		if (this.GetTouchDir(out vector))
		{
			Vector2 vector2 = vector;
			vector2.x *= (float)(Screen.width / Screen.height);
			float sqrMagnitude = vector2.sqrMagnitude;
			float num = this._touchDeadZone * this._touchDeadZone;
			if (sqrMagnitude > num && sqrMagnitude < num * 6f)
			{
				dir = vector.normalized;
				return true;
			}
		}

		//If in fpsmode, use fpsmode movement keys, otherwise use vanilla ones
		if (fpsControls)
		{
			vector.x = (Input.GetKey(fps_right) ? 1f : 0f) - (Input.GetKey(fps_left) ? 1f : 0f);
			vector.y = (Input.GetKey(fps_forward) ? 1f : 0f) - (Input.GetKey(fps_backward) ? 1f : 0f);
		}
		else
		{
			vector.x = this._input.PressedValue(this._right) - this._input.PressedValue(this._left);
			vector.y = this._input.PressedValue(this._up) - this._input.PressedValue(this._down);
		}

		if (vector.sqrMagnitude > 0.1f)
		{
			dir = vector;
			return true;
		}
		vector = new Vector2(this._input.GetAxisValue(this._xAxis), this._input.GetAxisValue(this._yAxis));
		if (vector.sqrMagnitude > this._deadZone * this._deadZone)
		{
			if (vector.sqrMagnitude > 1f)
			{
				vector.Normalize();
			}
			dir = vector;
			return true;
		}
		dir = Vector2.zero;
		return false;
	}

	public EntityAction.ActionData GetFreeData ()
	{
		if (this.savedData == null)
		{
			this.savedData = new EntityAction.ActionData();
		}
		return this.savedData;
	}

	public void SignalPress (InputButton btn)
	{
		for (int i = 0; i < this.btnSignals.Count; i++)
		{
			if (this.btnSignals[i].btn == btn)
			{
				return;
			}
		}
		this.btnSignals.Add(new PlayerController.BtnState(btn));
	}

	public void SignalRelease (InputButton btn)
	{
		for (int i = this.btnSignals.Count - 1; i >= 0; i--)
		{
			if (this.btnSignals[i].btn == btn)
			{
				this.btnSignals.RemoveAt(i);
				return;
			}
		}
	}

	private bool CheckButtonDown (InputButton btn)
	{
		if (this.btnSignals.Count > 0)
		{
			for (int i = this.btnSignals.Count - 1; i >= 0; i--)
			{
				PlayerController.BtnState btnState = this.btnSignals[i];
				if (btnState.btn == btn && btnState.down)
				{
					return true;
				}
			}
		}
		return this._input.IsDown(btn);
	}

	private bool CheckButtonPressed (InputButton btn)
	{
		if (this.btnSignals.Count > 0)
		{
			for (int i = this.btnSignals.Count - 1; i >= 0; i--)
			{
				if (this.btnSignals[i].btn == btn)
				{
					return true;
				}
			}
		}
		return this._input.IsPressed(btn);
	}

	private void UpdateRoll (Vector3 V)
	{
        if (!AllowRoll) return;
		if (this.CheckButtonDown(this._rollButton) && V.sqrMagnitude > 0.001f)
		{
			EntityAction.ActionData freeData = this.GetFreeData();
			freeData.dir = V.normalized;
			if (this._player.DoAction("roll", freeData))
			{
				this.rollAction = this._player.GetAction("roll");
				return;
			}
		}
		else if (this.rollAction != null)
		{
			if (this.rollAction.IsReleasable() && !this.CheckButtonPressed(this._rollButton))
			{
				this.rollAction.ReleaseAction();
			}
			if (this.rollAction.IsActive)
			{
				if (V.sqrMagnitude > 0.001f)
				{
					EntityAction.ActionData freeData2 = this.GetFreeData();
					freeData2.dir = V.normalized;
					this.rollAction.UpdateData(freeData2);
					return;
				}
			}
			else
			{
				this.rollAction = null;
			}
		}
	}

	private static bool IsRectilinear (Vector3 V)
	{
		return Mathf.Abs(V.x) > 0.9f || Mathf.Abs(V.z) > 0.9f;
	}

	//Main Update
	void IUpdatable.UpdateObject ()
	{
		if (this._player == null)
		{
			return;
		}
		if (this._player.InactiveOrDead)
		{
			this.ReleaseEntity(this._player);
			return;
		}

        //Check keybind
        DebugCommands.Instance.CheckCustomKeys();

		//Pan camera if fpsmode is on, then move if freecam is on
		if (nonDefaultCamera)
        {
			ModCamera.Instance.ModCamUpdate();
            if (lockPlayer) return;
		}

		Vector3 zero = Vector3.zero;
		Vector2 vector;

		if (this.GetMoveDir(out vector))
		{
			if (fpsControls) { vector = ModCamera.RotByDegrees(vector, -ModCamera.Instance.cameraRotation.y); } //If fpsmode, rotate vector to facing direction
			zero = new Vector3(vector.x, 0f, vector.y);
			if (vector.magnitude < 0.0625f)
			{
				zero.Normalize();
				if (this.rollAction == null || !this.rollAction.IsActive)
				{
					this._player.TurnTo(zero, 1080f);
				}
				if (this.isMoving)
				{
					this.isMoving = false;
					this._player.StopMoving();
				}
			}
			else
			{
				bool flag = this.isMoving;
				this.isMoving = this._player.SetMoveDirection(zero, true);
				this.UpdateRoll(zero);
				if (!flag || !PlayerController.IsRectilinear(zero) || Vector3.Dot(zero, this.lastDir) < 0.7f)
				{
					this.lastDir = zero;
				}
				else
				{
					this.lastDir = Vector3.RotateTowards(this.lastDir, zero, 12.566371f * Time.deltaTime, 10f * Time.deltaTime);
				}
			}
		}
		else
		{
			this.UpdateRoll(Vector3.zero);
			bool flag2 = this.isMoving;
			this.isMoving = false;
			if (flag2)
			{
				this._player.TurnTo(this.lastDir, 0f);
				this._player.StopMoving();
			}
		}
		for (int i = this.releaseActions.Count - 1; i >= 0; i--)
		{
			PlayerController.RelaseAction relaseAction = this.releaseActions[i];
			if (!this.CheckButtonPressed(relaseAction.button))
			{
				this._player.ReleaseAction(relaseAction.action);
				this.releaseActions.RemoveAt(i);
			}
		}

		//In fpsmode, fire actions using left mouse button, otherwise use vanilla code 
		if (fpsControls)
		{
			// Check if a new weapon was selected
			for (int j = 0; j < this.weapon_buttons.Length; j++)
			{
				if (Input.GetKeyDown(weapon_buttons[j]))
				{
					if (hud_elements[j].transform.GetChild(0).gameObject.activeSelf == true && active_weapon != j)
					{
						active_weapon = j;
						SoundPlayer.Instance.PlayPositionedSound(hud_sounds[UnityEngine.Random.Range(0, 2)], _player.transform.position);
						RefreshHud();
					}
				}
			}
			//Check if the scroll wheel moved
			if (Input.mouseScrollDelta.y < 0 || Input.mouseScrollDelta.y > 0)
			{
				int scrollDirection = (Input.mouseScrollDelta.y < 0 ? 1 : -1);
				if (!(Input.GetKey(KeyCode.Mouse1) && thirdperson))
				{
					int new_weapon = active_weapon;
					for (int i = 0; i < weapon_buttons.Length; i++)
					{
						new_weapon += scrollDirection;
						if (new_weapon >= weapon_buttons.Length) { new_weapon = 0; }
						if (new_weapon < 0) { new_weapon = weapon_buttons.Length - 1; }
						if (hud_elements[new_weapon].transform.GetChild(0).gameObject.activeSelf == true && new_weapon != active_weapon)
						{
							active_weapon = new_weapon;
							SoundPlayer.Instance.PlayPositionedSound(hud_sounds[UnityEngine.Random.Range(0, 2)], _player.transform.position);
							RefreshHud();
							break;
						}
					}
				}
			}

			//Check if mouse button has been pressed
            //If the stick is disallowed and the active weapons is the stick (0), cancel
			if (CheckButtonDown(mouseClick) && !(active_weapon == 0 && !AllowStick))
			{
				EntityAction.ActionData freeData = GetFreeData();
				//Change attack direction
				freeData.dir = (attackFromCamera ? ModCamera.Instance.PlayerDirectionToVector3() : zero);
				//freeData.dir.y = -Mathf.Sin(cameraRotation.y * 0.0174532924f); //Enable for 3D projectiles!
				AttackAction attack = _player.GetAttack(_attacks[order_switch[active_weapon]].attackName);
				if (attack != null && _player.Attack(_attacks[order_switch[active_weapon]].attackName, freeData) && attack.IsReleasable())
				{
					releaseActions.Add(new PlayerController.RelaseAction(attack, mouseClick));
				}
			}
		}
		else
		{
			for (int j = 0; j < this._attacks.Length; j++)
            {
                //If the stick is disallowed and the active weapons is the stick (0), cancel
                if (this.CheckButtonDown(this._attacks[j].button) && !(j == 0 && !AllowStick))
				{
					EntityAction.ActionData freeData = this.GetFreeData();
					freeData.dir = zero;
					AttackAction attack = this._player.GetAttack(this._attacks[j].attackName);
					if (attack != null && this._player.Attack(this._attacks[j].attackName, freeData) && attack.IsReleasable())
					{
						this.releaseActions.Add(new PlayerController.RelaseAction(attack, this._attacks[j].button));
					}
				}
			}
		}

		for (int k = this.btnSignals.Count - 1; k >= 0; k--)
		{
			PlayerController.BtnState value = this.btnSignals[k];
			value.down = false;
			this.btnSignals[k] = value;
		}
	}

	void IPauseListener.PauseChanged (bool pause)
	{
        if (pause && this._player != null)
		{
			this._player.StopMoving();
			for (int i = this.releaseActions.Count - 1; i >= 0; i--)
			{
				PlayerController.RelaseAction relaseAction = this.releaseActions[i];
				this._player.ReleaseAction(relaseAction.action);
				this.releaseActions.RemoveAt(i);
			}
		}
	}

	//====================================================================================
	//Start of Mod code
	//====================================================================================
	//Main bool variables
    bool nonDefaultCamera;
    bool ittleUsesCameraRot;
    bool lockPlayer;
    bool attackFromCamera;
    bool fpsControls;
    bool thirdperson;
	bool freecam;

	//Weapon switching
	private GameObject[] hud_elements;
	private SoundClip[] hud_sounds;
	private Vector3 hud_default;
	private Vector3 hud_unselected;
	private int active_weapon;
	private int[] order_switch;

	//Control keys and variables
	InputButton mouseClick;
	public KeyCode[] weapon_buttons;
    public KeyCode fps_right;
    public KeyCode fps_left;
    public KeyCode fps_forward;
    public KeyCode fps_backward;

	//Setup fpsmode
    //what do I do with you my child?
	public void SetupFPSMode (int mode)
	{
		//Configure HUD scale vectors
		hud_default = new Vector3(0.7f, 0.7f, 1f);
		hud_unselected = new Vector3(0.5f, 0.5f, 1f);
		switch (mode)
		{
			//Default mode
			case 0:
				Cursor.visible = true;
                nonDefaultCamera = false;
                fpsControls = false;
                freecam = false;
				thirdperson = false;
                ittleUsesCameraRot = false;
                lockPlayer = false;
                attackFromCamera = false;
				break;
			//First person mode
			case 1:
                nonDefaultCamera = true;
                fpsControls = true;
                freecam = false;
				thirdperson = false;
                ittleUsesCameraRot = true;
                lockPlayer = false;
                attackFromCamera = true;
                break;
			//Third person mode
			case 2:
                nonDefaultCamera = true;
                fpsControls = true;
				freecam = false;
				thirdperson = true;
                ittleUsesCameraRot = false;
                lockPlayer = false;
                attackFromCamera = false;
                break;
			//Free mode
			case 3:
                nonDefaultCamera = true;
                fpsControls = false;
				freecam = true;
				thirdperson = false;
                ittleUsesCameraRot = false;
                lockPlayer = true;
                attackFromCamera = false;
				break;
			default:
				break;
		}

		//If hud_sounds is not set, set all variables for the script
		if (hud_sounds == null)
		{
			//Configure HUD sounds
			hud_sounds = new SoundClip[3];
			foreach (SoundClip sound in Resources.FindObjectsOfTypeAll(typeof(SoundClip)))
			{
				if (sound.RawSoundName == "Menu_Buttons_Forward") { hud_sounds[0] = sound; }
				if (sound.RawSoundName == "Menu_Buttons_Back") { hud_sounds[1] = sound; }
				if (sound.RawSoundName == "Menu_Buttons_Browse") { hud_sounds[2] = sound; }
			}
			//Change the weapon order to stick/forcewand/dynamite/icering
			order_switch = new int[] { 0, 1, 3, 2 };
			//Configure mouse click button
			mouseClick = new InputButton();
			mouseClick.AddKey(KeyCode.Mouse0);
		}
		RefreshHud();
	}

    //Free camera movement
    public void FreeCamPlayerMovement(int mode)
    {
		if (freecam)
        {
			switch (mode)
            {
                case 0:
                case 3:
                    fpsControls = false;
                    lockPlayer = true;
					break;
                case 1:
                    fpsControls = true;
                    lockPlayer = false;
                    break;
                case 2:
                    fpsControls = false;
                    lockPlayer = false;
					break;
                default:
                    break;
            }
            RefreshHud();
        }
    }

    //Refresh weapons on hud
    void RefreshHud ()
	{
		//If hud elements is not set, set it. If it failed, exit
		if (hud_elements == null)
		{
			GameObject melee = GameObject.Find("MeleeIcon_anchor");
			GameObject wand = GameObject.Find("ForceWandIcon_anchor");
			GameObject ring = GameObject.Find("IceRingIcon_anchor");
			GameObject dynamite = GameObject.Find("DynamiteIcon_anchor");
			if (melee == null || wand == null || ring == null || dynamite == null) { return; }

			hud_elements = new GameObject[] { melee, wand, dynamite, ring };
		}
		for (int i = 0; i < hud_elements.Length; i++)
		{
			hud_elements[i].transform.GetChild(0).localScale = (fpsControls && (i != active_weapon)) ? hud_unselected : hud_default;
		}
	}

}
