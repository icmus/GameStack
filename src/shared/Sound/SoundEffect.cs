using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using GameStack.Content;
using OpusfileSharp;

namespace GameStack {
	public class SoundEffect : IDisposable {
		int _buffer;
		SfxMetadata _md;
		bool _isDisposed;

		public SoundEffect (string path)
			: this(Assets.ResolveStream(path), Path.GetExtension(path) == ".opus") {
		}

		public SoundEffect (Stream inputStream, bool isOpus = false) {
			byte[] pcmData = null;

			if (isOpus) {
				using (var opusFile = new OggOpusFile(inputStream)) {
					if (opusFile.LinkCount > 1)
						throw new NotSupportedException("Opus files with multiple links are not supported.");

					_md = new SfxMetadata() {
						Bits = 16,
						Rate = 48000,
						Channels = opusFile.GetChannelCount(),
						Length = (int)opusFile.GetPcmTotal(),
					};

					int valuesRead;
					pcmData = new byte[sizeof(short) * _md.Channels * _md.Length];
					short[] readBuffer = new short[5760 * 2];
					using (var dataWriter = new BinaryWriter(new MemoryStream(pcmData))) {
						while ((valuesRead = opusFile.Read(readBuffer, 0, readBuffer.Length) * _md.Channels) > 0) {
							for (int i = 0; i < valuesRead; i++)
								dataWriter.Write(readBuffer[i]);
						}
					}
				}
			} else {
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
			if (_isDisposed)
				return;
			_isDisposed = true;

			AL.DeleteBuffer(_buffer);
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
