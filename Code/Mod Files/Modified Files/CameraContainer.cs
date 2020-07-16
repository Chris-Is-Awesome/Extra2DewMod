using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class CameraContainer : MonoBehaviour
{
	[SerializeField]
	private CameraBehaviour[] _behaviours;
	[SerializeField]
	private PositionInterpolator _interpolator;

	private LevelTime _timer;
	private string _timerName = "currTime";
	private Camera cam;

	private Dictionary<string, Gradient> scenebackground;
	private Gradient _gradient;
	private bool fpsmode;
	private bool alreadyhassky;

	//Enable/disable FPS mode
	public void SetupFPSMode (bool mode)
	{
		fpsmode = mode;
		SetSky();
		if (!mode && !alreadyhassky) { this.cam.backgroundColor = Color.black; }
	}

	//Sets sky for the scene
	private void SetSky ()
	{
		string active_scene = SceneManager.GetActiveScene().name;
		// If there is a sky already present, set alreadyhassky to true
		switch (active_scene)
		{
			case "FancyRuins2":
			case "DreamFireChain":
			case "DreamForce":
			case "DreamAll":
				alreadyhassky = true;
				break;
			default:
				alreadyhassky = false;
				break;
		}

		if (this.cam == null)
		{
			this.cam = base.gameObject.transform.Find("Cameras").Find("Main Camera").GetComponent<Camera>();
		}

		Gradient var;

		if (scenebackground == null) { CreateGradient(); }
		if (!(scenebackground.TryGetValue(SceneManager.GetActiveScene().name, out var)))
		{
			scenebackground.TryGetValue("Black", out var);
		}
		_gradient = var;
	}

	public void SetEnabled (bool enable)
	{
		base.enabled = enable;
	}

	private void Awake ()
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			if (this._behaviours[i] != null)
			{
				this._behaviours[i].SetOwner(this);
			}
		}
		if (this._interpolator == null)
		{
			this._interpolator = base.gameObject.AddComponent<PositionInterpolator>();
		}
		//Configure sky color on awake
		CreateGradient();
		SetSky();
	}

	public void Init (GameObject prefab)
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			this._behaviours[i].Init(prefab);
		}
	}

	public void StartTransition (Vector3 worldFrom, Vector3 worldTo, LevelRoom toRoom = null)
	{
		this.SetRoom(toRoom);
		if (fpsmode) { return; }
		this.SetEnabled(false);
		Vector3 position = base.transform.position;
		Vector3 to;
		if (toRoom != null)
		{
			to = this.GetConstrainedPos(toRoom, worldTo);
		}
		else
		{
			to = worldTo + (position - worldFrom);
		}
		this._interpolator.StartInterpolation(position, to);
	}

	public void FinishTransition ()
	{
		this._interpolator.Interpolate(1f);
		this.SetEnabled(true);
	}

	public void UpdateTransition (float t)
	{
		this._interpolator.Interpolate(t);
	}

	public Vector3 GetConstrainedPos (LevelRoom forRoom, Vector3 target)
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			target = this._behaviours[i].GetConstrainedPos(forRoom, target);
		}
		return target;
	}

	public void SetRoom (LevelRoom room)
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			this._behaviours[i].SetRoom(room);
		}
	}

	private void Update ()
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			this._behaviours[i].DoUpdate();
		}
		if (this.cam == null)
		{
			this.cam = base.gameObject.transform.Find("Cameras").Find("Main Camera").GetComponent<Camera>();
		}
		if (this.cam != null)
		{
			LevelTime timer = this.Timer;
			//If a timer exists, a sky was not already present and fpsmode is activated, change background color
			if (timer != null && !alreadyhassky && fpsmode)
			{
				this.cam.backgroundColor = this._gradient.Evaluate(timer.GetTimePercent(this._timerName));
			}
		}
	}

	private void LateUpdate ()
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			this._behaviours[i].DoLateUpdate();
		}
	}

	private void FixedUpdate ()
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			this._behaviours[i].DoFixedUpdate();
		}
	}

	private LevelTime Timer
	{
		get
		{
			if (this._timer == null)
			{
				this._timer = LevelTime.Instance;
			}
			return this._timer;
		}
	}

	//Gradient dictionary setup
	private void CreateGradient ()
	{
		scenebackground = new Dictionary<string, Gradient>();

		Gradient gradient;
		GradientColorKey[] colorKey;

		// Populate the alpha keys shared by everyone
		GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
		alphaKey[0].alpha = 1.0f;
		alphaKey[0].time = 0.0f;
		alphaKey[1].alpha = 0.0f;
		alphaKey[1].time = 1.0f;

		//FluffyFields
		gradient = new Gradient();
		colorKey = new GradientColorKey[8];
		ColorUtility.TryParseHtmlString("#8DECFF", out colorKey[0].color);
		colorKey[0].time = 0.15f;
		ColorUtility.TryParseHtmlString("#FF8E45", out colorKey[1].color);
		colorKey[1].time = 0.25f;
		ColorUtility.TryParseHtmlString("#A23E8B", out colorKey[2].color);
		colorKey[2].time = 0.335f;
		ColorUtility.TryParseHtmlString("#1F2971", out colorKey[3].color);
		colorKey[3].time = 0.40f;
		ColorUtility.TryParseHtmlString("#1E2772", out colorKey[4].color);
		colorKey[4].time = 0.553f;
		ColorUtility.TryParseHtmlString("#924421", out colorKey[5].color);
		colorKey[5].time = 0.618f;
		ColorUtility.TryParseHtmlString("#F0F245", out colorKey[6].color);
		colorKey[6].time = 0.75f;
		ColorUtility.TryParseHtmlString("#8DECFF", out colorKey[7].color);
		colorKey[7].time = 0.85f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("FluffyFields", gradient);
		scenebackground.Add("Deep3", gradient);
		scenebackground.Add("Deep18", gradient);

		//FancyRuins
		gradient = new Gradient();
		colorKey = new GradientColorKey[8];
		ColorUtility.TryParseHtmlString("#8DECFF", out colorKey[0].color);
		colorKey[0].time = 0.15f;
		ColorUtility.TryParseHtmlString("#FF8E45", out colorKey[1].color);
		colorKey[1].time = 0.25f;
		ColorUtility.TryParseHtmlString("#A23E8B", out colorKey[2].color);
		colorKey[2].time = 0.335f;
		ColorUtility.TryParseHtmlString("#1F2971", out colorKey[3].color);
		colorKey[3].time = 0.40f;
		ColorUtility.TryParseHtmlString("#1E2772", out colorKey[4].color);
		colorKey[4].time = 0.553f;
		ColorUtility.TryParseHtmlString("#924421", out colorKey[5].color);
		colorKey[5].time = 0.618f;
		ColorUtility.TryParseHtmlString("#F0F245", out colorKey[6].color);
		colorKey[6].time = 0.75f;
		ColorUtility.TryParseHtmlString("#8DECFF", out colorKey[7].color);
		colorKey[7].time = 0.85f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("FancyRuins", gradient);
		scenebackground.Add("Deep7", gradient);

		//VitaminHills
		gradient = new Gradient();
		colorKey = new GradientColorKey[8];
		ColorUtility.TryParseHtmlString("#FF8E45", out colorKey[0].color);
		colorKey[0].time = 0.15f;
		ColorUtility.TryParseHtmlString("#FF5845", out colorKey[1].color);
		colorKey[1].time = 0.25f;
		ColorUtility.TryParseHtmlString("#A23E45", out colorKey[2].color);
		colorKey[2].time = 0.335f;
		ColorUtility.TryParseHtmlString("#1F2971", out colorKey[3].color);
		colorKey[3].time = 0.40f;
		ColorUtility.TryParseHtmlString("#1E2772", out colorKey[4].color);
		colorKey[4].time = 0.553f;
		ColorUtility.TryParseHtmlString("#92444E", out colorKey[5].color);
		colorKey[5].time = 0.618f;
		ColorUtility.TryParseHtmlString("#F0F245", out colorKey[6].color);
		colorKey[6].time = 0.75f;
		ColorUtility.TryParseHtmlString("#FF8E45", out colorKey[7].color);
		colorKey[7].time = 0.85f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("VitaminHills", gradient);
		scenebackground.Add("VitaminHills2", gradient);
		scenebackground.Add("VitaminHills3", gradient);
		scenebackground.Add("Deep6", gradient);

		//SlipperySlope
		gradient = new Gradient();
		colorKey = new GradientColorKey[8];
		ColorUtility.TryParseHtmlString("#37321E", out colorKey[0].color);
		colorKey[0].time = 0.15f;
		ColorUtility.TryParseHtmlString("#64321E", out colorKey[1].color);
		colorKey[1].time = 0.25f;
		ColorUtility.TryParseHtmlString("#644B54", out colorKey[2].color);
		colorKey[2].time = 0.335f;
		ColorUtility.TryParseHtmlString("#191C26", out colorKey[3].color);
		colorKey[3].time = 0.40f;
		ColorUtility.TryParseHtmlString("#191C33", out colorKey[4].color);
		colorKey[4].time = 0.553f;
		ColorUtility.TryParseHtmlString("#623421", out colorKey[5].color);
		colorKey[5].time = 0.618f;
		ColorUtility.TryParseHtmlString("#9A9945", out colorKey[6].color);
		colorKey[6].time = 0.75f;
		ColorUtility.TryParseHtmlString("#37321E", out colorKey[7].color);
		colorKey[7].time = 0.85f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("SlipperySlope", gradient);
		scenebackground.Add("Deep1", gradient);

		//CandyCoast
		gradient = new Gradient();
		colorKey = new GradientColorKey[8];
		ColorUtility.TryParseHtmlString("#74BCFF", out colorKey[0].color);
		colorKey[0].time = 0.15f;
		ColorUtility.TryParseHtmlString("#FF9774", out colorKey[1].color);
		colorKey[1].time = 0.25f;
		ColorUtility.TryParseHtmlString("#88247D", out colorKey[2].color);
		colorKey[2].time = 0.335f;
		ColorUtility.TryParseHtmlString("#20224D", out colorKey[3].color);
		colorKey[3].time = 0.40f;
		ColorUtility.TryParseHtmlString("#2A2250", out colorKey[4].color);
		colorKey[4].time = 0.553f;
		ColorUtility.TryParseHtmlString("#9A4229", out colorKey[5].color);
		colorKey[5].time = 0.618f;
		ColorUtility.TryParseHtmlString("#E3BFA3", out colorKey[6].color);
		colorKey[6].time = 0.75f;
		ColorUtility.TryParseHtmlString("#74BCFF", out colorKey[7].color);
		colorKey[7].time = 0.85f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("CandyCoast", gradient);
		scenebackground.Add("Deep4", gradient);
		scenebackground.Add("Deep16", gradient);

		//FrozenCourt
		gradient = new Gradient();
		colorKey = new GradientColorKey[8];
		ColorUtility.TryParseHtmlString("#909090", out colorKey[0].color);
		colorKey[0].time = 0.15f;
		ColorUtility.TryParseHtmlString("#606060", out colorKey[1].color);
		colorKey[1].time = 0.25f;
		ColorUtility.TryParseHtmlString("#2F2F2F", out colorKey[2].color);
		colorKey[2].time = 0.335f;
		ColorUtility.TryParseHtmlString("#151515", out colorKey[3].color);
		colorKey[3].time = 0.40f;
		ColorUtility.TryParseHtmlString("#151515", out colorKey[4].color);
		colorKey[4].time = 0.553f;
		ColorUtility.TryParseHtmlString("#2F2F2F", out colorKey[5].color);
		colorKey[5].time = 0.618f;
		ColorUtility.TryParseHtmlString("#606060", out colorKey[6].color);
		colorKey[6].time = 0.75f;
		ColorUtility.TryParseHtmlString("#909090", out colorKey[7].color);
		colorKey[7].time = 0.85f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("FrozenCourt", gradient);
		scenebackground.Add("Deep9", gradient);

		//StarWoods
		gradient = new Gradient();
		colorKey = new GradientColorKey[2];
		ColorUtility.TryParseHtmlString("#0A032D", out colorKey[0].color);
		colorKey[0].time = 0.00f;
		ColorUtility.TryParseHtmlString("#0A032D", out colorKey[1].color);
		colorKey[1].time = 1.00f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("StarWoods", gradient);
		scenebackground.Add("StarWoods2", gradient);
		scenebackground.Add("Deep15", gradient);

		//LonelyRoad
		gradient = new Gradient();
		colorKey = new GradientColorKey[2];
		ColorUtility.TryParseHtmlString("#050519", out colorKey[0].color);
		colorKey[0].time = 0.00f;
		ColorUtility.TryParseHtmlString("#050519", out colorKey[1].color);
		colorKey[1].time = 1.00f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("LonelyRoad", gradient);
		scenebackground.Add("LonelyRoad2", gradient);
		scenebackground.Add("Deep10", gradient);

		//SandCastle
		gradient = new Gradient();
		colorKey = new GradientColorKey[2];
		ColorUtility.TryParseHtmlString("#191414", out colorKey[0].color);
		colorKey[0].time = 0.00f;
		ColorUtility.TryParseHtmlString("#191414", out colorKey[1].color);
		colorKey[1].time = 1.00f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("SandCastle", gradient);
		scenebackground.Add("Deep8", gradient);

		//ArtExhibit
		gradient = new Gradient();
		colorKey = new GradientColorKey[2];
		ColorUtility.TryParseHtmlString("#140A14", out colorKey[0].color);
		colorKey[0].time = 0.00f;
		ColorUtility.TryParseHtmlString("#140A14", out colorKey[1].color);
		colorKey[1].time = 1.00f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("ArtExhibit", gradient);

		//BoilingGrave
		gradient = new Gradient();
		colorKey = new GradientColorKey[2];
		ColorUtility.TryParseHtmlString("#0A0A0A", out colorKey[0].color);
		colorKey[0].time = 0.00f;
		ColorUtility.TryParseHtmlString("#0A0A0A", out colorKey[1].color);
		colorKey[1].time = 1.00f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("BoilingGrave", gradient);
		scenebackground.Add("Deep14", gradient);

		//PillowFort
		gradient = new Gradient();
		colorKey = new GradientColorKey[2];
		ColorUtility.TryParseHtmlString("#0A0714", out colorKey[0].color);
		colorKey[0].time = 0.00f;
		ColorUtility.TryParseHtmlString("#0A0714", out colorKey[1].color);
		colorKey[1].time = 1.00f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("PillowFort", gradient);

		//FloodedBasement
		gradient = new Gradient();
		colorKey = new GradientColorKey[2];
		ColorUtility.TryParseHtmlString("#00050F", out colorKey[0].color);
		colorKey[0].time = 0.00f;
		ColorUtility.TryParseHtmlString("#00050F", out colorKey[1].color);
		colorKey[1].time = 1.00f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("FloodedBasement", gradient);

		//PotassiumMine
		/*         gradient = new Gradient();
			   colorKey = new GradientColorKey[2];
				ColorUtility.TryParseHtmlString("#191414",out colorKey[0].color);
			   colorKey[0].time = 0.00f;
				ColorUtility.TryParseHtmlString("#191414",out colorKey[1].color);
			   colorKey[1].time = 1.00f;
			   gradient.SetKeys(colorKey, alphaKey);
				scenebackground.Add("PotassiumMine",gradient); */

		//TrashCave
		gradient = new Gradient();
		colorKey = new GradientColorKey[2];
		ColorUtility.TryParseHtmlString("#0A140A", out colorKey[0].color);
		colorKey[0].time = 0.00f;
		ColorUtility.TryParseHtmlString("#0A140A", out colorKey[1].color);
		colorKey[1].time = 1.00f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("TrashCave", gradient);
		scenebackground.Add("Deep5", gradient);

		//GrandLibrary
		gradient = new Gradient();
		colorKey = new GradientColorKey[2];
		ColorUtility.TryParseHtmlString("#0C1217", out colorKey[0].color);
		colorKey[0].time = 0.00f;
		ColorUtility.TryParseHtmlString("#0C1217", out colorKey[1].color);
		colorKey[1].time = 1.00f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("GrandLibrary", gradient);
		scenebackground.Add("GrandLibrary2", gradient);
		scenebackground.Add("Deep12", gradient);

		//SecretDungeons
		gradient = new Gradient();
		colorKey = new GradientColorKey[2];
		ColorUtility.TryParseHtmlString("#0A0A0A", out colorKey[0].color);
		colorKey[0].time = 0.00f;
		ColorUtility.TryParseHtmlString("#0A0A0A", out colorKey[1].color);
		colorKey[1].time = 1.00f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("SunkenLabyrinth", gradient);
		scenebackground.Add("DarkHypostyle", gradient);
		scenebackground.Add("MachineFortress", gradient);
		scenebackground.Add("TombOfSimulacrum", gradient);
		scenebackground.Add("Deep11", gradient);

		//Dreamworld HUB
		gradient = new Gradient();
		colorKey = new GradientColorKey[2];
		ColorUtility.TryParseHtmlString("#fadcff", out colorKey[0].color);
		colorKey[0].time = 0.00f;
		ColorUtility.TryParseHtmlString("#fadcff", out colorKey[1].color);
		colorKey[1].time = 1.00f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("DreamWorld", gradient);

		//Black (default)
		gradient = new Gradient();
		colorKey = new GradientColorKey[2];
		ColorUtility.TryParseHtmlString("#000000", out colorKey[0].color);
		colorKey[0].time = 0.00f;
		ColorUtility.TryParseHtmlString("#000000", out colorKey[1].color);
		colorKey[1].time = 1.00f;
		gradient.SetKeys(colorKey, alphaKey);
		scenebackground.Add("Black", gradient);
	}
}