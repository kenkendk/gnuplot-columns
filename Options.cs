#region Disclaimer / License
// Copyright (C) 2012, Kenneth Skovhede
// http://www.hexad.dk, opensource@hexad.dk
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
// 
#endregion

using System;
using System.Linq;
using System.Collections.Generic;

namespace Columns
{
	public enum OptionType
	{
		String,
		Integer,
		Boolean
	}

	public class OptionItem
	{
		public readonly string Key;
		public readonly string Description;
		public readonly string DefaultValue;
		public readonly bool Required;
		public readonly OptionType Type;
		public readonly Func<string, bool> Validator;

		public OptionItem(string key, string description, string defaultvalue, OptionType type = OptionType.String, bool required = false, Func<string, bool> validator = null)
		{
			this.Key =  key;
			this.Description = description;
			this.DefaultValue = defaultvalue;
			this.Required = required;
			this.Type = type;
			if (validator != null)
				this.Validator = validator;
			else
			{
				if (this.Type == OptionType.Boolean)
					this.Validator = (x) => {
						bool b;
						return string.IsNullOrEmpty(x) || bool.TryParse(x, out b);
					};
				else if (this.Type == OptionType.Integer)
					this.Validator = (x) => {
						int i;
						return int.TryParse(x, out i);
					};
				else
					this.Validator = null;

			}
		}
	}

	public class Options
	{
		public static Dictionary<string, string> ParseCommandLine(List<string> args)
		{
			var d = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			var r = ParseCommandLine(args, d);
			args.Clear();
			args.AddRange(r);
			return d;
		}

		public static List<string> ParseCommandLine(IEnumerable<string> inputargs, Dictionary<string, string> options)
		{
			return inputargs.Where(x => {
				if (x.StartsWith("--"))
				{
					int ix = x.IndexOf("=");
					string k;
					string v;
					if (ix < 0)
					{
						k = x.Substring(2);
						v = "";
					}
					else
					{
						k = x.Substring(2, ix-2);
						v = x.Substring(ix+1);
					}

					options[k] = v;
					return false;
				}
				else
				{
					return true;
				}
			}).ToList();
		}

		private Dictionary<string, string> m_opts;

		private static readonly OptionItem COLUMNS_OPT = new OptionItem("columns", "The number of columns to output", "3", OptionType.Integer, false);
		private static readonly OptionItem COLUMN_SEP_OPT = new OptionItem("columseperator", "The character(s) to output as columns seperators", "\t", OptionType.String);
		private static readonly OptionItem FIELD_SEP_OPT = new OptionItem("fieldseperator", "The character(s) used to seperate fields", "\t ", OptionType.String);
		private static readonly OptionItem FIELD_IDX_OPT = new OptionItem("fieldindex", "The index of the field to output, 0 means entire line, -1 means first number field, -2 means last number field", "0", OptionType.Integer);
		private static readonly OptionItem LINE_RX_OPT = new OptionItem("lineregexp", "The regular expression to find in the line", ".*", OptionType.String, false, (x) => {
			try {
				var z = new System.Text.RegularExpressions.Regex(x);
				return true;
			} catch {}
			return false;
		});
		private static readonly OptionItem INPUT_FILE_OPT = new OptionItem("inputfile", "The file to read input data from, will use stdin if not present", "", OptionType.String);
		private static readonly OptionItem OUPUT_FILE_OPT = new OptionItem("outputfile", "The file to write output data to, will use stdout if not present", "", OptionType.String);
		private static readonly OptionItem VERBOSE_OPT = new OptionItem("verbose", "True to generate debug output, false otherwise", "false", OptionType.Boolean);

		private static readonly OptionItem[] _supportedOptions = new OptionItem[] {
			COLUMNS_OPT,
			COLUMN_SEP_OPT,
			FIELD_SEP_OPT,
			FIELD_IDX_OPT,
			LINE_RX_OPT,
			INPUT_FILE_OPT,
			OUPUT_FILE_OPT,
			VERBOSE_OPT
		};

		public static IEnumerable<OptionItem> SupportedOptions { get { return _supportedOptions; } }

		public Options(Dictionary<string, string> opts)
		{
			m_opts = opts;

			foreach(var k in opts.Keys)
				if (!_supportedOptions.Select(x => x.Key).Contains(k, System.StringComparer.CurrentCultureIgnoreCase))
					throw new Exception("Unsupported option found: " + k);

			foreach(var op in _supportedOptions.Where(x => x.Required))
				if (!opts.ContainsKey(op.Key))
					throw new Exception("Required option not found: " + op.Key);

			foreach(var op in _supportedOptions)
				if (op.Validator != null && opts.ContainsKey(op.Key) && !op.Validator(opts[op.Key]))
					throw new Exception(string.Format("{0} is not a valid value for option {1}", opts[op.Key], op.Key));
		}

		private int AsInt(OptionItem o)
		{
			return int.Parse(m_opts.ContainsKey(o.Key) ? m_opts[o.Key] : o.DefaultValue);
		}

		private string AsString(OptionItem o)
		{
			return m_opts.ContainsKey(o.Key) ? m_opts[o.Key] : o.DefaultValue;
		}

		private bool AsBool(OptionItem o)
		{
			return bool.Parse(m_opts.ContainsKey(o.Key) ? m_opts[o.Key] : o.DefaultValue);
		}

		public int Columns { get { return AsInt(COLUMNS_OPT); } }
		public int FieldIndex { get { return AsInt(FIELD_IDX_OPT); } }
		public string ColumnSeperator { get { return AsString(COLUMN_SEP_OPT); } }
		public string FieldSeperator { get { return AsString(FIELD_SEP_OPT); } }
		public System.Text.RegularExpressions.Regex LineRegExp { get { return new System.Text.RegularExpressions.Regex(AsString(LINE_RX_OPT)); } }
		public string Inputfile { get { return AsString(INPUT_FILE_OPT); } }
		public string Outputfile { get { return AsString(OUPUT_FILE_OPT); } }
		public bool Verbose { get { return AsBool(VERBOSE_OPT); } }

	}
}

