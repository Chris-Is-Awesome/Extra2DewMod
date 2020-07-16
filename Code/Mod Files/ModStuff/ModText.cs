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
    public class ModText : Singleton<ModText>
    {
        public static string GetString(string key)
        {
            ModText.Instance.CheckDictionary();
            if (ModText.Instance.modStrings.TryGetValue(key, out string output)) { return output; }
            return key;
        }
        Dictionary<string, string> modStrings;

        void CheckDictionary()
        {
            if(modStrings != null) { return; }
            modStrings = new Dictionary<string, string>
            {
                { "pausemenu_camera_default","Default camera\n\n" },
                { "pausemenu_camera_firstperson","First person camera \n\nPlay from Ittle's perspective. Look around using the mouse, move with the WASD keys and click to attack. Change your weapons with 1, 2, 3 and 4 or using the scroll wheel\n\nThe game needs the mouse to stay still for a moment to calibrate the camera" },
                { "pausemenu_camera_thirdperson","Third person camera \n\nOrbit around Ittle. Look around using the mouse, move with the WASD keys and click to attack. Change your weapons with 1, 2, 3 and 4 or using the scroll wheel. Hold right click and scroll to zoom in/out\n\nThe game needs the mouse to stay still for a moment to calibrate the camera" },
                { "pausemenu_camera_free","Free flight camera \n\nMove around the world, take the camera anywhere. Look around using the mouse and move with the WASD keys. Use the scroll wheel to speed down/up while flying\n\nThe game needs the mouse to stay still for a moment to calibrate the camera" }

            };
        }

        TextMesh characterFixer;
        void FixCharInfoIssue()
        {
            if (characterFixer != null) { return; }
            GameObject instantiatedButton = null;
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {

                if (go.name == "Default" && go.transform.parent != null && go.transform.parent.name == "KeyConfig")
                {
                    instantiatedButton = GameObject.Instantiate(go);
                    characterFixer = instantiatedButton.GetComponentInChildren<TextMesh>();
                    break;
                }
            }
            if (characterFixer == null) { return; } //I hope this doesnt happen
            string magicText = "abcdefghijklmnñopqrstuvwxyzABCDEFGHIJKLMNÑOPQRSTUVWXYZ0123456789<>_-(){}[]+*-\\/#@|º.,´`\"' \n\t";
            characterFixer.text = magicText;
            _font = characterFixer.font;
            instantiatedButton.SetActive(false);
            _font.RequestCharactersInTexture(magicText);
        }

        public static string WrapText(string text, float lineSize)
        {
            return ModText.Instance.GetTextWithJumps(text, lineSize);
        }

        public static string WrapText(string text, float lineSize, bool vertFix)
        {
            return ModText.Instance.GetTextWithJumps(text, lineSize, vertFix);
        }

        public string GetTextWithJumps(string text, float lineSize, bool vertFix)
        {
            if (string.IsNullOrEmpty(text)) { return null; }
            int index = text.IndexOf("</");
            if (index != -1)
            {
                for (int i = index; i < text.Length; i++)
                {
                    if (text[i] == '>') { return text; }
                }
            }
            FixCharInfoIssue();
            float charSize = 1f;
            string newText = AddLineJumps(text, charSize, lineSize);
            if(vertFix) newText = FixVerticalShift(newText);

            return newText;
        }

        public string GetTextWithJumps(string text, float lineSize)
        {
            if (string.IsNullOrEmpty(text)) { return null; }
            int index = text.IndexOf("</");
            if(index != -1)
            {
                for(int i = index;i<text.Length;i++)
                {
                    if(text[i] == '>') { return text; }
                }
            }
            FixCharInfoIssue();
            float charSize = 1f;
            string newText = AddLineJumps(text, charSize, lineSize);
            newText = FixVerticalShift(newText);

            return newText;
        }

        public string GetTextWithJumps(string text, float lineSize, float charSize)
        {
            if (string.IsNullOrEmpty(text)) { return null; }
            FixCharInfoIssue();
            string newText = AddLineJumps(text, charSize, lineSize);
            newText = FixVerticalShift(newText);

            return newText;
        }

        public float GetMaxLineSize(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0f;
            FixCharInfoIssue();
            float output = 0f;
            float px = 0f;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\n')
                {
                    if (px > output) output = px;
                    px = 0f;
                }
                else
                {
                    _font.GetCharacterInfo(c, out CharacterInfo characterInfo);
                    px += (float)characterInfo.advance;
                }
            }
            if (px > output) output = px;
            return output;
        }

        //Text wrapping
        Font _font;
        //Pause menu wrapSize: 980f
        bool DoWrapHere(string text, int index, float initialLength, float charScale, float wrapSize)
        {
            float px = initialLength;
            char newLetter = text[index];
            if (newLetter == '\n') { return false; }
            if (newLetter == ' ')
            {
                _font.GetCharacterInfo(newLetter, out CharacterInfo characterInfoSpace);
                px += (float)characterInfoSpace.advance * charScale;
                for (int i = index + 1; i < text.Length; i++)
                {
                    char c = text[i];
                    if (c == ' ' || c == '\n') { return px > wrapSize; }
                    _font.GetCharacterInfo(c, out CharacterInfo characterInfo);

                    px += (float)characterInfo.advance * charScale;
                    if (px > wrapSize) { return true; }
                }
                return false;
            }
            _font.GetCharacterInfo(newLetter, out CharacterInfo characterInfo2);
            px += (float)characterInfo2.advance * charScale;

            return px > wrapSize;
        }

        //PauseMenu charsize: 25f
        string AddLineJumps(string text, float charSize, float lineSize)
        {
            float lineLength = 0f;
            float charScale = charSize * 0.03f;
            string output = "";

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\n')
                {
                    output += '\n';
                    lineLength = 0f;
                }
                else
                {
                    bool addChar = true;
                    if (DoWrapHere(text, i, lineLength, charScale, lineSize))
                    {
                        output += '\n';
                        lineLength = 0f;
                        if (c == ' ') { addChar = false; }
                    }
                    if (addChar)
                    {
                        _font.GetCharacterInfo(c, out CharacterInfo characterInfo);
                        output += c;
                        lineLength += (float)characterInfo.advance * charScale;
                    }
                }
            }
            return output;

        }

        public string FixVerticalShift(string text)
        {
            if (text.Contains("\n"))
            {
                string str = "";
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == '\n')
                    {
                        str += "\n";
                    }
                }
                text = str + text;
            }
            return text;
        }
    }
}