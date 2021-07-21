using System;
using UnityEngine;

namespace ModStuff
{
	static public class ModVersion
	{
		static public string GetModVersion ()
		{
			return "v3.0";
		}

		static public bool IsDevBuild ()
		{
			return GetModVersion().Contains("a") || GetModVersion().Contains("b") || Application.dataPath.Contains("D:/Games/Steam");
		}
	}
}