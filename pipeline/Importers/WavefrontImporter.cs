using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace GameStack.Pipeline {
	[ContentType(".obj2", ".model")]
	public class WavefrontImporter : ContentImporter {
		static readonly char[] SplitChar = new char[] { ' ' };

		public override void Import (Stream input, Stream output, string filename) {
			var v = new List<Vector3>() { Vector3.Zero };
			var vn = new List<Vector3>() { Vector3.Zero };
			var vt = new List<Vector2>() { Vector2.Zero };

			var vertices = new List<Vertex>();
			var map = new Dictionary<ulong, int>();
			var faces = new List<int>();
			var groups = new List<string>();
			var offsets = new List<int>();
			var lengths = new List<int>();

			var reader = new StreamReader(input);
			string line;
			while ((line = reader.ReadLine()) != null) {
				var parts = line.Trim().Split(SplitChar, 2, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 1)
					continue;
				switch (parts[0]) {
					case "#":
						break;
					case "v":
						v.Add(ParseVector3(parts[1]));
						break;
					case "vn":
						vn.Add(ParseVector3(parts[1]));
						break;
					case "vt":
						vt.Add(ParseVector2(parts[1]));
						break;
					case "g":
						groups.Add(parts[1]);
						offsets.Add(faces.Count * sizeof(int));
						lengths.Add(0);
						break;
					case "f":
						if (groups.Count == 0) {
							// use a default group
							groups.Add("");
							offsets.Add(faces.Count * sizeof(int));
							lengths.Add(0);
						}
						var fparts = parts[1].Split(SplitChar, StringSplitOptions.RemoveEmptyEntries);
						foreach (var s in fparts) {
							var vparts = ParseVertex(s);
							ulong vid = ((ulong)vparts[2] << 32) | ((ulong)vparts[1] << 16) | (ulong)vparts[0];
							int idx = 0;
							if (map.ContainsKey(vid))
								idx = map[vid];
							else {
								var vertex = new Vertex();
								vertex.V = v[vparts[0]];
								vertex.VT = vt[vparts[1]];
								vertex.VN = vn[vparts[2]];
								idx = vertices.Count;
								vertices.Add(vertex);
								map.Add(vid, idx);
							}
							faces.Add(idx);
							lengths[lengths.Count - 1]++;
						}
						break;

					default:
						break;
				}
			}

			var writer = new BinaryWriter(output);
			writer.Write(vertices.Count * 8);
			foreach (var iv in vertices) {
				writer.Write(iv.V);
				writer.Write(iv.VN);
				writer.Write(iv.VT);
			}
			writer.Write(faces.Count);
			foreach (var ii in faces) {
				writer.Write(ii);
			}
			writer.Write(groups.Count);
			for (var i = 0; i < groups.Count; i++) {
				writer.Write(groups[i]);
				writer.Write(offsets[i]);
				writer.Write(lengths[i]);
			}
		}

		static Vector2 ParseVector2 (string s) {
			var floats = ParseFloatArray(s, 3);
			var v = new Vector2(floats[0], floats[1]);
			return floats[2] == 0f ? v : v / floats[2];
		}

		static Vector3 ParseVector3 (string s) {
			var floats = ParseFloatArray(s, 4);
			var v = new Vector3(floats[0], floats[1], floats[2]);
			return floats[3] == 0f ? v : v / floats[3];
		}

		static float[] ParseFloatArray (string s, int count) {
			var parts = s.Split(SplitChar, StringSplitOptions.RemoveEmptyEntries);
			var floats = new float[count];
			for (var i = 0; i < count && i < parts.Length; i++)
				float.TryParse(parts[i], out floats[i]);
			return floats;
		}

		static ushort[] ParseVertex (string s) {
			var parts = s.Split('/');
			var shorts = new ushort[3];
			for (var i = 0; i < parts.Length; i++)
				ushort.TryParse(parts[i], out shorts[i]);
			return shorts;
		}
	}
}
