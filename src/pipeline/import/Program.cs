using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameStack.Pipeline;
using Assimp;

namespace GameStack.Tools.Import {
	class MainClass {
		static readonly char[] SplitChar = new char[] { '=' };

		public static int Usage () {
			Console.WriteLine(string.Format(@"Usage:
    {0} <inputFile|inputFolder> <outputFolder> [PROPERTY=VAL]...",
				Assembly.GetExecutingAssembly().GetName().Name));
			return -1;
		}

		public static int Main (string[] args) {
			if (args.Length < 2) {
				return Usage();
			}
			var opts = new Dictionary<string,string>();
			for (var i = 2; i < args.Length; i++) {
				var parts = args[i].Split(SplitChar, 2);
				if (parts.Length < 2)
					return Usage();
				opts[parts[0]] = parts[1];
			}

			Assimp.Unmanaged.AssimpLibrary.Instance.LoadLibrary();
			if (Directory.Exists(args[0])) {
				try {
					processDirectoryRec(args[0], args[1], opts);
				} catch (Exception ex) {
					Console.WriteLine(ex.ToString());
					return -1;
				}
			}
			Assimp.Unmanaged.AssimpLibrary.Instance.FreeLibrary();

			return 0;
		}

		static void processDirectoryRec (string iDir, string oDir, Dictionary<string, string> opts) {
			var paths = Directory.GetFileSystemEntries(iDir)
				.Where(f => !Path.GetFileName(f).StartsWith(".") && !Path.GetExtension(f).EndsWith(".meta"))
				.ToArray();

			foreach (var path in paths) {
				Console.WriteLine("Processing: " + path);
				if (Directory.Exists(path) && string.IsNullOrEmpty(Path.GetExtension(path))) {
					var subDir = Path.GetFileName(path);
					processDirectoryRec(Path.Combine(iDir, subDir), Path.Combine(oDir, subDir), opts);
				} else
					ContentImporter.Process(path, oDir, opts);
			}
		}
	}
}
