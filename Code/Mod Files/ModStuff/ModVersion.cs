using System;
using UnityEngine;

namespace ModStuff
{
	static public class ModVersion
	{
		static public string GetModVersion ()
		{
			return "v2.0-1.1b";
		}

		static public bool IsDevBuild ()
		{
			return GetModVersion().Contains("a") || GetModVersion().Contains("b") || Application.dataPath.Contains("D:/Games/Steam");
		}
	}
}