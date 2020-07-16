using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff
{
    public class MenuMapNode
    {
        public struct Door
        {
            public string _screenName;
            public string _doorName;
            public string _optionalScene;

            public Door(string name, string door)
            {
                _screenName = name;
                _doorName = door;
                _optionalScene = null;
            }

            public Door(string name, string door, string scene)
            {
                _screenName = name;
                _doorName = door;
                _optionalScene = scene;
            }
        }

        public enum MapType { Overworld, Dungeon, /*Cave,*/ DeepCave, Special };
        static string[] _mapTypeNames;
        public static string[] MapTypeNames
        {
            get
            {
                if(_mapTypeNames == null)
                {
                    _mapTypeNames = new string[]
                    {
                        "Overworld",
                        "Dungeons",
                        /*"Caves",*/
                        "Portal worlds",
                        "Special"
                    };
                }
                return _mapTypeNames;
            }
        }

        public string Name { get; private set;}
        public string Scene { get; private set; }
        public MapType Type { get; private set; }
        public Door[] Doors { get; private set; }

        public MenuMapNode(string screenName, string sceneName, MapType type, Door[] sceneDoors)
        {
            Name = screenName;
            Scene = sceneName;
            Type = type;
            Doors = sceneDoors;
        }

        public MenuMapNode(string screenName, string sceneName, MapType type, string[] sceneDoors)
        {
            Name = screenName;
            Scene = sceneName;
            Type = type;
            List<Door> doorList = new List<Door>();
            foreach(string door in sceneDoors)
            {
                doorList.Add(new Door("", door));
            }
            Doors = doorList.ToArray();
        }

        //Return door list
        public string[] DoorsList()
        {
            List<string> output = new List<string>();
            foreach(Door door in Doors)
            {
                output.Add(door._screenName);
            }

            return output.ToArray();
        }

        //Static list with all the nodes
        static MenuMapNode[] allNodes;
        static MenuMapNode[] AllNodes
        {
            get
            {
                if (allNodes == null) { BuildNodes(); }
                return allNodes;
            }
        }

        //Get door list of the node
        public string[] GetDoors()
        {
            List<string> output = new List<string>();
            foreach(Door door in Doors)
            {
                output.Add(door._screenName);
            }
            return output.ToArray();
        }
        public Door FindDoor(string screenName)
        {
            foreach(Door door in Doors)
            {
                if (door._screenName == screenName) return door;
            }

            return new Door(null,null);
        }

        //Gets a list of all the nodes names
        static public string[] GatherNodesByType(MapType type)
        {
            List<string> output = new List<string>();

            foreach(MenuMapNode node in AllNodes)
            {
                if(node.Type == type)
                {
                    output.Add(node.Name);
                }
            }

            return output.ToArray();
        }

        //Find node by name
        static public MenuMapNode FindByName(string name)
        {
            foreach (MenuMapNode node in AllNodes)
            {
                if (node.Name == name)
                {
                    return node;
                }
            }
            return null;
        }

        // Returns SaverOwner
        public static SaverOwner GetSaver()
        {
            if (ModMaster.GetMapName() == "MainMenu")
            {
                return GameObject.Find("GuiFuncs").GetComponent<MainMenu>()._saver;
            }

            if (GameObject.Find("BaseLevelData") != null)
            {
                return GameObject.Find("BaseLevelData").GetComponent<LevelTime>()._saver;
            }

            return null;
        }

        //Warp
        public void Warp(string doorName)
        {
            FadeEffectData gotoTransition = null;
            foreach (SceneDoor sceneDoor in Resources.FindObjectsOfTypeAll<SceneDoor>())
            {
                if (sceneDoor._fadeData != null)
                {
                    gotoTransition = sceneDoor._fadeData;
                }
            }

            Door door = FindDoor(doorName);
            string scene = door._optionalScene == null ? Scene : door._optionalScene;

            // Start load
            if (gotoTransition != null)
            {
                SceneDoor.StartLoad(scene, door._doorName, gotoTransition, GetSaver(), null);
            }
        }


        static void BuildNodes()
        {
            List<MenuMapNode> temp = new List<MenuMapNode>();
            //--------------------
            //Overworld
            //--------------------
            temp.Add(new MenuMapNode
            (
                "Fluffy Fields", "FluffyFields", MapType.Overworld,
                new Door[]
                {
                    new Door("Pond","FluffyFields"),
                    new Door("Village","CaveS"),
                    new Door("Dungeon entrance","PillowFortOutside"),
                    new Door("West Side","FF_SS1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Sweetwater Coast", "CandyCoast", MapType.Overworld,
                new Door[]
                {
                    new Door("North entrance","CC_FF1"),
                    new Door("South side","CaveN"),
                    new Door("Dungeon entrance","SandCastleOutside"),
                    new Door("Changing tent","CaveQ")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Fancy Ruins", "FancyRuins", MapType.Overworld,
                new Door[]
                {
                    new Door("East entrance","FR_FF1"),
                    new Door("South entrance","FR_SW1"),
                    new Door("Maze","FR_FC1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Fancy Hilltop", "FancyRuins2", MapType.Overworld,
                new Door[]
                {
                    new Door("Dungeon entrance","ArtExhibitOutside")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Star Woods", "StarWoods", MapType.Overworld,
                new Door[]
                {
                    new Door("North entrance","SW_FF4"),
                    new Door("West entrance","SW_FF1"),
                    new Door("Dungeon entrance","TrashCaveOutside"),
                    new Door("Eastern island","CaveJ"),
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Star Coast", "StarWoods2", MapType.Overworld,
                new Door[]
                {
                    new Door("Central entrance","CaveT1"),
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Slippery Slope", "SlipperySlope", MapType.Overworld,
                new Door[]
                {
                    new Door("South entrance","SS_FF1"),
                    new Door("North entrance","SS_LR1"),
                    new Door("Dungeon entrance","CaveO"),
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Frozen Court", "FrozenCourt", MapType.Overworld,
                new Door[]
                {
                    new Door("South entrance","SS_FF1"),
                    new Door("North entrance","FC_FR1"),
                    new Door("Dungeon entrance","BoilingGraveOutside")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Pepperpain Prairie", "VitaminHills", MapType.Overworld,
                new Door[]
                {
                    new Door("West entrance","VH_SS1"),
                    new Door("East entrance","VH_FR1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Pepperpain Trail", "VitaminHills2", MapType.Overworld,
                new Door[]
                {
                    new Door("West entrance","CaveC2"),
                    new Door("East entrance","CaveR1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Pepperpain Mountain", "VitaminHills3", MapType.Overworld,
                new Door[]
                {
                    new Door("West entrance","CaveR2"),
                    new Door("Dungeon entrance","PotassiumMineOutside")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Lonely Road", "LonelyRoad", MapType.Overworld,
                new Door[]
                {
                    new Door("Main entrance","LR_SS1"),
                    new Door("Lake","CaveS"),
                    new Door("Dungeon entrance","GrandLibraryOutside")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Forbidden Area", "LonelyRoad2", MapType.Overworld,
                new Door[]
                {
                    new Door("Main entrance","CaveJ2"),
                    new Door("Dungeon entrance","TombOfSimulacrumOutside")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Dream World", "DreamWorld", MapType.Overworld,
                new Door[]
                {
                    new Door("Entrance","DreamWorldInside")
                }
            ));

            //--------------------
            //Dungeons
            //--------------------
            temp.Add(new MenuMapNode
            (
                "Pillow Fort", "PillowFort", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","PillowFortInside"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Sand Castle", "SandCastle", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","SandCastleInside"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Art Exhibit", "ArtExhibit", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","ArtExhibitInside"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Trash Cave", "TrashCave", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","TrashCaveInside"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Flooded Basement", "FloodedBasement", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","FloodedBasementInside"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Potassium Mine", "PotassiumMine", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","PotassiumMineInside"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Boiling Grave", "BoilingGrave", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","BoilingGraveInside"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Grand Library", "GrandLibrary", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","GrandLibraryInside")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Shifting Chamber", "GrandLibrary2", MapType.Dungeon,
                new Door[]
                {
                    new Door("Boss","GrandLibrary2")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Sunken Labyrinth", "SunkenLabyrinth", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","SunkenLabyrinthInside"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Machine Fortress", "MachineFortress", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","MachineFortressInside"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Dark Hypostyle", "DarkHypostyle", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","DarkHypostyleInside"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Tomb of Simulacrum", "TombOfSimulacrum", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","TombOfSimulacrumInside"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Wizardry Lab", "DreamForce", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","DreamForceInside")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Syncope", "DreamDynamite", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","DreamDynamiteInside"),
                    new Door("Mid point","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Antigram", "DreamFireChain", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","DreamFireChainInside"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Bottomless Tower", "DreamIce", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","DreamIceInside"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Quietus", "DreamAll", MapType.Dungeon,
                new Door[]
                {
                    new Door("Entrance","DreamAllInside"),
                    new Door("Boss","RestorePt2")
                }
            ));

            //--------------------
            //Deep caves
            //--------------------
            temp.Add(new MenuMapNode
            (
                "Autumn Climb", "Deep1", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "The Vault", "Deep2", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep2")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Painful Plain", "Deep3", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep3")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Farthest Shore", "Deep4", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep4")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Scrap Yard", "Deep5", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep5")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Brutal Oasis", "Deep6", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep6")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Former Colossus", "Deep7", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep7")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Sand Crucible", "Deep8", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep8")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Ocean Castle", "Deep9", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep9")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Promenade Path", "Deep10", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep10")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Maze of Steel", "Deep11", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep11")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Wall of Text", "Deep12", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep12")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Lost City of Avlopp", "Deep13", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep13")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Northern End", "Deep14", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep14")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Moon Garden", "Deep15", MapType.DeepCave,
                new Door[]
                {
                    new Door("Entrance","Deep15")
                }
            ));

            //--------------------
            //Special
            //--------------------
            temp.Add(new MenuMapNode
            (
                "Test Room", "Deep19", MapType.Special,
                new Door[]
                {
                    new Door("Entrance","Deep19")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Changing Tent", "CandyCoastCaves", MapType.Special,
                new Door[]
                {
                    new Door("Entrance","CaveQ")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Nowhere", "Deep16", MapType.Special,
                new Door[]
                {
                    new Door("Entrance","Deep16")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Cave of Mystery", "Deep17", MapType.Special,
                new Door[]
                {
                    new Door("Entrance","Deep17")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Somewhere", "Deep18", MapType.Special,
                new Door[]
                {
                    new Door("Entrance","Deep18")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Ludo City", "Deep20", MapType.Special,
                new Door[]
                {
                    new Door("Entrance","Deep20")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "The Promised Remedy", "Deep19s", MapType.Special,
                new Door[]
                {
                    new Door("Entrance","Deep19s"),
                    new Door("Boss","RestorePt1")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Abyssal Plain", "Deep21", MapType.Special,
                new Door[]
                {
                    new Door("Entrance","Deep21")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Abandoned house", "Deep23", MapType.Special,
                new Door[]
                {
                    new Door("Entrance","Deep23")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "House of Secrets", "Deep24", MapType.Special,
                new Door[]
                {
                    new Door("Entrance","Deep24")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Dark room", "Deep25", MapType.Special,
                new Door[]
                {
                    new Door("Entrance","Deep25")
                }
            ));
            temp.Add(new MenuMapNode
            (
                "Bad Dream", "Deep26", MapType.Special,
                new Door[]
                {
                    new Door("Entrance","Deep26")
                }
            ));
            allNodes = temp.ToArray();
        }
    }
}
