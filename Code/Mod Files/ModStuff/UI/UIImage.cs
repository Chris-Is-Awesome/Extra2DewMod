using UnityEngine;
using System.Collections;
using System.IO;

namespace ModStuff
{
    public class UIImage : UIElement
    {
        MeshRenderer _renderer;
        Material _material;

        public MeshRenderer MeshRenderer { get { return _renderer; } }
        public Vector3 ImageScale { get; set; } = Vector3.one;
        public Material Material { get { return _material; } }

        public void ChangeTexture(string path)
        {
            //Load image texture and apply as material
            //string path = ModMaster.TexturesPath + pngName + ".png";
            Texture2D tex = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            byte[] fileData;
            if (File.Exists(path))
            {
                fileData = File.ReadAllBytes(path);
                tex.LoadImage(fileData, false);
            }
            ApplyTexture(tex);
        }

        public void ApplyTexture(Texture2D tex)
        {
            if (tex == null || tex.height == 0 || tex.width == 0) return;
            _renderer.material.mainTexture = tex;
            float width = tex.width;
            float height = tex.height;

            if (width > height)
            {
                transform.localScale = new Vector3(ImageScale.x, ImageScale.y * height / width, 0f);
            }
            else
            {
                transform.localScale = new Vector3(ImageScale.x * width / height, ImageScale.y, 0f);
            }
        }

        public void Initialize()
        {
            //Set TextMesh
            nameTextMesh = gameObject.GetComponentInChildren<TextMesh>();
            //Set mesh and material
            _renderer = transform.Find("ImgTexture").GetComponent<MeshRenderer>();
            _material = _renderer.material;
        }
    }
}
