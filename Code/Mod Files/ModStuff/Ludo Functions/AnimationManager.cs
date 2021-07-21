using System;
using UnityEngine;

namespace ModStuff.LudoFunctions
{
	public class AnimationManager : Singleton<AnimationManager>
	{
		// Plays animation on specified object
		public void PlayAnimation(GameObject objToAnimate, string animName, WrapMode loopType, float speed = 0f)
		{
			EntityAnimator entAnimator = ModMaster.GetEntComp<EntityAnimator>(objToAnimate.name);

			if (entAnimator != null)
			{
				EntityAnimator.AnimationData animData = entAnimator.GetAnimData(animName);
				if (speed > 0)	animData._speed = speed;

				entAnimator.PlayAnim(animName, 0).realState.wrapMode = loopType;
			}
		}
	}
}