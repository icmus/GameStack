using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace GameStack.Pipeline {
	[ContentType (".*", ".*")]
	public class BlobImporter : ContentImporter {
		public override void Import (Stream input, Stream output) {
			var buffer = new byte[1024 * 1024];
			int count;

			while ((count = input.Read (buffer, 0, buffer.Length)) > 0)
				output.Write (buffer, 0, count);
		}
	}
}
