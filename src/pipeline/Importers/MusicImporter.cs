using System;


namespace GameStack.Pipeline
{
	[ContentType (".mp3", ".*")]
	public class MusicImporter : ContentImporter {
		public override void Import (System.IO.Stream iStream, System.IO.Stream oStream, string filename)
		{
			iStream.CopyTo(oStream);
		}
	}
}
