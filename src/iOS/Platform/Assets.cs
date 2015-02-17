using System;
using System.IO;
using System.Security.Cryptography;
using MonoTouch;
using MonoTouch.Foundation;

namespace GameStack {
	public static class Assets {
		static readonly string AssetBasePath = Path.Combine(NSBundle.MainBundle.BundleUrl.Path, "assets");
        static readonly string AddonsBasePath = Path.Combine(NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path, "Addons");
		public static string AppName { get; private set; }
		public static string OrgName { get; private set; }
		static string _userPath;
		static float _pixelScale = 1;
		static RijndaelManaged _rm = new RijndaelManaged();
		static byte[] _key, _iv;

		public static float PixelScale { get { return _pixelScale; } set { _pixelScale = value; } }

		internal static string ResolvePath (string path) {
			return FindScaledAsset(Path.Combine(AssetBasePath, path));
		}

		public static void SetKey (string key, string iv) {
			_key = Convert.FromBase64String(key);
			_iv = Convert.FromBase64String(iv);
		}

		static string FindScaledAsset (string fullPath) {
			if (_pixelScale == 1)
				return fullPath;

			var scaledPath = Path.Combine(
				Path.GetDirectoryName(fullPath),
				string.Format("{0}@{1}x{2}", Path.GetFileNameWithoutExtension(fullPath), _pixelScale, Path.GetExtension(fullPath))
			);

			return File.Exists(scaledPath) ? scaledPath : fullPath;
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

            _userPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
		}

		public static Stream ResolveUserStream (string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite) {
			if (_userPath == null)
				throw new InvalidOperationException("Must call SetAppInfo first!");

			return File.Open(FindScaledAsset(Path.Combine(_userPath, path)), mode, access);
		}

		public static string ResolveUserPath (string path = "") {
			if (_userPath == null)
				throw new InvalidOperationException("Must call SetAppInfo first!");

			return FindScaledAsset(Path.Combine(_userPath, path));
		}

		public static Stream ResolveAddonStream (string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read) {
			var stream = File.Open(FindScaledAsset(Path.Combine(AddonsBasePath, path)), mode, access);

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

		public static string ResolveAddonPath (string path = "") {
			return FindScaledAsset(Path.Combine(AddonsBasePath, path));
		}
	}
}
