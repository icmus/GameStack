using System;
using System.IO;
using SDL2;

namespace GameStack {
	public static class Assets {
		const string AssetBasePath = "Assets/";
		public static string AppName { get; private set; }
		public static string OrgName { get; private set; }
		static string _userPath;

		public static Stream ResolveStream (string path) {
			return File.OpenRead (Path.Combine(AssetBasePath, path));
		}

		public static void SetAppInfo (string appName, string orgName = "") {
			AppName = appName;
			OrgName = orgName;

			_userPath = SDL2.SDL.SDL_GetPrefPath(OrgName, AppName);
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
