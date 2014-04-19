using System;
using System.IO;

namespace GameStack {
	public static class Assets {
		const string AssetBasePath = "Assets/";

		public static Stream ResolveStream (string path) {
			return File.OpenRead (ResolvePath (path));
		}

		public static string ResolvePath (string path) {
			return Path.Combine (AssetBasePath, path);
		}
	}
}
