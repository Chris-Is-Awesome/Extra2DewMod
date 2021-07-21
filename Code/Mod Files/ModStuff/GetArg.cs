using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace ModStuff
{
    public static class FindArg
    {
        //ParseBool
        public static bool ParseBool(string s, out bool result)
        {
            string arg_value = s.ToLower();
            result = false;
            if (arg_value == "on" || arg_value == "yes" || arg_value == "1" || arg_value == "true")
            {
                result = true;
                return true;
            }
            if (arg_value == "off" || arg_value == "no" || arg_value == "0" || arg_value == "false")
            {
                result = false;
                return true;
            }
            return false;
        }

        //ParseVector3 (for string array)
        public static bool ParseVector3(string[] args, out Vector3 result, int index = 0)
        {
            result = Vector3.zero;

            if ((index + 3) > args.Length) { return false; }

            if (float.TryParse(args[index], out float x) && float.TryParse(args[index + 1], out float y) && float.TryParse(args[index + 2], out float z))
            {
                result = new Vector3(x, y, z);
                return true;
            }
            return false;
        }

        //ParseVector3 (for string)
        public static Boolean ParseVector3(string args, out Vector3 result)
        {
            result = Vector3.zero;

            if (float.TryParse(args, out float x))
            {
                result = new Vector3(x, x, x);
                return true;
            }
            return false;
        }

        //-------------------------------------
        //GetArg functions
        //-------------------------------------

        //Find single argument
        public static bool GetSingle(string key_arg, string[] args)
        {
            string target_arg = key_arg.ToLower();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == target_arg) { return true; }
            }
            return false;
        }

        //Find float argument
        public static bool GetFloat(string key_arg, out float output, string[] args, out bool invalidValue)
        {
            string target_arg = key_arg.ToLower();
            bool result = false;
            output = 0f;
            invalidValue = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == target_arg)
                {
                    if ((i + 1) < args.Length)
                    {
                        if (float.TryParse(args[i + 1], out float tempvalue))
                        {
                            output = tempvalue;
                            result = true;
                        }
                    }
                    invalidValue = true;
                }
            }
            if (result) { invalidValue = false; }
            return result;
        }

        //Find int argument
        public static bool GetInt(string key_arg, out int output, string[] args, out bool invalidValue)
        {
            string target_arg = key_arg.ToLower();
            bool result = false;
            output = 0;
            invalidValue = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == target_arg)
                {
                    if ((i + 1) < args.Length)
                    {
                        if (int.TryParse(args[i + 1], out int tempvalue))
                        {
                            output = tempvalue;
                            result = true;
                        }
                    }
                    invalidValue = true;
                }
            }
            if (result) { invalidValue = false; }
            return result;
        }

        //Find bool argument
        public static bool GetBool(string key_arg, out bool output, string[] args, out bool invalidValue)
        {
            string target_arg = key_arg.ToLower();
            bool result = false;
            output = false;
            invalidValue = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == target_arg)
                {
                    if ((i + 1) < args.Length)
                    {
                        if (ParseBool(args[i + 1], out bool tempvalue))
                        {
                            output = tempvalue;
                            result = true;
                        }
                    }
                    invalidValue = true;
                }
            }
            if (result) { invalidValue = false; }
            return result;
        }

        //Find string argument
        public static bool GetString(string key_arg, out string output, string[] args, out bool invalidValue)
        {
            string target_arg = key_arg.ToLower();
            bool result = false;
            output = "";
            invalidValue = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == target_arg)
                {
                    invalidValue = true;
                    if ((i + 1) < args.Length)
                    {
                        if (args[i + 1] != "")
                        {
                            output = args[i + 1];
                            result = true;
                        }
                    }
                }
            }
            if (result) { invalidValue = false; }
            return result;
        }

        //Find Vector3 argument
        public static bool GetVector3(string key_arg, out Vector3 output, string[] args, out bool invalidValue)
        {
            string target_arg = key_arg.ToLower();
            bool result = false;
            output = Vector3.zero;
            invalidValue = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == target_arg)
                {
                    if ((i + 3) < args.Length)
                    {
                        if (float.TryParse(args[i + 1], out float x) && float.TryParse(args[i + 2], out float y) && float.TryParse(args[i + 3], out float z))
                        {
                            output = new Vector3(x, y, z);
                            result = true;
                        }
                    }
                    invalidValue = true;
                }
            }
            if (result) { invalidValue = false; }
            return result;
        }

        //Find array argument (string output)
        public static bool GetArray(string key_arg, int arrayLength, out string[] output, string[] args, out bool invalidValue)
        {
            output = new string[arrayLength];
            string target_arg = key_arg.ToLower();
            bool result = false;
            invalidValue = false;

            for (int i = 0; i < args.Length; i++)
            {
                //Check for key word
                if (args[i] != target_arg)
                {
                    continue;
                }
                if ((i + arrayLength) < args.Length)
                {
                    result = true;
                    for (int j = 0; j < arrayLength; j++)
                    {
                        if (args[i + j + 1] != null) { output[j] = args[i + j + 1]; } else { result = false; }
                    }
                }
                invalidValue = true;
            }
            if (result) { invalidValue = false; }
            return result;
        }

        //Find array elements argument (string return)
        public static bool GetArrayString(string key_arg, string[] key_parameters, out string[] output, string[] args, out bool invalidValue)
        {
            output = new string[key_parameters.Length];
            string target_arg = key_arg.ToLower();
            bool result = false;
            invalidValue = false;

            for (int i = 0; i < args.Length; i++)
            {
                //Check for key word
                if (args[i] != target_arg)
                {
                    continue;
                }
                //Change invalidValue to true, if a correct value was
                //found, invalidValue will be changed before returning
                invalidValue = true;

                //If there is no space for the second argumentand number, go to the next loop
                if ((i + 2) >= args.Length)
                {
                    continue;
                }

                for (int j = 0; j < key_parameters.Length; j++)
                {
                    if (args[i + 1] == key_parameters[j].ToLower() && args[i + 2] != "")
                    {
                        output[j] = args[i + 2];
                        result = true;
                    }
                }
            }
            if (result) { invalidValue = false; }
            return result;
        }

        //Find array elements argument (float return)
        public static bool GetArrayFloat(string key_arg, string[] key_parameters, out float[] output, string[] args, out bool invalidValue)
        {
            output = new float[key_parameters.Length];
            string target_arg = key_arg.ToLower();
            bool result = false;
            invalidValue = false;

            for (int i = 0; i < args.Length; i++)
            {
                //Check for key word
                if (args[i] != target_arg)
                {
                    continue;
                }
                //Change invalidValue to true, if a correct value was
                //found, invalidValue will be changed before returning
                invalidValue = true;

                //If there is no space for the second argumentand number, go to the next loop
                if ((i + 2) >= args.Length)
                {
                    continue;
                }

                for (int j = 0; j < key_parameters.Length; j++)
                {
                    if (args[i + 1] == key_parameters[j].ToLower() && float.TryParse(args[i + 2], out float floatvalue))
                    {
                        output[j] = floatvalue;
                        result = true;
                    }
                }
            }
            if (result) { invalidValue = false; }
            return result;
        }
    }
}
