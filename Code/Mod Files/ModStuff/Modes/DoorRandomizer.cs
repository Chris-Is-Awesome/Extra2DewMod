using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

//Modified ludo scripts:
//SceneDoor.cs: Calls GetRandomizedDoor to get a randomized door in IBC_TriggerEnterListener
//MainMenu.cs: Calls RandomizeWithSeed with the flag true, to create the cheat sheet txt

namespace ModStuff
{
    public class DoorsRandomizer : Singleton<DoorsRandomizer>
    {
        class RoomNode
        {
            public struct Link
            {
                public string scene;
                public string door;

                public Link(string s, string d)
                {
                        scene = s;
                        door = d;
                }
            }

            public string Scene { get { return scene; } }

            string scene;
            string[] doors;
            List<string> freeDoors;
            public Dictionary<string, Link> connections;

            public int availableDoors
            {
                get { return freeDoors.Count; }
            }

            public RoomNode(string sceneName, string[] doorNames)
            {
                scene = sceneName;
                doors = doorNames;

                connections = new Dictionary<string, Link>();
                freeDoors = new List<string>();
                for (int i = 0; i < doorNames.Length; i++)
                {
                        if (doorNames[i] == null)
                        {
                            freeDoors = new List<string>();
                            return;
                        }
                        freeDoors.Add(doorNames[i]);
                }
            }

            public RoomNode CopyRoom()
            {
                return new RoomNode(scene, doors);
            }

            string GetFreeDoor()
            {
                if (availableDoors == 0)
                { return null; }
                else
                {
                        string output = freeDoors[UnityEngine.Random.Range(0, freeDoors.Count)];
                        RemoveFreeDoor(output);
                        return output;
                }
            }

            bool RemoveFreeDoor(string doorName)
            {
                return freeDoors.Remove(doorName);
            }

            void MakeConnection(string departureDoor, string destinationScene, string destinationDoor)
            {
                //if (connections.TryGetValue(Scene + "/" + departureDoor, out Link linkwhy))
                //{ GUIUtility.systemCopyBuffer = Scene + "/" + departureDoor; }
                connections.Add(Scene + "/" + departureDoor, new Link(destinationScene, destinationDoor));
            }

            public static void ConnectNodes(RoomNode node1, RoomNode node2)
            {
                if (node1 != node2)
                {
                        string door1 = node1.GetFreeDoor();
                        string door2 = node2.GetFreeDoor();
                        node1.MakeConnection(door1, node2.Scene, door2);
                        node2.MakeConnection(door2, node1.Scene, door1);
                }
                else if (node1.availableDoors > 1)
                {
                        string door1 = node1.GetFreeDoor();
                        string door2 = node1.GetFreeDoor();
                        node1.MakeConnection(door1, node1.Scene, door2);
                        node1.MakeConnection(door2, node1.Scene, door1);
                }
                else if (node1.availableDoors == 1)
                {
                        string door1 = node1.GetFreeDoor();
                        node1.MakeConnection(door1, node1.Scene, door1);
                }
            }

            public bool GetDestination(string departureDoor, out string sceneName, out string doorName)
            {
                sceneName = null;
                doorName = null;
                if (connections.TryGetValue(departureDoor, out Link output))
                {
                        sceneName = output.scene;
                        doorName = output.door;
                        return true;
                }
                return false;
            }

            public override string ToString()
            {
                string output = "Scene: " + scene + "\n----------\n";
                if (availableDoors != 0)
                {
                        output += "Free doors: ";
                        for (int i = 0; i < freeDoors.Count; i++) { output += freeDoors[i] + " "; }
                        output += "\n";
                }

                output += "Door links:\n";
                if (connections.Count == 0) { output += "-"; }
                else
                {
                        int i = 0;
                        int lineMax = 3;
                        foreach (KeyValuePair<string, Link> entry in connections)
                        {
                            if (i >= lineMax)
                            {
                                output += "\n";
                                i = 0;
                            }
                            output += entry.Key + " -> " + entry.Value.scene + "/" + entry.Value.door + "\t";
                            i++;
                        }
                }
                output += "\n";
                return output;
            }

