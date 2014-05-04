using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using GameStack.Content;

namespace GameStack {
	public class SoundEffect : IDisposable {
		int _buffer;
		SfxMetadata _md;

		public SoundEffect (string path)
			: this(Assets.ResolveStream(path)) {
		}

		public SoundEffect (Stream inputStream) {
			byte[] pcmData = null;

			using (var s = inputStream) {
				var tr = new TarReader (s);
				while (tr.MoveNext (false)) {
					switch (tr.FileInfo.FileName) {
						case "sound.bin":
							var bytes = new byte[tr.FileInfo.SizeInBytes];
							using (var ms = new MemoryStream(bytes)) {
								tr.Read (ms);
								ms.Position = 0;
								using (var br = new BinaryReader(ms)) {
									_md = SfxMetadata.Read (br);
								}
							}
							break;
						case "sound.pcm":
							pcmData = new byte[tr.FileInfo.SizeInBytes];
							tr.Read (new MemoryStream (pcmData));
							break;
						default:
							throw new ContentException ("Unrecognized sound file " + tr.FileInfo.FileName);
					}
				}
			}

			ALFormat format;
			switch (_md.Channels) {
				case 1:
					switch (_md.Bits) {
						case 8:
							format = ALFormat.Mono8;
							break;
						case 16:
							format = ALFormat.Mono16;
							break;
						case 32:
							format = ALFormat.MonoFloat32Ext;
							break;
						default:
							throw new NotSupportedException ("Sounds must be 8, 16, or 32 bit.");
					}
					break;
				case 2:
					switch (_md.Bits) {
						case 8:
							format = ALFormat.Stereo8;
							break;
						case 16:
							format = ALFormat.Stereo16;
							break;
						case 32:
							format = ALFormat.StereoFloat32Ext;
							break;
						default:
							throw new NotSupportedException ("Sounds must be 8, 16, or 32 bit.");
					}
					break;
				default:
					throw new NotSupportedException ("Sound effects must be mono or stereo.");
			}

			_buffer = AL.GenBuffer ();
			AL.BufferData (_buffer, format, pcmData, pcmData.Length, _md.Rate);
		}

		internal int Buffer {
			get { return _buffer; }
		}

		public float Length { get { return (float)_md.Length / (float)_md.Rate; } }

		public void Dispose () {
			AL.GetError();
			AL.DeleteBuffer (_buffer);
			Sounds.CheckError();
		}

		class SfxMetadata {
			public int Bits;
			public int Rate;
			public int Channels;
			public int Length;

			public static SfxMetadata Read (BinaryReader br) {
				var result = new SfxMetadata ();
				result.Bits = br.ReadInt32 ();
				result.Rate = br.ReadInt32 ();
				result.Channels = br.ReadInt32 ();
				result.Length = br.ReadInt32 ();
				return result;
			}
		}
	}
}
