using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using GameStack.Pipeline.Tar;

namespace GameStack.Pipeline {
	[ContentType (".wav", ".sfx")]
	public class SoundEffectImporter : ContentImporter {
		const int RIFF = 0x46464952;
		const int WAVE = 0x45564157;
		const int FMT_ = 0x20746d66;
		const int DATA = 0x61746164;
		const int WAVE_FORMAT_PCM = 0x0001;
		const int WAVE_FORMAT_IEEE_FLOAT = 0x0003;

		public override void Import (Stream input, Stream output, string filename) {
			var reader = new BinaryReader (input);

			int chunkID = reader.ReadInt32 ();
			if (chunkID != RIFF)
				throw new InvalidDataException ();
			reader.ReadInt32 (); // fileSize
			int riffType = reader.ReadInt32 ();
			if (riffType != WAVE)
				throw new InvalidDataException ();
			while (reader.ReadInt32 () != FMT_) {
				var dummy = reader.ReadInt32 ();
				reader.ReadBytes (dummy);
			}
			int fmtSize = reader.ReadInt32 ();
			int fmtCode = reader.ReadInt16 ();
			int channels = reader.ReadInt16 ();
			int sampleRate = reader.ReadInt32 ();
			reader.ReadInt32 (); // fmtAvgBPS
			reader.ReadInt16 (); // fmtBlockAlign
			int bitDepth = reader.ReadInt16 ();

			if (fmtSize == 18) {
				int fmtExtraSize = reader.ReadInt16 ();
				reader.ReadBytes (fmtExtraSize);
			}

			while (reader.ReadInt32 () != DATA) {
				var dummy = reader.ReadInt32 ();
				reader.ReadBytes (dummy);
			}
			int dataSize = reader.ReadInt32 ();

			byte[] byteArray = reader.ReadBytes (dataSize);

			if (fmtCode != WAVE_FORMAT_PCM && fmtCode != WAVE_FORMAT_IEEE_FLOAT)
				throw new NotSupportedException ("Wave files must be PCM or IEEE_FLOAT format.");
			var md = new SfxMetadata () {
				Bits = (fmtCode == WAVE_FORMAT_IEEE_FLOAT) ? 32 : bitDepth,
				Rate = sampleRate,
				Channels = channels,
				Length = dataSize / (bitDepth / 8) / channels,
			};

			using (var tw = new TarWriter(output)) {
				using (var ms = new MemoryStream()) {
					using (var bw = new BinaryWriter(ms)) {
						md.Write (bw);
						ms.Position = 0;
						tw.Write (ms, ms.Length, "sound.bin");
					}
				}
				tw.Write (new MemoryStream (byteArray), byteArray.Length, "sound.pcm");
			}
		}
	}

	class SfxMetadata {
		public int Bits;
		public int Rate;
		public int Channels;
		public int Length;

		public void Write (BinaryWriter bw) {
			bw.Write (this.Bits);
			bw.Write (this.Rate);
			bw.Write (this.Channels);
			bw.Write (this.Length);
		}
	}
}