            public string GetDoorLinkedToItself()
            {
                string output = "";
                foreach (KeyValuePair<string, Link> entry in connections)
                {
                        if (entry.Key == (entry.Value.scene + "/" + entry.Value.door))
                        {
                            output += entry.Key + " -> " + entry.Key;
                        }
                }
                if (output == "") { output = null; }
                return output;
            }
         }

        //SCENE DOORS RANDOMIZER
        //-----------------------
        //Randomize connections between room nodes (scenes where a player can walk and reach doors without changing scenes)
        //-----------------------
        //Take all the nodes and divide them in two groups: nodes with a single free door and nodes with multiples free doors
        //If there is a room with a single door, try to pair it with a room with multiple doors, if that multiple door room
        //becomes a single door room after being linked, move it to the single door group. Repeat until only 2 single door rooms
        //remain and link them together
        //When rooms are linked, a door in each one stops being free
        //-----------------------
        //Exceptions:
        //If there are no single door rooms and only multiple door rooms, make links between them and move them to the single
        //door group when necessary. If only 1 multiple door room remain with 2 or more doors, make a link with itself.
        //If, after using all multiple door rooms, there is an odd amount of doors in the single door group, one of them will
        //be linked to itself

        public string roomRandomizerLog;
        RoomNode[] RandomizeRoomNodes(RoomNode[] rooms)
        {
            List<RoomNode> singleLink = new List<RoomNode>();
            List<RoomNode> multipleLinks = new List<RoomNode>();
            List<RoomNode> output = new List<RoomNode>();
            bool iterating = true;
            roomRandomizerLog = "";
            int totalDoorCount = 0;
            int singleDoorRooms = 0;
            int branchSplits = 2;
            int totalRoomCount = 0;
            //Fill groups
            for (int i = 0; i < rooms.Length; i++)
            {
                if (rooms[i].availableDoors == 1)
                {
                        singleLink.Add(rooms[i].CopyRoom());
                        singleDoorRooms++;
                }
                else if (rooms[i].availableDoors > 1)
                {
                        multipleLinks.Add(rooms[i].CopyRoom());
                        branchSplits += rooms[i].availableDoors - 2;
                }
                else
                {
                        roomRandomizerLog += "Node in " + rooms[i].Scene + " doesn't contain any doors\n";
                }
                totalDoorCount += rooms[i].availableDoors;
                totalRoomCount++;
            }
            roomRandomizerLog += "Randomized rooms: " + totalRoomCount + "\n";
            roomRandomizerLog += "Single door rooms: " + singleDoorRooms + "\n";
            roomRandomizerLog += "Total number of doors: " + totalDoorCount + "\n";
            if (totalDoorCount % 2 == 1) { roomRandomizerLog += "There is an odd number of doors! One door will be linked to itself!\n"; }
            if (singleDoorRooms > branchSplits)
            {
                roomRandomizerLog += "Not enough branch splits to find a solution!\n";
                return null;
            }
            RoomNode room1;
            RoomNode room2;
            while (iterating)
            {
                //Single door rooms available
                if (singleLink.Count > 0)
                {
                        int i = UnityEngine.Random.Range(0, singleLink.Count);
                        room1 = singleLink[i];
                        singleLink.RemoveAt(i);
                        output.Add(room1);
                        //Multiple door rooms available
                        if (multipleLinks.Count > 0)
                        {
                            i = UnityEngine.Random.Range(0, multipleLinks.Count);
                            room2 = multipleLinks[i];
                            RoomNode.ConnectNodes(room1, room2);
                            if (room2.availableDoors == 1)
                            {
                                multipleLinks.RemoveAt(i);
                                singleLink.Add(room2);
                            }
                        }
                        //Other single door rooms available, but no multiple door rooms
                        else if (singleLink.Count > 0)
                        {
                            i = UnityEngine.Random.Range(0, singleLink.Count);
                            room2 = singleLink[i];
                            singleLink.RemoveAt(i);
                            output.Add(room2);
                            RoomNode.ConnectNodes(room1, room2);
                        }
                        //Only one door remaining
                        else
                        {
                            RoomNode.ConnectNodes(room1, room1);
                            roomRandomizerLog += "Door linked to itself!\n" + room1.GetDoorLinkedToItself();
                        }
                }
                //Multiple door rooms available, but no single ones
                else if (multipleLinks.Count > 1)
                {
                        int i = UnityEngine.Random.Range(0, multipleLinks.Count);
                        room1 = multipleLinks[i];
                        multipleLinks.RemoveAt(i); //Remove it to make it non-eligible for a new random pick
                        i = UnityEngine.Random.Range(0, multipleLinks.Count);
                        room2 = multipleLinks[i];
                        RoomNode.ConnectNodes(room1, room2);
                        //Check room1
                        if (room1.availableDoors > 1) { multipleLinks.Add(room1); } else { singleLink.Add(room1); }
                        //Check room2
                        if (room2.availableDoors == 1)
                        {
                            multipleLinks.RemoveAt(i);
                            singleLink.Add(room2);
                        }
                }
                //Only one multiple door room available
                else if (multipleLinks.Count == 1)
                {
                        room1 = multipleLinks[0];
                        RoomNode.ConnectNodes(room1, room1);
                        //Check if it needs to be moved
                        if (room1.availableDoors < 2)
                        {
                            multipleLinks.RemoveAt(0);
                            if (room1.availableDoors == 1)
                            {
                                singleLink.Add(room1);
                            }
                            else
                            {
                                output.Add(room1);
                            }
                        }
                }
                //If there are no rooms left, stop iterating
                else { iterating = false; }
            }

            return output.ToArray();
        }

