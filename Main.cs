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
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Columns
{
	class MainClass
	{

		public static void PrintUsage()
		{
			Console.WriteLine("");
			Console.WriteLine("columns - A tool for formatting data in columns, compatible with gnuplot");
			Console.WriteLine("");
			Console.WriteLine("Usage: ");
			Console.WriteLine("columns [--option=value]");
			Console.WriteLine("");
			Console.WriteLine("If no options are given, the program will read from stdin and write to stdout");
			Console.WriteLine("");
			Console.WriteLine("Options:");
			foreach(var o in Options.SupportedOptions)
			{
				Console.WriteLine("\t--{0}: {1}", o.Key, o.Description);
				if (!string.IsNullOrEmpty(o.DefaultValue))
					Console.WriteLine("\t\tDefault: {0}", o.DefaultValue.Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r").Replace(" ", "<space>"));
			}
		}

		public static void Main(string[] _args)
		{
			if (_args.Contains("help", System.StringComparer.InvariantCultureIgnoreCase) || 
			    _args.Contains("about", System.StringComparer.InvariantCultureIgnoreCase) || 
			    _args.Contains("-h", System.StringComparer.InvariantCultureIgnoreCase) || 
			    _args.Contains("help", System.StringComparer.InvariantCultureIgnoreCase) || 
			    _args.Contains("/h", System.StringComparer.InvariantCultureIgnoreCase))
			{
				PrintUsage();
				return;
			}

			List<string> args = new List<string>(_args);
			Options options;

			try
			{
				options = new Options(Options.ParseCommandLine(args));
			} 
			catch (Exception ex)
			{
				using(var es = new StreamWriter(Console.OpenStandardError()))
					es.WriteLine("Error: " + ex.Message);

				return;
			}


			long ix = 0;
			System.Text.RegularExpressions.Regex lineregexp = options.LineRegExp;
			char[] fieldseps = options.FieldSeperator.ToCharArray();
			int fieldIndex = options.FieldIndex;
			int columns = options.Columns;
			string columnseperator = options.ColumnSeperator;

			try
			{
				using(StreamReader sr = new StreamReader(string.IsNullOrEmpty(options.Inputfile) ? Console.OpenStandardInput() : System.IO.File.OpenRead(options.Inputfile), System.Text.Encoding.UTF8, true))
				using(StreamWriter sw = new StreamWriter(string.IsNullOrEmpty(options.Outputfile) ? Console.OpenStandardOutput() : System.IO.File.OpenWrite(options.Outputfile), sr.CurrentEncoding))
				{
					string line;
					while ((line = sr.ReadLine()) != null)
					{
						if (!lineregexp.Match(line).Success)
							continue;

						string v;

						if (fieldIndex == 0)
						{
							v = line;
						}
						else
						{
							string[] fields = line.Split(fieldseps);
							v = "";

							if (fieldIndex == -1 || fieldIndex == -2)
							{
								string first = null;
								double d;
								foreach(var sx in fields)
									if (double.TryParse(sx, out d) || double.TryParse(sx, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
									{
										if (first == null)
											first = sx;
										v = sx;
									}

								if (fieldIndex == -1)
									v = first;

							}
							else if (fieldIndex < fields.Length && fieldIndex >= 0)
								v = fields[options.FieldIndex];
						}

						sw.Write(v);
						ix++;
						if (ix >= columns)
						{
							ix = 0;
							sw.WriteLine();
						}
						else
						{
							sw.Write(columnseperator);
						}
					}
				}
			}
			catch (Exception ex)
			{
				using(var es = new StreamWriter(Console.OpenStandardError()))
					if (options.Verbose)
						es.WriteLine("Error: " + ex.ToString());
					else
						es.WriteLine("Error: " + ex.Message);

			}
		}
	}
}