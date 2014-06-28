using System;
using System.IO;
using Newtonsoft.Json;

namespace GameStack.Pipeline {
	public static class JsonSerializerExtensions {
		public static string Serialize (this JsonSerializer ser, object value) {
			var writer = new StringWriter ();
			ser.Serialize (writer, value);
			writer.Flush ();
			return writer.ToString ();
		}

		public static T Deserialize<T> (this JsonSerializer ser, string str) {
			var reader = new StringReader (str);
			return (T)ser.Deserialize (reader, typeof(T));
		}
	}
}
