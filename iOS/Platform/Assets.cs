using System;
using System.IO;

namespace GameStack {
	public static class Assets {
		const string AssetBasePath = "Assets/";
		public static string AppName { get; private set; }
		public static string OrgName { get; private set; }
		static string _userPath;

		internal static string ResolvePath (string path) {
			return Path.Combine(AssetBasePath, path);
		}

		public static Stream ResolveStream (string path) {
			return File.OpenRead(ResolvePath(path));
		}

		public static void SetAppInfo (string appName, string orgName = "") {
			AppName = appName;
			OrgName = orgName;

			_userPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		}

		public static Stream ResolveUserStream (string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite) {
			if (_userPath == null)
				throw new InvalidOperationException("Must call SetAppInfo first!");

			return File.Open(Path.Combine(_userPath, path), mode, access);
		}

		public static string ResolveUserPath (string path = "") {
			if (_userPath == null)
				throw new InvalidOperationException("Must call SetAppInfo first!");

			return Path.Combine(_userPath, path);
		}
	}
}
