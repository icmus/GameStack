using System;
using System.IO;
using System.Security.Cryptography;

namespace GameStack {
	public static class Assets {
		const string AssetBasePath = "Assets/";
		public static string AppName { get; private set; }
		public static string OrgName { get; private set; }
		static string _userPath;
		static RijndaelManaged _rm = new RijndaelManaged();
		static byte[] _key, _iv;

		internal static string ResolvePath (string path) {
			return Path.Combine(AssetBasePath, path);
		}

		public static void SetKey (string key, string iv) {
			_key = Convert.FromBase64String(key);
			_iv = Convert.FromBase64String(iv);
		}

		public static Stream ResolveStream (string path) {
			var stream = File.OpenRead(ResolvePath(path));

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
