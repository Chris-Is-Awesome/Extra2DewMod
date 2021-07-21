using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace ModStuff
{
    public class CameraSkyColor : Singleton<CameraSkyColor>
    {
        Dictionary<string, Gradient> scenebackground;
        List<string> scenesWithSky;

        public bool UseForceColor { get; set; }
        public Color ForceColor
        {
            get
            {
                Color sky = new Color();
                sky.r = red;
                sky.b = blue;
                sky.g = green;

                return sky;
            }
            set
            {
                red = value.r;
                blue = value.b;
                green = value.g;
            }
        }
        public float red;
        public float blue;
        public float green;

        void Awake()
        {
            red = 1f;
            blue = 1f;
            green = 1f;
            CreateGradient();
        }

        //Sets sky for the scene
        public Gradient SetSky(out bool alreadyhassky)
        {
            if (scenebackground == null) { CreateGradient(); }
            string active_scene = SceneManager.GetActiveScene().name;
            alreadyhassky = scenesWithSky.Contains(active_scene);

            if (!(scenebackground.TryGetValue(active_scene, out Gradient var)))
            {
                scenebackground.TryGetValue("Black", out var);
            }
            return var;
        }

        //Gradient and ScenesWithSky dictionaries setup
        void CreateGradient()
        {
            scenebackground = new Dictionary<string, Gradient>();
            scenesWithSky = new List<string>() { "FancyRuins2", "DreamFireChain", "DreamForce", "DreamAll" };

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
}