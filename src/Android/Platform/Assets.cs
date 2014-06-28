using System;
using System.IO;
using Android.Content;
using Android.Content.Res;
using Java.IO;
using File = System.IO.File;
using Android.App;

namespace GameStack {
	public static class Assets {
		const int BufSize = 4096;

		public static string AppName { get; private set; }

		public static string OrgName { get; private set; }

		static string _userPath;

		const string AssetBasePath = "assets/";

		public static Stream ResolveStream (string path) {
			var s = Application.Context.Assets.Open(AssetBasePath + path);
			var ms = new MemoryStream();
			s.CopyTo(ms);
			ms.Position = 0;
			return ms;
		}

		public static AssetFileDescriptor ResolveFd (string path) {
			return Application.Context.Assets.OpenFd(AssetBasePath + path);
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
