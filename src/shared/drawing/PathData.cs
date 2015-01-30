using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Cairo;

namespace GameStack.Graphics {
	public class PathData {
		List<PathCommand> _commands;

		public PathData (string path, double offsetX = 0.0, double offsetY = 0.0) {
			_commands = this.ReadCommands(path, offsetX, offsetY).ToList();
		}

		public void Draw(Context ctx) {
			foreach (var cmd in _commands) {
				switch (cmd.Operation) {
				case PathOperation.MoveTo:
					ctx.MoveTo(cmd.Arg0);
					break;
				case PathOperation.RelMoveTo:
					ctx.RelMoveTo(cmd.Arg0.X, cmd.Arg0.Y);
					break;
				case PathOperation.LineTo:
					ctx.LineTo(cmd.Arg0);
					break;
				case PathOperation.RelLineTo:
					ctx.RelLineTo(cmd.Arg0.X, cmd.Arg0.Y);
					break;
				case PathOperation.CurveTo:
					ctx.CurveTo(cmd.Arg0, cmd.Arg1, cmd.Arg2);
					break;
				case PathOperation.RelCurveTo:
					ctx.RelCurveTo(cmd.Arg0.X, cmd.Arg0.Y, cmd.Arg1.X, cmd.Arg1.Y, cmd.Arg2.X, cmd.Arg2.Y);
					break;
				case PathOperation.ClosePath:
					ctx.ClosePath();
					break;
				}
			}
		}

		IEnumerable<PathCommand> ReadCommands(string path, double x, double y) {
			var ie = this.ReadTokens(path).GetEnumerator();
			var cmd = new PathCommand();
			cmd.Operation = PathOperation.MoveTo;
			while (ie.MoveNext()) {
				var cur = ie.Current;
				double? firstVal = null;
				if (cur[0] >= '0' && cur[0] <= '9' || cur[0] == '-' || cur[0] == '.') {
					double val;
					if (!double.TryParse(ie.Current, out val))
						throw new FormatException("Bad number `" + ie.Current + "'");
					firstVal = val;
				} else {
					var c = ie.Current[0];
					if (!(c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z'))
						throw new FormatException("Unexpected symbol `" + c + "'");
					switch (c) {
					case 'M':
						cmd.Operation = PathOperation.MoveTo;
						break;
					case 'm':
						cmd.Operation = PathOperation.RelMoveTo;
						break;
					case 'L':
						cmd.Operation = PathOperation.LineTo;
						break;
					case 'l':
						cmd.Operation = PathOperation.RelLineTo;
						break;
					case 'C':
						cmd.Operation = PathOperation.CurveTo;
						break;
					case 'c':
						cmd.Operation = PathOperation.RelCurveTo;
						break;
					case 'z':
						cmd.Operation = PathOperation.ClosePath;
						break;
					default:
						throw new NotImplementedException("Unsupported command '" + c + "'");
					}
				}
				switch (cmd.Operation) {
				case PathOperation.MoveTo:
				case PathOperation.LineTo:
					cmd.Arg0 = new PointD((firstVal ?? this.ReadNumber(ie)) + x, this.ReadNumber(ie) + y);
					break;
				case PathOperation.RelMoveTo:
				case PathOperation.RelLineTo:
					cmd.Arg0 = new PointD(firstVal ?? this.ReadNumber(ie), this.ReadNumber(ie));
					break;
				case PathOperation.CurveTo:
					cmd.Arg0 = new PointD((firstVal ?? this.ReadNumber(ie)) + x, this.ReadNumber(ie) + y);
					cmd.Arg1 = new PointD(this.ReadNumber(ie) + x, this.ReadNumber(ie) + y);
					cmd.Arg2 = new PointD(this.ReadNumber(ie) + x, this.ReadNumber(ie) + y);
					break;
				case PathOperation.RelCurveTo:
					cmd.Arg0 = new PointD(firstVal ?? this.ReadNumber(ie), this.ReadNumber(ie));
					cmd.Arg1 = new PointD(this.ReadNumber(ie), this.ReadNumber(ie));
					cmd.Arg2 = new PointD(this.ReadNumber(ie), this.ReadNumber(ie));
					break;
				}
				yield return cmd;
			}
		}

		double ReadNumber(IEnumerator<string> ie) {
			if (!ie.MoveNext())
				throw new FormatException("Unexpected end of data.");
			double val;
			if (!double.TryParse(ie.Current, out val))
				throw new FormatException("Bad number `" + ie.Current + "'");
			return val;
		}

		IEnumerable<string> ReadTokens(string data) {
			for (var i = 0; i < data.Length; i++) {
				var c = data[i];
				if (c == ' ' || c == ',')
					continue;
				else if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z') {
					yield return new string(c, 1);
				}
				else if (c >= '0' && c <= '9' || c == '-' || c == '.') {
					var idx = i;
					for (i++; i < data.Length; i++) {
						c = data[i];
						if (c != 'e' && c != '.' && c != '-' && (c < '0' || c > '9'))
							break;
					}
					yield return data.Substring(idx, i - idx);
				} else
					throw new FormatException("Unexpected symbol `" + c + "'");
			}
		}

		struct PathCommand {
			public PathOperation Operation;
			public PointD Arg0, Arg1, Arg2;
		}

		public enum PathOperation {
			MoveTo,
			RelMoveTo,
			LineTo,
			RelLineTo,
			CurveTo,
			RelCurveTo,
			ClosePath
		}
	}
}
