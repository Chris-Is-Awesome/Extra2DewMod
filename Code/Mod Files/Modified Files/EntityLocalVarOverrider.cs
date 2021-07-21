using System;
using UnityEngine;

public class EntityLocalVarOverrider : MonoBehaviour
{
	[SerializeField]
	EntityLocalVarOverrider.VarData[] _setVars;

	[SerializeField]
	ExprVarHolderBase _exprCtx;

	public void Apply(Entity ent)
	{
		IExprContext ctx;
		if (this._exprCtx != null)
		{
			ctx = this._exprCtx;
		}
		else
		{
			ctx = ent;
		}
		for (int i = 0; i < this._setVars.Length; i++)
		{
			EntityLocalVarOverrider.VarData varData = this._setVars[i];
			int value;
            
            //If there is no stick (melee = 100) keep the value as 100
            if (varData.varName == "melee" && ent.GetStateVariable("melee") == 100)
            {
                value = 100;
            }
            else if (varData.exprValue != null)
            {
                value = varData.exprValue.Evaluate(ctx);
            }
			else
			{
				value = varData.value;
			}
			ent.AddLocalTempVar(varData.varName);
			ent.SetStateVariable(varData.varName, value);
		}
	}

	[Serializable]
	public class VarData
	{
		public string varName;

		public int value;

		public ExpressionHolder exprValue;
	}
}
