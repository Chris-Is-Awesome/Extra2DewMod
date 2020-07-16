using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.IO;
using System.Linq;
using ModStuff;

namespace ModStuff
{
    public class ModScriptHandler : Singleton<ModScriptHandler>
    {
        //New scene script
        string onNewSceneTxt;
        public string OnNewSceneTxt
        {
            get { return onNewSceneTxt; }
            set
            {
                onNewSceneTxt = value;
                if(string.IsNullOrEmpty(value))
                {
                    OutputToConsole("Cleared on scene load script", true);
                    return;
                }
                OutputToConsole("Set " + value + ".txt to run on each scene load", true);
            }
        }

        //Parse txt function
        string conditionNpcString = "#CONDITION_NPC";
        string conditionErrorMsg = "#ERRORMSG";
        public bool ParseTxt(string filename, out string errors)
        {
            List<string> commandlist = new List<string>();
            string line;
            errors = "";

            try
            {
                StreamReader theReader = new StreamReader(ModMaster.ScriptsPath + filename + ".txt");
                using (theReader)
                {
                    do
                    {
                        line = theReader.ReadLine();
                        if (line != null) { commandlist.Add(line); }
                    }
                    while (line != null);
                    theReader.Close();
                }
            }
            catch
            {
                return false;
            }

            //Check for conditions
            for (int i = 0; i < commandlist.Count; i++)
            {
                if (commandlist[i] == "") { continue; } //Skip command if it's empty
                if (SubstringCond(commandlist[i], "//")) { continue; } //Skip command if it's a comment
                if (SubstringCond(commandlist[i], conditionErrorMsg)) { continue; } //Skip command if it's an error message
                if (SubstringCond(commandlist[i], conditionNpcString)) //If the command is a condition, check it and skip it
                {
                    string targetNpc = commandlist[i].Substring(conditionNpcString.Length).Replace(" ", "");
                    GameObject[] gos = ModSpawner.Instance.FindEntityAndController(targetNpc);
                    if (gos[0] == null)
                    {
                        string errormsg = "Error: '" + targetNpc + "' NPC not found in line " + (i + 1).ToString() + ", canceling txt execution";
                        if (i < commandlist.Count - 1)
                        {
                            if (SubstringCond(commandlist[i + 1], conditionErrorMsg) && commandlist[i + 1].Length > (conditionErrorMsg.Length + 1))
                            {
                                errormsg = commandlist[i + 1].Substring(conditionErrorMsg.Length + 1);
                                errormsg = errormsg.Replace('|', '\n');
                            }
                            errors += errormsg;
                            ReportErrors(errors);
                            return true;
                        }
                        errors += errormsg;
                        ReportErrors(errors);
                        return true;
                    }
                    continue;
                }
                //ModSaver.SaveIntToPrefs("ComsUsed", ModSaver.LoadIntFromPrefs("ComsUsed") - 1);
                if (!DebugCommands.Instance.ParseResultString(commandlist[i], false)) { errors += "Invalid command '" + commandlist[i] + "'\n"; }
            }
            return true;
        }

        bool SubstringCond(string command, string condition)
        {
            if (command.Length < condition.Length) { return false; }
            return command.Substring(0, condition.Length) == condition;
        }

        void ReportErrors(string text)
        {
            DebugCommands.Instance.OutputText(text);
            OutputToConsole(text);
        }

        UIBigFrame menuText;
        public void OutputToConsole(string text, bool wrapText = true)
        {
            if (menuText == null)
            {
                GameObject console = GameObject.Find("LoadConfigConsole");
                if (console == null) { return; }
                menuText = console.GetComponent<UIBigFrame>();
            }
            if (wrapText) text = ModText.WrapText(text, 21f, false);
            text = ModText.Instance.FixVerticalShift(text);
            menuText.UIName = text;
        }

        public string[] GetScriptList()
        {
            List<string> output = new List<string>();
            string path = ModMaster.ScriptsPath;
            foreach (string file in System.IO.Directory.GetFiles(ModMaster.ScriptsPath))
            {
                int pos = file.IndexOf(".txt");
                if (pos >= 0)
                {
                    string beforeTxt = file.Remove(pos);
                    beforeTxt = beforeTxt.Remove(0, path.Length);
                    if (!output.Contains(beforeTxt) && beforeTxt != "init") output.Add(beforeTxt);
                }
            }

            return output.ToArray();
        }
    }
}