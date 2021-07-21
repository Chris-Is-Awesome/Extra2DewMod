using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace ModStuff
{
    public static class RNGSeed
    {
        const int SEEDLENGTH = 8;
        const string SEEDCHARS = "0123456789ABCDEFGHIJKLMNPQRSTUVWXYZ_";
        static string _currentSeed;
        static public string CurrentSeed
        {
            get
            {
                if (string.IsNullOrEmpty(_currentSeed)) ReRollSeed();
                return _currentSeed;
            }
        }

        //Generate random seed
        static public void ReRollSeed()
        {
            string output = "";

            for (int i = 0; i < SEEDLENGTH; i++)
            {
                output += SEEDCHARS[UnityEngine.Random.Range(0, SEEDCHARS.Length)];
            }

            _currentSeed = output;
        }

        //Set a seed
        public static void SetSeed(string seed)
        {
            string output = "";
            if (string.IsNullOrEmpty(seed)) { output = "00000000"; }

            //Fix buffered seed
            string upperSeed = seed.ToUpper();

            for (int i = 0; i < SEEDLENGTH; i++)
            {
                if (i < upperSeed.Length)
                {
                    if(SEEDCHARS.Contains(upperSeed[i]))
                    {
                        output += upperSeed[i];
                    }
                    else if (upperSeed[i] == ' ')
                    {
                        output += "_";
                    }
                    else
                    {
                        output += "0";
                    }
                }
                else
                {
                    output += "_";
                }
            }

            _currentSeed = output;
        }
    }
}
