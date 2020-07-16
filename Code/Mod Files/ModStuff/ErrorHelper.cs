using UnityEngine;

namespace ModStuff
{
	static class ErrorHelper
	{
		public enum ErrorType
		{
			Unspecified,
			Unknown,
			NullRef,
			OutOfBounds,
			InvalidArg
		}

		// Returns a formatted error message and optionally saves it to PlayerPrefs.SetString("test")
		public static string SetErrorText(string errorInfo, ErrorType errorType = ErrorType.Unspecified, bool includeErrorType = true,  bool includeStackTrace = true, bool saveToPref = true)
		{
			string errorMsg = "ERROR: ";

			if (errorType != ErrorType.Unspecified && includeErrorType)
			{
				if (includeErrorType)
				{
					switch (errorType)
					{
						case ErrorType.NullRef:
							errorMsg += "Null reference exception!";
							break;
						case ErrorType.OutOfBounds:
							errorMsg += "Index out of bounds!";
							break;
						case ErrorType.InvalidArg:
							errorMsg += "Invalid argument!";
							break;
					}

					errorMsg += "\n";
				}
			}

			errorMsg += errorInfo;

			if (includeStackTrace)
			{
				string stackTrace = StackTraceUtility.ExtractStackTrace();
				string divider = "\n----------------------------------------------------------------------------------------------------------------------------------------------------------------------------";
				errorMsg += divider + "<size=13>" + stackTrace.Remove(0, stackTrace.IndexOf(')') + 1) + "</size>";
			}

			if (saveToPref) { PlayerPrefs.SetString("test", errorMsg); }

			return errorMsg;
		}
	}
}