        //Populate reference room array
        static RoomNode[] vanillaRooms;
        void BuildVanillaRooms()
        {
            vanillaRooms = new RoomNode[]
            {
            new RoomNode("FluffyFields", new string[] {"CaveN", "FF_SW3", "CaveQ", "CaveH", "CaveC", "CaveB", "FF_SS1", "CaveL", "FF_SW1", "CaveA", "CaveU" , "PillowFortOutside", "CaveE", "CaveS", "CaveO", "CaveM", "CaveX", "FF_VH1", "CaveR", "CaveD", "FF_FR4", "CaveJ", "FF_FR2", "FF_FR1", "FF_SW4", "CaveP", "CaveF", "CaveG", "CaveK", "CaveH2", "FF_CC1", "CaveY", "CaveW", "CaveI", "CaveS2"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveA", "Deep1"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveB"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveC"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveD"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveE"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveF"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveG"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveH", "CaveH2"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveI"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveJ"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveK"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveL"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveM"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveN"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveO"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveP"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveQ"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveR"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveS"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveU"}), //JennyBerry's house
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveW"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveX"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveY"}),
            new RoomNode("FluffyFieldsCaves", new string[] {"CaveS2"}),
            new RoomNode("PillowFort", new string[] {"PillowFortInside"}),
            new RoomNode("StarWoods", new string[] {"CaveG", "CaveI", "CaveR1", "SW_CC1", "SW_FF4", "CaveS1", "SW_FF3", "TrashCaveOutside", "CaveA", "CaveF", "CaveE", "CaveC", "SW_FR1", "CaveB", "CaveN", "SW_CC2", "CaveD", "CaveH", "CaveQ1", "CaveK", "SW_FF1"}),
            new RoomNode("StarWoods", new string[] {"CaveR2", "CaveQ2", "CaveP", "CaveJ"}),
            new RoomNode("StarWoods2", new string[] {"CaveL", "CaveM", "CaveT1", "CaveS2"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveA"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveB"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveC"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveD"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveE"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveF"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveG"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveH"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveI"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveJ"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveK"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveL"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveM"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveT2", "CaveT1"}),
            new RoomNode("StarWoodsCaves", new string[] {"Deep6", "CaveN"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveP", "Deep7"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveQ2", "CaveQ1"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveR1", "CaveR2"}),
            new RoomNode("StarWoodsCaves", new string[] {"CaveS1", "CaveS2"}),
            new RoomNode("TrashCave", new string[] {"TrashCaveInside"}),
            new RoomNode("FancyRuins", new string[] {"CaveD", "FR_FC1", "CaveA", "FR_SW1", "CaveC", "CaveQ1", "CaveR", "CaveJ", "CaveF", "CaveG", "CaveH", "FR_VH1", "CaveK", "CaveL", "CaveP2", "FR_FF1", "FR_FF4", "CaveB", "CaveI", "CaveM", "FR_FF2"}),
            new RoomNode("FancyRuins2", new string[] {"CaveQ2", "ArtExhibitOutside"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveA"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveB"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveC"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveD"}),
            new RoomNode("FancyRuinsCaves", new string[] {"Deep4", "CaveF"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveG"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveH"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveI"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveJ"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveK"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveL"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveM"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveN2"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveQ1", "CaveQ2"}),
            new RoomNode("FancyRuinsCaves", new string[] {"CaveR", "Deep5"}),
            new RoomNode("ArtExhibit", new string[] {"ArtExhibitInside"}),
            new RoomNode("CandyCoast", new string[] {"CaveB", "CaveF2", "CaveI", "CaveQ", "CaveH", "CaveA", "CaveN", "CaveK", "CC_SW2", "CaveC", "CaveJ", "CaveP1", "CaveL", "CC_SS1", "CC_SW1", "CaveM", "CC_FF1", "CaveG", "CaveO", "SandCastleOutside"}),
            new RoomNode("CandyCoast", new string[] {"CaveD1", "CaveE"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveA"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveB"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveC"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveD1"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveE"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveF2"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveG"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveH"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveI"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveJ"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveK"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveL"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveM"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveN"}),
            new RoomNode("CandyCoastCaves", new string[] {"Deep3"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveO"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveP1", "CaveP2"}),
            new RoomNode("CandyCoastCaves", new string[] {"CaveQ"}),
            new RoomNode("CandyCoastCaves", new string[] {"Deep2"}),
            new RoomNode("SandCastle", new string[] {"SandCastleInside"}),
            new RoomNode("FrozenCourt", new string[] {"CaveN2", "FC_FR1", "CaveC", "CaveB", "BoilingGraveOutside", "CaveG1", "CaveJ", "CaveA", "CaveH", "CaveF", "CaveG2", "CaveM1", "CaveI", "CaveL", "CaveD", "CaveK", "CaveE", "CaveT2"}),
            new RoomNode("FrozenCourt", new string[] {"CaveO", "CaveM2"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveA"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveB"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveC"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveD", "Deep12"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveE"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveF"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveI"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveG1", "CaveG2"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveH"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveJ"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveK"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveL"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveM1", "CaveM2"}),
            new RoomNode("FrozenCourtCaves", new string[] {"CaveO", "Deep13"}),
            new RoomNode("BoilingGrave", new string[] {"BoilingGraveInside"}),
            new RoomNode("VitaminHills", new string[] {"CaveK", "CaveE", "CaveH", "CaveN3", "CaveF", "CaveC1", "CaveN2", "CaveJ", "CaveD", "CaveG", "CaveM", "VH_SS1", "CaveB", "VH_FR1", "CaveL", "VH_FF1", "CaveA", "CaveI"}),
            new RoomNode("VitaminHills2", new string[] {"CaveU", "CaveC2", "CaveR1"}),
            new RoomNode("VitaminHills3", new string[] {"PotassiumMineOutside", "CaveQ", "CaveT", "CaveS", "CaveP", "CaveR2", "CaveO"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveA"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveB"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveC2", "CaveC1"}),
            new RoomNode("VitaminHillsCaves", new string[] {"Deep10", "CaveD"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveE"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveF"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveG"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveH"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveI"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveJ"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveK"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveL"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveM"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveO"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveP"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveQ"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveR2", "CaveR1"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveS"}),
            new RoomNode("VitaminHillsCaves", new string[] {"CaveU"}),
            new RoomNode("VitaminHillsCaves", new string[] {"Deep11", "CaveT"}),
            new RoomNode("PotassiumMine", new string[] {"PotassiumMineInside"}),
            new RoomNode("SlipperySlope", new string[] {"SS_CC1", "CaveL", "CaveK", "CaveI", "CaveG", "SS_VH1", "CaveC", "CaveF", "CaveH", "CaveM2", "CaveB", "CaveA", "CaveN1", "CaveM1", "CaveJ", "CaveE", "SS_FF1", "SS_LR1", "CaveO", "CaveD"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveA"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveB"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveC"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"Deep9"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveD"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveE"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveF"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveG"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"Deep8", "CaveH"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveI"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveJ"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveK"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveL"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveM1", "CaveM2"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveN2", "CaveN3", "CaveN1"}),
            new RoomNode("SlipperySlopeCaves", new string[] {"CaveO", "FloodedBasementOutside"}),
            new RoomNode("FloodedBasement", new string[] {"FloodedBasementInside"}),
            new RoomNode("LonelyRoad", new string[] {"CaveA", "CaveB", "CaveC", "CaveD", "CaveE3"}), //Grand library section
            new RoomNode("LonelyRoad", new string[] { "LR_SS1", "CaveE1", "CaveP", "CaveG1", "CaveQ"}), //Section reached from slipperyslope
            new RoomNode("LonelyRoad", new string[] {"CaveG2", "CaveR", "CaveH1"}), //Section with thunderstrike for cave
            new RoomNode("LonelyRoad", new string[] {"CaveF2", "CaveK", "CaveE2"}), //Section with lots of grass snakes
            new RoomNode("LonelyRoad", new string[] {"CaveH2", "CaveM", "CaveN", "CaveT", "CaveS", "CaveU", "CaveJ1", "CaveI2"}), //Section with lake
            new RoomNode("LonelyRoad", new string[] {"CaveH3", "CaveO", "CaveF1", "CaveL"}), //Mjau cave section
            new RoomNode("LonelyRoad2", new string[] {"CaveJ2", "CaveI1"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveA", "Deep15"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveB"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveC"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveD"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveE1"}), //Divinding these two to avoid logic issues
            new RoomNode("LonelyRoadCaves", new string[] {"CaveE3", "CaveE2"}), //Divinding these two to avoid logic issues
            new RoomNode("LonelyRoadCaves", new string[] {"CaveF1", "CaveF2"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveG2", "CaveG1"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveH3", "CaveH2", "CaveH1"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveI1"}), //Need specific unlock order, splitting
            new RoomNode("LonelyRoadCaves", new string[] {"CaveI2"}), //Need specific unlock order, splitting
            new RoomNode("LonelyRoadCaves", new string[] {"CaveJ1", "CaveJ2"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveK"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveM"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveN"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveO"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveP"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveQ"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveR"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveS"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveT"}),
            new RoomNode("LonelyRoadCaves", new string[] {"CaveL"}),
            new RoomNode("LonelyRoadCaves", new string[] {"Deep14", "CaveU"}),
            //new RoomNode("GrandLibrary", new string[] {"GrandLibraryInside", "GrandLibrary1"}), IGNORING GRAND LIBRARY FOR NOW
            new RoomNode("Deep1", new string[] {"Deep1"}),
            new RoomNode("Deep2", new string[] {"Deep2"}),
            new RoomNode("Deep3", new string[] {"Deep3"}),
            new RoomNode("Deep4", new string[] {"Deep16", "Deep4"}),
            new RoomNode("Deep5", new string[] {"Deep5"}),
            new RoomNode("Deep6", new string[] {"Deep6"}),
            new RoomNode("Deep7", new string[] {"Deep7"}),
            new RoomNode("Deep7", new string[] {"Deep17"}),
            new RoomNode("Deep8", new string[] {"Deep8"}),
            new RoomNode("Deep9", new string[] {"Deep9"}),
            new RoomNode("Deep10", new string[] {"Deep10"}),
            new RoomNode("Deep11", new string[] {"Deep11"}),
            new RoomNode("Deep12", new string[] {"Deep12"}),
            new RoomNode("Deep13", new string[] {"Deep13"}),
            new RoomNode("Deep14", new string[] {"Deep14"}),
            new RoomNode("Deep15", new string[] {"Deep15"}),
            new RoomNode("Deep15", new string[] {"Deep21"}),
            new RoomNode("Deep16", new string[] {"Deep16"}),
            new RoomNode("Deep17", new string[] {"Deep17"}), //Ignoring deep19 (test) 
            new RoomNode("Deep17", new string[] {"Deep18"}),
            new RoomNode("Deep18", new string[] {"Deep23", "Deep20", "Deep18"}),
            new RoomNode("Deep20", new string[] {"Deep20"}),
            new RoomNode("Deep21", new string[] {"Deep21", "Deep22"}),
            new RoomNode("Deep22", new string[] {"Deep22"}),
            new RoomNode("Deep23", new string[] {"Deep23"})
            };
        }

        //Set and recover randomized rooms
        Dictionary<string, RoomNode.Link> randomizedDoors;
        bool useRandomized;
        void SetRandomizedRoom(RoomNode[] rooms)
        {
            useRandomized = true;
            //Combine dictionaries
            randomizedDoors = new Dictionary<string, RoomNode.Link>();
            for (int i = 0; i < rooms.Length; i++)
            {
                foreach (KeyValuePair<String, RoomNode.Link> kvp in rooms[i].connections)
                {
                        if (!randomizedDoors.TryGetValue(kvp.Key, out RoomNode.Link value))
                        {
                            randomizedDoors.Add(kvp.Key, kvp.Value);
                        }
                }
            }
        }

        public bool GetRandomizedDoor(string departureDoor, out string targetScene, out string targetDoor)
        {
            targetScene = null;
            targetDoor = null;

            if (!useRandomized) { return false; }
            string key = SceneManager.GetActiveScene().name + "/" + departureDoor;
            if (randomizedDoors.TryGetValue(key, out RoomNode.Link value))
            {
                targetScene = value.scene;
                targetDoor = value.door;
                return true;
            }
            return false;
        }

        string PrintSceneDoors(string scene, string[] connections)
        {
            string output = "Scene: " + SceneName.GetName(scene) + "\n---------------------------\n";
            //int j = 0;
            //int lineMax = 3;
            foreach (string door in connections)
            {
                /*if (j >= lineMax)
                {
                        output += "\n";
                        j = 0;
                }*/
                output += door + "\n";
                //j++;
            }
            output += "\n\n";
            return output;
        }

        string currentSeed;
        string log;
        //Randomize doors using a seed. If the flag is true, print the cheat sheet
        public void RandomizeWithSeed(string seed)
        {
            currentSeed = seed;
            UnityEngine.Random.State oldState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed.GetHashCode());
            BuildVanillaRooms();
            RoomNode[] result = RandomizeRoomNodes(vanillaRooms);
            if (result == null)
            {
                //GUIUtility.systemCopyBuffer = roomRandomizerLog;
                DebugCommands.Instance.OutputText("Randomizer returned null array!");
                return;
            }
            SetRandomizedRoom(result);
            string output = "##############################\n" + roomRandomizerLog + "Seed: " + seed + "\n##############################\n\n";
            output += PrintCheatSheet();
                
            log = output;
            //GUIUtility.systemCopyBuffer = output;
            UnityEngine.Random.state = oldState;
        }

        //Creates a string with the cheat sheet solution of the randomizer
        public string PrintCheatSheet()
        {
            string output = "";

            var list = randomizedDoors.Keys.ToList();
            list.Sort();

            string scene = "";
            List<string> doors = new List<string>();
            for (int k = 0; k < list.Count; k++)
            {
                string tempscene = "";
                for (int i = 0; i < list[k].Length; i++)
                {
                    if (list[k][i] != '/')
                    {
                        tempscene += list[k][i];
                    }
                    else
                    {
                        break;
                    }
                }
                if (scene == "") { scene = tempscene; }
                if (tempscene != scene)
                {
                    output += PrintSceneDoors(scene, doors.ToArray());
                    doors = new List<string>();
                    scene = tempscene;
                }
                string sceneRemoved = list[k].Remove(0, scene.Length + 1);
                doors.Add(sceneRemoved + " -> " + SceneName.GetName(randomizedDoors[list[k]].scene) + "/" + randomizedDoors[list[k]].door);
            }
            output += PrintSceneDoors(scene, doors.ToArray());

            return output;
        }

        public void PrintSheetTxt()
        {
            if (string.IsNullOrEmpty(currentSeed)) { currentSeed = "InvalidSeed"; }
            File.WriteAllText(ModMaster.RandomizerPath + "Door randomizer " + currentSeed + ".txt", log);
        }

        //Remove randomization
        public void ResetRooms()
        {
            useRandomized = false;
            currentSeed = null;
            randomizedDoors = new Dictionary<string, RoomNode.Link>();
        }
    }
}
