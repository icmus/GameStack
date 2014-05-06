using System;
using System.IO;
using SDL2;
using System.Security.Cryptography;

namespace GameStack {
	public static class Assets {
		const string AssetBasePath = "Assets/";
		public static string AppName { get; private set; }
		public static string OrgName { get; private set; }
		static string _userPath;
		static RijndaelManaged _rm = new RijndaelManaged();
		static byte[] _key, _iv;

		public static void SetKey (string key, string iv) {
			_key = Convert.FromBase64String(key);
			_iv = Convert.FromBase64String(iv);
		}

		public static Stream ResolveStream (string path) {
			var stream = File.OpenRead (Path.Combine(AssetBasePath, path));

			if (_key != null && _iv != null) {
				var ms = new MemoryStream();

				using (var cs = new CryptoStream(stream, _rm.CreateDecryptor(_key, _iv), CryptoStreamMode.Read)) {
					cs.CopyTo(ms);
				}
				ms.Position = 0;
				return ms;
			} else
				return stream;
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

		public static Stream ResolveAddonStream (string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read) {
			var stream = File.OpenRead(Path.Combine(_userPath, path));

			if (_key != null && _iv != null) {
				var ms = new MemoryStream();

				using (var cs = new CryptoStream(stream, _rm.CreateDecryptor(_key, _iv), CryptoStreamMode.Read)) {
					cs.CopyTo(ms);
				}
				ms.Position = 0;
				return ms;
			} else
				return stream;
		}
	}
}
