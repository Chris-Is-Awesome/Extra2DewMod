using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class StringHolder
{
	public struct OutString
	{
		public OutString(string text, Vector2 scale, Font font)
		{
			this.text = text;
			this.scale = scale;
			this.font = font;
		}

		public OutString(string text)
		{
			this.text = text;
			this.scale = Vector2.one;
			this.font = null;
		}

		public string text;

		public Vector2 scale;

		public Font font;
	}

	private class ConfigLine
	{
		public ConfigLine(string v)
		{
			this.value = v;
		}

		public virtual void Write(TextWriter writer)
		{
			writer.WriteLine(this.value);
		}

		public string value;

		public static readonly StringHolder.ConfigLine Empty = new StringHolder.ConfigLine(string.Empty);
	}

	private struct StringData
	{
		public StringData(string data, StringHolder.HolderData baseData)
		{
			this.fullData = data;
			int num = data.IndexOf('(');
			int num2 = data.IndexOf(')');
			this.scaleX = (this.scaleY = 1f);
			if (num != -1 && num2 != -1)
			{
				this.name = data.Substring(0, num).Trim();
				string[] array = data.Substring(num + 1, num2 - num - 1).Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = array[i].Split(new char[]
					{
						':'
					});
					if (array2.Length > 1)
					{
						string a = array2[0].Trim();
						string v = array2[1].Trim();
						float num3;
						if (a == "sx")
						{
							StringHolder.StringData.TryGetFloat(v, baseData, out this.scaleX);
						}
						else if (a == "sy")
						{
							StringHolder.StringData.TryGetFloat(v, baseData, out this.scaleY);
						}
						else if (a == "s" && StringHolder.StringData.TryGetFloat(v, baseData, out num3))
						{
							this.scaleX = (this.scaleY = num3);
						}
					}
				}
			}
			else if (num != -1 && num2 == -1)
			{
				this.name = data.Substring(0, num);
				Debug.LogWarning("Malformed string attributes near " + data);
			}
			else
			{
				this.name = data;
			}
		}

		private static bool TryGetFloat(string v, StringHolder.HolderData data, out float res)
		{
			StringHolder.ConfigString configString;
			return float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out res) || (data.strings.TryGetValue(v, out configString) && float.TryParse(configString.value, NumberStyles.Any, CultureInfo.InvariantCulture, out res));
		}

		public string fullData;

		public string name;

		public float scaleX;

		public float scaleY;
	}

	private struct StringChunk
	{
		public StringChunk(string value, bool isVar)
		{
			this.value = value;
			this.isVar = isVar;
			this.nested = null;
		}

		public string value;

		public StringHolder.StringChunk[] nested;

		public bool isVar;
	}

	private class ConfigString : StringHolder.ConfigLine
	{
		private class StringStream
		{
			public StringStream(string str)
			{
				this.str = str;
				this.pos = 0;
			}

			public bool IsAtEnd()
			{
				return this.pos >= this.str.Length;
			}

			public int PeekNext()
			{
				if (this.pos + 1 < this.str.Length)
				{
					return (int)this.str[this.pos + 1];
				}
				return -1;
			}

			public bool Get(out char c)
			{
				if (this.pos < this.str.Length)
				{
					c = this.str[this.pos++];
					return true;
				}
				c = '\0';
				return false;
			}

			public int Get()
			{
				if (this.pos < this.str.Length)
				{
					return (int)this.str[this.pos++];
				}
				return -1;
			}

			private string str;

			private int pos;
		}

		public ConfigString(StringHolder.StringData data, string value) : base(value)
		{
			this.data = data;
		}

		public override void Write(TextWriter writer)
		{
			writer.WriteLine(this.data.fullData + ": " + this.value);
		}

		private static string ResolveCompileVars(string value, StringHolder.HolderData allData)
		{
			if (value.IndexOf('<') == -1)
			{
				return value;
			}
			string text = string.Empty;
			string text2 = string.Empty;
			bool flag = false;
			foreach (char c in value)
			{
				if (!flag)
				{
					if (c == '<')
					{
						text2 = string.Empty;
						flag = true;
					}
					else
					{
						text += c;
					}
				}
				else if (c == '>')
				{
					flag = false;
					string key = text2.Trim();
					StringHolder.ConfigString configString;
					if (allData.strings.TryGetValue(key, out configString))
					{
						if (!configString.wasResolved)
						{
							configString.Resolve(allData);
						}
						text += configString.finalValue;
					}
					else
					{
						string text3 = text;
						text = string.Concat(new object[]
						{
							text3,
							'<',
							text2,
							'>'
						});
					}
				}
				else
				{
					text2 += c;
				}
			}
			return text;
		}

		private static void PushChunk(ref List<StringHolder.StringChunk> result, string str)
		{
			if (!string.IsNullOrEmpty(str))
			{
				if (result == null)
				{
					result = new List<StringHolder.StringChunk>();
				}
				result.Add(new StringHolder.StringChunk(str, false));
			}
		}

		private static void PushChunk(ref List<StringHolder.StringChunk> result, List<StringHolder.StringChunk> list)
		{
			if (result == null)
			{
				result = new List<StringHolder.StringChunk>();
			}
			if (list.Count == 1)
			{
				StringHolder.StringChunk item = list[0];
				item.isVar = true;
				result.Add(item);
			}
			else
			{
				StringHolder.StringChunk item2 = default(StringHolder.StringChunk);
				item2.isVar = true;
				item2.nested = list.ToArray();
				result.Add(item2);
			}
		}

		private static List<StringHolder.StringChunk> DoDecomposeString(StringHolder.ConfigString.StringStream str, int level = 0)
		{
			List<StringHolder.StringChunk> result = null;
			string text = string.Empty;
			char c;
			while (str.Get(out c))
			{
				if (c == '\\')
				{
					str.Get(out c);
					text += c;
				}
				else if (c == '[')
				{
					List<StringHolder.StringChunk> list = StringHolder.ConfigString.DoDecomposeString(str, level + 1);
					if (list != null)
					{
						StringHolder.ConfigString.PushChunk(ref result, text);
						text = string.Empty;
						StringHolder.ConfigString.PushChunk(ref result, list);
					}
				}
				else
				{
					if (c == ']' && level > 0)
					{
						break;
					}
					text += c;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				StringHolder.ConfigString.PushChunk(ref result, text);
			}
			return result;
		}

		private static List<StringHolder.StringChunk> DecomposeString(string str)
		{
			if (str.IndexOf('[') == -1)
			{
				return null;
			}
			return StringHolder.ConfigString.DoDecomposeString(new StringHolder.ConfigString.StringStream(str), 0);
		}

		private static string ComposeChunks(StringHolder.StringChunk[] chunks, Dictionary<string, string> vars, StringHolder allStrings)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (StringHolder.StringChunk stringChunk in chunks)
			{
				if (stringChunk.isVar)
				{
					string text;
					if (stringChunk.nested != null)
					{
						text = StringHolder.ConfigString.ComposeChunks(stringChunk.nested, vars, allStrings);
					}
					else
					{
						text = stringChunk.value;
					}
					string value;
					bool flag;
					if (text.Length > 0 && text[0] == '!')
					{
						flag = allStrings.TryGetString(text.Substring(1), vars, out value);
					}
					else
					{
						flag = vars.TryGetValue(text, out value);
					}
					if (flag)
					{
						stringBuilder.Append(value);
					}
					else
					{
						stringBuilder.Append('[');
						stringBuilder.Append(text);
						stringBuilder.Append(']');
					}
				}
				else
				{
					stringBuilder.Append(stringChunk.value);
				}
			}
			return stringBuilder.ToString();
		}

		public void Resolve(StringHolder.HolderData allData)
		{
			if (this.wasResolved)
			{
				return;
			}
			this.wasResolved = true;
			this.finalValue = this.value.Replace(allData.newline, '\n');
			this.finalValue = StringHolder.ConfigString.ResolveCompileVars(this.finalValue, allData);
			List<StringHolder.StringChunk> list = StringHolder.ConfigString.DecomposeString(this.finalValue);
			if (list != null)
			{
				this.chunks = list.ToArray();
			}
		}

		public string Name
		{
			get
			{
				return this.data.name;
			}
		}

		public string String
		{
			get
			{
				return this.finalValue;
			}
		}

		public string GetString(Dictionary<string, string> vars, StringHolder allStrings)
		{
			if (this.chunks == null)
			{
				return this.finalValue;
			}
			return StringHolder.ConfigString.ComposeChunks(this.chunks, vars, allStrings);
		}

		public StringHolder.StringData data;

		public string finalValue;

		public StringHolder.StringChunk[] chunks;

		public bool wasResolved;
	}

	private class HolderData
	{
		public Font GetLoadedFont()
		{
			if (this.loadedFont == null)
			{
				this.loadedFont = FontMaterialMap.LookupFont(this.font);
			}
			return this.loadedFont;
		}

		public StringHolder.ConfigString GetString(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			StringHolder.ConfigString result;
			this.strings.TryGetValue(name, out result);
			return result;
		}

		public string font;

		public float baseScale = 1f;

		public Dictionary<string, StringHolder.ConfigString> strings = new Dictionary<string, StringHolder.ConfigString>();

		public List<StringHolder.ConfigLine> allLines;

		public char newline = StringHolder.DefaultNewline;

		public bool hasFont;

		public bool hasScale;

		private Font loadedFont;
	}

	public StringHolder(byte[] data)
	{
		using (MemoryStream memoryStream = new MemoryStream(data))
		{
			using (StreamReader streamReader = new StreamReader(memoryStream))
			{
				this.Load(streamReader);
			}
		}
	}

	public StringHolder(string filename)
	{
		using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
		{
			using (StreamReader streamReader = new StreamReader(fileStream))
			{
				this.Load(streamReader);
			}
		}
	}

	private static int GetFirstTokenIndex(string line, char token)
	{
		if (string.IsNullOrEmpty(line))
		{
			return -1;
		}
		int i = 0;
		while (i < line.Length)
		{
			if (!char.IsWhiteSpace(line[i]))
			{
				if (line[i] == token)
				{
					return i;
				}
				return -1;
			}
			else
			{
				i++;
			}
		}
		return -1;
	}

	private static void HandleComment(string line, StringHolder.HolderData data, int lineno)
	{
		int num = line.IndexOf("META");
		if (num != -1)
		{
			string[] array = line.Substring(num).Split(new char[]
			{
				' '
			});
			if (array[0] == "META" && array.Length > 1)
			{
				if (array[1] == "font" && array.Length > 2)
				{
					if (array[2] != "default")
					{
						data.hasFont = true;
						data.font = array[2];
					}
				}
				else if (array[1] == "basescale" && array.Length > 2)
				{
					if (!float.TryParse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture, out data.baseScale))
					{
						Debug.LogWarning("Error parsing StringHolder basescale at line " + lineno);
					}
					else
					{
						data.hasScale = true;
					}
				}
				else if (array[1] == "newline" && array.Length > 2)
				{
					data.newline = array[2][0];
				}
			}
		}
		if (data.allLines != null)
		{
			data.allLines.Add(new StringHolder.ConfigLine(line));
		}
	}

	private static bool ReadStringData(string dataStr, StringHolder.HolderData data, out StringHolder.StringData result)
	{
		result = new StringHolder.StringData(dataStr, data);
		return !string.IsNullOrEmpty(result.name);
	}

	private static bool SeparateNameValue(string line, out string name, out string value, int lineno)
	{
		name = string.Empty;
		value = string.Empty;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < line.Length; i++)
		{
			num2++;
			char c = line[i];
			if (num == 0 && c == StringHolder.KeySepToken)
			{
				break;
			}
			name += c;
			if (c == '(')
			{
				num++;
			}
			else if (c == ')')
			{
				num--;
			}
		}
		value = line.Substring(num2).Trim();
		if (num == 0)
		{
			name = name.Trim();
			return true;
		}
		Debug.LogError("Mismatched parantheses at line " + lineno);
		return false;
	}

	private static bool ParseLine(string line, StringHolder.HolderData data, int lineno)
	{
		int num = line.IndexOf(StringHolder.KeySepToken);
		string dataStr;
		string value;
		if (num == -1 || !StringHolder.SeparateNameValue(line, out dataStr, out value, lineno))
		{
			return false;
		}
		StringHolder.StringData data2;
		if (!StringHolder.ReadStringData(dataStr, data, out data2))
		{
			Debug.LogWarning("Empty string key at line " + lineno);
			return false;
		}
		StringHolder.ConfigString configString = new StringHolder.ConfigString(data2, value);
		if (data.allLines != null)
		{
			data.allLines.Add(configString);
		}
		if (data.strings.ContainsKey(data2.name))
		{
			Debug.LogWarning(string.Concat(new object[]
			{
				"Duplicate string ",
				data2.name,
				" at line ",
				lineno,
				", overwriting"
			}));
		}
		data.strings[data2.name] = configString;
		return true;
	}

	private static StringHolder.HolderData ReadStream(TextReader reader, bool fullData)
	{
		StringHolder.HolderData holderData = new StringHolder.HolderData();
		if (fullData)
		{
			holderData.allLines = new List<StringHolder.ConfigLine>();
		}
		List<StringHolder.ConfigLine> allLines = holderData.allLines;
		int num = 0;
		while (reader.Peek() != -1)
		{
			num++;
			string text = reader.ReadLine();
			if (string.IsNullOrEmpty(text))
			{
				if (allLines != null)
				{
					allLines.Add(StringHolder.ConfigLine.Empty);
				}
			}
			else if (StringHolder.GetFirstTokenIndex(text, StringHolder.CommentToken) != -1)
			{
				StringHolder.HandleComment(text, holderData, num);
			}
			else if (!StringHolder.ParseLine(text, holderData, num) && allLines != null)
			{
				allLines.Add(new StringHolder.ConfigLine(text));
			}
		}
		foreach (KeyValuePair<string, StringHolder.ConfigString> keyValuePair in holderData.strings)
		{
			keyValuePair.Value.Resolve(holderData);
		}
		return holderData;
	}

    //Mod function to add new entries
    public void ModAddEntry(string text)
    {
        if (!StringHolder.ParseLine(text, currentData, 0))
        {
            currentData.allLines.Add(new StringHolder.ConfigLine(text));
        }
		foreach (KeyValuePair<string, StringHolder.ConfigString> keyValuePair in currentData.strings)
		{
			keyValuePair.Value.Resolve(currentData);
		}
    }

	private static int MatchNum(string a, string b)
	{
		int num = Mathf.Min(a.Length, b.Length);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (a[i] != b[i])
			{
				return num2;
			}
			num2++;
		}
		return num2;
	}

	public void Load(TextReader reader)
	{
		this.currentData = StringHolder.ReadStream(reader, StringHolder.StoreLines);
	}

	public bool HasFont
	{
		get
		{
			return this.currentData.hasFont;
		}
	}

	public string FontName
	{
		get
		{
			return this.currentData.font;
		}
	}

	private static Dictionary<string, string> MergeVarSets(Dictionary<string, string> a, Dictionary<string, string> b)
	{
		if (a == null || a.Count == 0)
		{
			return b;
		}
		if (b == null || b.Count == 0)
		{
			return a;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>(a.Count + b.Count);
		foreach (KeyValuePair<string, string> keyValuePair in a)
		{
			dictionary[keyValuePair.Key] = keyValuePair.Value;
		}
		foreach (KeyValuePair<string, string> keyValuePair2 in b)
		{
			dictionary[keyValuePair2.Key] = keyValuePair2.Value;
		}
		return dictionary;
	}

	private string DoGetString(StringHolder.ConfigString strData, Dictionary<string, string> vars)
	{
		if (strData.chunks == null)
		{
			return strData.String;
		}
		vars = StringHolder.MergeVarSets(vars, this.globalValues);
		if (vars == null || vars.Count == 0)
		{
			return strData.String;
		}
		return strData.GetString(vars, this);
	}

	private Font GetDefaultFont()
	{
		if (this.currentData.hasFont)
		{
			return this.currentData.GetLoadedFont();
		}
		return null;
	}

	public string GetString(string key)
	{
		StringHolder.ConfigString @string = this.currentData.GetString(key);
		if (@string != null)
		{
			return this.DoGetString(@string, null);
		}
		Debug.LogWarning("String not found: " + key);
		return string.Empty;
	}

	public string GetString(string key, Dictionary<string, string> vars)
	{
		if (vars == null || vars.Count == 0)
		{
			return this.GetString(key);
		}
		StringHolder.ConfigString @string = this.currentData.GetString(key);
		if (@string != null)
		{
			return this.DoGetString(@string, vars);
		}
		Debug.LogWarning("String not found: " + key);
		return string.Empty;
	}

	public bool TryGetString(string key, Dictionary<string, string> vars, out string result)
	{
		StringHolder.ConfigString @string = this.currentData.GetString(key);
		if (@string == null)
		{
			result = null;
			return false;
		}
		result = this.DoGetString(@string, vars);
		return true;
	}

	public StringHolder.OutString GetFullString(string key, Dictionary<string, string> vars = null)
	{
		StringHolder.ConfigString @string = this.currentData.GetString(key);
		if (@string != null)
		{
			Vector2 vector = Vector2.one;
			if (this.currentData.hasScale)
			{
				vector *= this.currentData.baseScale;
			}
			vector.x *= @string.data.scaleX;
			vector.y *= @string.data.scaleY;
			string text = this.DoGetString(@string, vars);
			return new StringHolder.OutString(text, vector, this.GetDefaultFont());
		}
		Debug.LogWarning("String not found: " + key);
		Vector2 vector2 = Vector2.one;
		if (this.currentData.hasScale)
		{
			vector2 *= this.currentData.baseScale;
		}
		return new StringHolder.OutString(key, vector2, null);
	}

	public bool HasString(string key)
	{
		return this.currentData.strings.ContainsKey(key);
	}

	public void SetGlobalValues(Dictionary<string, string> values)
	{
		this.globalValues = values;
	}

	private static readonly char CommentToken = '#';

	private static readonly char KeySepToken = ':';

	private static readonly char DefaultNewline = '|';

	private static readonly bool StoreLines;

	private StringHolder.HolderData currentData;

	private Dictionary<string, string> globalValues = new Dictionary<string, string>();
}
