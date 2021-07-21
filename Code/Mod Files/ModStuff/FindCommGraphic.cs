using UnityEngine;

namespace ModStuff
{
	public class FindCommGraphic : MonoBehaviour
	{
		Transform following;
		Camera camera;
		GUIStyle boxStyle;
		Texture2D tex;
		int blockSize;
		int offset;

		public void Init (GameObject goToFollow, Camera cam, bool selected)
		{
			//Make texture
			tex = new Texture2D(1, 1);
			tex.SetPixel(0, 0, selected ? new Color(0f, 1f, 0f, 0.5f) : new Color(0f, 0f, 1f, 0.5f));
			tex.Apply();
			//Asign variables
			following = goToFollow.transform;
			camera = cam;
			blockSize = selected ? 120 : 100;
			offset = blockSize / 2;
			Destroy(gameObject, 10f);

		}

        public void Init(GameObject goToFollow, Camera cam, bool selected, float time)
        {
            //Make texture
            tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, selected ? new Color(0f, 1f, 0f, 0.5f) : new Color(0f, 0f, 1f, 0.5f));
            tex.Apply();
            //Asign variables
            following = goToFollow.transform;
            camera = cam;
            blockSize = selected ? 120 : 100;
            offset = blockSize / 2;
            Destroy(gameObject, time);

        }

        void OnGUI ()
		{
			if (following != null && camera != null)
			{
				if (boxStyle == null)
				{
					boxStyle = new GUIStyle(GUI.skin.box);
					boxStyle.normal.background = tex;
				}
				Vector3 screenPos = camera.WorldToScreenPoint(following.position);
                if (Vector3.Dot(camera.transform.forward, following.position - camera.transform.position) <= 0) screenPos = new Vector3(-100000f, -100000f, 0f);
                GUI.Box(new Rect(screenPos.x - offset, Screen.height - screenPos.y - offset, blockSize, blockSize), tex, boxStyle);
			}
		}
	}
}