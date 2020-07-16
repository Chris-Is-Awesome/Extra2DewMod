using UnityEngine;
using System.Collections.Generic;

namespace ModStuff
{
	public class FindManager : Singleton<FindManager>
	{
		public List<Texture2D> FindAllTextures()
		{
			List<Texture2D> textures = new List<Texture2D>();

			foreach (Texture2D tex in Resources.FindObjectsOfTypeAll<Texture2D>())
			{
				textures.Add(tex);
			}

			return textures;
		}

		public Texture2D FindTextureByName(string name)
		{
			List<Texture2D> textures = FindAllTextures();

			foreach (Texture2D tex in textures)
			{
				if (tex.name.ToLower() == name.ToLower()) { return tex; }
			}

			return null;
		}
	}
}