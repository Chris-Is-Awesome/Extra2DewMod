using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using ModStuff;

[AddComponentMenu("Ittle 2/Actions/Set save variable action")]
public class GA_SetSaveVariable : GameAction
{
	[SerializeField]
	SaverOwner _saver;

	[SerializeField]
	string _varPath;

	[SerializeField]
	string _value;

	[SerializeField]
	GA_SetSaveVariable.ValueMode _valueMode = GA_SetSaveVariable.ValueMode.Int;

	[SerializeField]
	ExpressionHolder _expr;

	[SerializeField]
	ExprVarHolderBase _exprContext;

	[SerializeField]
	string[] _exprIndexValues;

	Dictionary<string, string> dungeonNames = new Dictionary<string, string>()
	{
		{ "dungeons/dungeon1", "PillowFort" },
		{ "dungeons/dungeon2", "SandCastle" },
		{ "dungeons/dungeon3", "ArtExhibit" },
		{ "dungeons/dungeon4", "TrashCave" },
		{ "dungeons/dungeon5", "FloodedBasement" },
		{ "dungeons/dungeon6", "PotassiumMine" },
		{ "dungeons/dungeon7", "BoilingGrave" },
		{ "dungeons/dungeon8", "GrandLibrary" },
		{ "dungeons/secdun1", "SunkenLabyrinth" },
		{ "dungeons/secdun2", "MachineFortress" },
		{ "dungeons/secdun3", "DarkHypostyle" },
		{ "dungeons/secdun4", "TombOfSimulacrum" },
		{ "dream/dungeon1", "WizardryLab" },
		{ "dream/dungeon2", "Syncope" },
		{ "dream/dungeon3", "BottomlessTower" },
		{ "dream/dungeon4", "Antigram" },
		{ "dream/dungeon5", "Quietus" }
	};

	void DoSave(IDataSaver saver, string name, string value)
	{
		if (this._valueMode == GA_SetSaveVariable.ValueMode.Int)
		{
			saver.SaveInt(name, int.Parse(value));
		}
		else if (this._valueMode == GA_SetSaveVariable.ValueMode.Float)
		{
			saver.SaveFloat(name, float.Parse(value, CultureInfo.InvariantCulture));
		}
		else if (this._valueMode == GA_SetSaveVariable.ValueMode.Bool)
		{
			saver.SaveBool(name, bool.Parse(value));
		}
		else
		{
			saver.SaveData(name, value);
		}
	}

	void DoSave(IDataSaver saver, string name, IExprContext ctx)
	{
		if (this._expr != null && ctx != null)
		{
			int num = this._expr.Evaluate(ctx);
			if (this._exprIndexValues == null || this._exprIndexValues.Length == 0)
			{
				saver.SaveInt(name, num);
			}
			else
			{
				int num2 = Mathf.Clamp(num, 0, this._exprIndexValues.Length - 1);
				this.DoSave(saver, name, this._exprIndexValues[num2]);
			}
		}
		else
		{
			this.DoSave(saver, name, this._value);
		}
	}

	protected override void DoExecute(GameAction.ActionData data)
	{
		// Update dunegon progress
		if (_varPath.Contains("/local/dungeons/dungeon") || _varPath.Contains("/local/dungeons/secdun") || _varPath.Contains("/local/dream/dungeon"))
		{
			if (dungeonNames.TryGetValue(_varPath.Substring(7), out string dunName))
			{
				ModSaver.SaveBoolToFile("mod/GameState", "HasCompletedDungeon", true);
				ModSaver.SaveStringToFile("mod/GameState", "CompletedDungeonName", dunName);
				GameState.Instance.OnDungeonComplete();

				// Invoke OnDungeonComplete events
				GameStateNew.OnDungeonCompleted(SceneManager.GetActiveScene());
			}
		}

		string name;
		IDataSaver saverAndName = this._saver.GetSaverAndName(this._varPath, out name, false);
		IExprContext exprContext = this._exprContext;
		if (exprContext == null)
		{
			exprContext = (data.owner as Entity);
		}
		this.DoSave(saverAndName, name, exprContext);
	}

	public enum ValueMode
	{
		String,
		Int,
		Float,
		Bool
	}
}
