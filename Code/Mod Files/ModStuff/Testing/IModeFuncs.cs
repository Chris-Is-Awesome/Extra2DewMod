using UnityEngine;
using System.Collections;

namespace ModStuff
{
	public interface IModeFuncs
	{
		void Initialize(bool activate, RealDataSaver saver = null, int difficulty = -1);
		void Activate();
		void Deactivate();
	}
}