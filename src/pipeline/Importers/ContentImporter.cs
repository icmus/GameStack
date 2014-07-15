using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Assimp;
using Mono.Unix;

namespace GameStack.Pipeline {
	public abstract class ContentImporter {
		static Dictionary<string, Type> _importers;
		// Find all classes marked with [ContentImporter] and catalog them by extension.
		static ContentImporter () {
			_importers = new Dictionary<string, Type>();
			foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(ContentImporter).IsAssignableFrom(t) && !t.IsAbstract)) {
				var attr = Attribute.GetCustomAttribute(type, typeof(ContentTypeAttribute)) as ContentTypeAttribute;
				if (attr != null) {
					foreach (var extension in attr.InExtension.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) {
						_importers.Add(extension, type);
					}
				}
			}
		}

		public static void Process (string inputFile, string outputFolder, IDictionary<string,string> opts) {
			if (!File.Exists(inputFile) && !Directory.Exists(inputFile))
				throw new ArgumentException("Input file does not exist: " + inputFile);
			if (!Directory.Exists(outputFolder))
				Directory.CreateDirectory(outputFolder);

			inputFile = Path.GetFullPath(inputFile);
			outputFolder = Path.GetFullPath(outputFolder);

			var extension = Path.GetExtension(inputFile).ToLower();
			var baseName = Path.GetFileNameWithoutExtension(inputFile);

			Type type;
			_importers.TryGetValue(extension, out type);
			if (type == null) {
				var attr = File.GetAttributes(inputFile);
				if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
					Console.WriteLine("Unrecognized folder extension " + Path.GetExtension(inputFile) + ", skipping.");
					return;
				}
				Console.WriteLine("No custom importer for {0}, copying as blob.", extension);

				string outputFile = Path.Combine(outputFolder, baseName + extension);
				if (File.Exists(outputFile) && File.GetLastWriteTime(outputFile) > File.GetLastWriteTime(inputFile))
					return;

				if (opts.ContainsKey("Key") && opts.ContainsKey("IV")) {
					var key = Convert.FromBase64String(opts["Key"]);
					var iv = Convert.FromBase64String(opts["IV"]);

					var rm = new RijndaelManaged();
					using (var oStream = new CryptoStream(new FileStream(outputFile, FileMode.Create, FileAccess.Write), rm.CreateEncryptor(key, iv), CryptoStreamMode.Write)) {
						using (var iStream = File.OpenRead(inputFile)) {
							iStream.CopyTo(oStream);
						}
					}
				} else {
					File.Copy(inputFile, outputFile, true);
					var ufi = new UnixFileInfo(outputFile);
					ufi.FileAccessPermissions |= FileAccessPermissions.UserWrite;
				}
			} else {
				var attr = (ContentTypeAttribute)Attribute.GetCustomAttribute(type, typeof(ContentTypeAttribute));

				var importer = (ContentImporter)Activator.CreateInstance(type);
				foreach (var kvp in opts) {
					if (kvp.Key == "Key" || kvp.Key == "IV")
						continue;

					var prop = type.GetProperty(kvp.Key, BindingFlags.IgnoreCase | BindingFlags.Public);
					if (prop == null)
						throw new ArgumentException("No such property found on importer: " + kvp.Key);
					prop.SetValue(importer, Convert.ChangeType(kvp.Value, prop.PropertyType), null);
				}

				string outputFile = Path.Combine(outputFolder, baseName + (attr.OutExtension == ".*" ? extension : attr.OutExtension));
				if (File.Exists(outputFile) && File.GetLastWriteTime(outputFile) > File.GetLastWriteTime(inputFile))
					return;

				var oldWorkingDir = Environment.CurrentDirectory;
				using (FileStream ofStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write)) {
					Stream oStream = ofStream;
					if (opts.ContainsKey("Key") && opts.ContainsKey("IV")) {
						var key = Convert.FromBase64String(opts["Key"]);
						var iv = Convert.FromBase64String(opts["IV"]);

						var rm = new RijndaelManaged();
						oStream = new CryptoStream(ofStream, rm.CreateEncryptor(key, iv), CryptoStreamMode.Write);
					}

					if (Directory.Exists(inputFile)) {
						Environment.CurrentDirectory = inputFile;
						importer.Import(inputFile, oStream);
					} else {
						using (FileStream iStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read)) {
							var dir = Path.GetDirectoryName(inputFile);
							if (dir != string.Empty)
								Environment.CurrentDirectory = Path.GetDirectoryName(inputFile);
							importer.Import(iStream, oStream, Path.GetFileName(inputFile));
						}
					}

					if (oStream is CryptoStream)
						((CryptoStream)oStream).FlushFinalBlock();
				}
				Environment.CurrentDirectory = oldWorkingDir;
			}
		}

		public virtual void Import (Stream iStream, Stream oStream, string filepath) {
			throw new NotImplementedException();
		}
		public virtual void Import (string iDir, Stream oStream) {
			throw new NotImplementedException();
		}
	}
}
