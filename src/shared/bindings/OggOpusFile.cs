/* OpusfileSharp - A C# wrapper for libopusfile
 * 
 * Copyright (c) 2013 Jameson Ernst
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;

namespace OpusfileSharp {
	public class OggOpusFile : IDisposable {
		#if __IOS__
		const string LibName = "__Internal";
		#else
		const string LibName = "opusfile";
		#endif

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr op_open_callbacks (IntPtr source, ref _OpusFileCallbacks cb, IntPtr initial_data, IntPtr initial_bytes, out OpusError error);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern void op_free (IntPtr of);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern int op_link_count (IntPtr of);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern uint op_serialno (IntPtr of, int li);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern int op_channel_count (IntPtr of, int li);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern long op_raw_total (IntPtr of, int li);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern long op_pcm_total (IntPtr of, int li);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr op_head (IntPtr of, int li);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr op_tags (IntPtr of, int li);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern int op_current_link (IntPtr of);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern int op_bitrate (IntPtr of, int li);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern int op_bitrate_instant (IntPtr of);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern long op_pcm_tell (IntPtr of);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern int op_read (IntPtr of, IntPtr pcm, int buf_size, out int li);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern int op_read_float (IntPtr of, IntPtr pcm, int buf_size, out int li);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern int op_read_stereo (IntPtr of, IntPtr pcm, int buf_size);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern int op_read_float_stereo (IntPtr of, IntPtr pcm, int buf_size);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern int op_raw_seek (IntPtr of, long byte_offset);
		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		static extern int op_pcm_seek (IntPtr of, long pcm_offset);

		static Dictionary<IntPtr, OggOpusFile> _streams = new Dictionary<IntPtr, OggOpusFile>();
		static int _sHandle = 0;

		ReadFunc _read;
		SeekFunc _seek;
		TellFunc _tell;

		IntPtr _opusFile, _handle;
		Stream _sourceStream;
		bool _leaveOpen;
		byte[] _buffer;

		public OggOpusFile (Stream sourceStream, bool leaveOpen = false) {
			if (sourceStream == null || !sourceStream.CanRead)
				throw new ArgumentException("fileStream must be a readable stream.");
			_sourceStream = sourceStream;
			_leaveOpen = leaveOpen;

			_read = new ReadFunc(ReadCallback);
			_seek = new SeekFunc(SeekCallback);
			_tell = new TellFunc(TellCallback);

			Console.WriteLine(_sourceStream.CanSeek);
			var cb = new _OpusFileCallbacks() {
				read = Marshal.GetFunctionPointerForDelegate(_read),
				seek = _sourceStream.CanSeek ? Marshal.GetFunctionPointerForDelegate(_seek) : IntPtr.Zero,
				tell = _sourceStream.CanSeek ? Marshal.GetFunctionPointerForDelegate(_tell) : IntPtr.Zero,
				close = IntPtr.Zero,
			};

			lock (_streams) {
				_handle = (IntPtr)_sHandle++;
				_streams.Add(_handle, this);
			}

			OpusError error;
			_opusFile = op_open_callbacks(_handle, ref cb, IntPtr.Zero, IntPtr.Zero, out error);
			if (_opusFile == IntPtr.Zero)
				throw new OpusException(error);
		}

		public Stream SourceStream { get { return _sourceStream; } }

		public int LinkCount {
			get {
				return op_link_count(_opusFile);
			}
		}

		public int CurrentLink {
			get {
				int result = op_current_link(_opusFile);
				if (result < 0)
					throw new OpusException((OpusError)result);
				else
					return result;
			}
		}

		public int BitrateInstant {
			get {
				int result = op_bitrate_instant(_opusFile);
				if (result < 0)
					throw new OpusException((OpusError)result);
				else
					return result;
			}
		}

		public long PcmTell {
			get {
				long result = op_pcm_tell(_opusFile);
				if (result < 0)
					throw new OpusException((OpusError)result);
				else
					return result;
			}
		}


		#if __IOS__
		[MonoTouch.MonoPInvokeCallback(typeof(ReadFunc))]
		#endif
		static int ReadCallback (IntPtr stream, IntPtr ptr, int nbytes) {
			OggOpusFile instance;
			var found = false;
			lock (_streams) {
				found = _streams.TryGetValue(stream, out instance);
			}
			return found ? instance.ReadCallbackImpl(stream, ptr, nbytes) : -1;
		}

		int ReadCallbackImpl (IntPtr stream, IntPtr ptr, int nbytes) {
			try {
				if (_buffer == null || _buffer.Length < nbytes)
					_buffer = new byte[nbytes];
				int bytesRead = _sourceStream.Read(_buffer, 0, nbytes);
				Marshal.Copy(_buffer, 0, ptr, nbytes);
				return bytesRead;
			} catch {
				return -1;
			}
		}

		#if __IOS__
		[MonoTouch.MonoPInvokeCallback(typeof(SeekFunc))]
		#endif
		static int SeekCallback (IntPtr stream, long offset, SeekOrigin whence) {
			OggOpusFile instance;
			var found = false;
			lock (_streams) {
				found = _streams.TryGetValue(stream, out instance);
			}
			return found ? instance.SeekCallbackImpl(stream, offset, whence) : -1;
		}

		int SeekCallbackImpl (IntPtr stream, long offset, SeekOrigin whence) {
			try {
				_sourceStream.Seek(offset, whence);
				return 0;
			} catch {
				return -1;
			}
		}

		#if __IOS__
		[MonoTouch.MonoPInvokeCallback(typeof(TellFunc))]
		#endif
		static long TellCallback (IntPtr stream) {
			OggOpusFile instance;
			var found = false;
			lock (_streams) {
				found = _streams.TryGetValue(stream, out instance);
			}
			return found ? instance.TellCallbackImpl(stream) : -1;
		}

		long TellCallbackImpl (IntPtr stream) {
			try {
				return _sourceStream.Position;
			} catch {
				return -1;
			}
		}

		public int GetSerialNumber (int linkIndex = -1) {
			return (int)op_serialno(_opusFile, linkIndex);
		}

		public int GetChannelCount (int linkIndex = -1) {
			return op_channel_count(_opusFile, linkIndex);
		}

		public long GetRawTotal (int linkIndex = -1) {
			long result = op_raw_total(_opusFile, linkIndex);
			if (result < 0)
				throw new OpusException((OpusError)result);
			else
				return result;
		}

		public long GetPcmTotal (int linkIndex = -1) {
			long result = op_pcm_total(_opusFile, linkIndex);
			if (result < 0)
				throw new OpusException((OpusError)result);
			else
				return result;
		}

		public OpusHead GetHeader (int linkIndex = -1) {
			return (OpusHead)Marshal.PtrToStructure(op_head(_opusFile, linkIndex), typeof(OpusHead));
		}

		public OpusTags GetTags (int linkIndex = -1) {
			return new OpusTags((_OpusTags)Marshal.PtrToStructure(op_tags(_opusFile, linkIndex), typeof(_OpusTags)));
		}

		public int GetBitrate (int linkIndex = -1) {
			int result = op_bitrate(_opusFile, linkIndex);
			if (result < 0)
				throw new OpusException((OpusError)result);
			else
				return result;
		}

		public unsafe int Read (short[] pcm, int offset, int count, out int linkIndex) {
			if (offset + count > pcm.Length)
				throw new ArgumentException("Offset + count is larger than pcm.Length.", "count");

			int result;
			fixed (short* ptr = &pcm[offset]) {
				result = op_read(_opusFile, (IntPtr)ptr, count, out linkIndex);
			}
			if (result < 0)
				throw new OpusException((OpusError)result);
			else
				return result;
		}

		public int Read (short[] pcm, int offset, int count) {
			int unused;
			return Read (pcm, offset, count, out unused);
		}

		public unsafe int ReadStereo (short[] pcm, int offset, int count) {
			if (offset + count > pcm.Length)
				throw new ArgumentException("Offset + count is larger than pcm.Length.", "count");

			int result;
			fixed (short* ptr = &pcm[offset]) {
				result = op_read_stereo(_opusFile, (IntPtr)ptr, count);
			}
			if (result < 0)
				throw new OpusException((OpusError)result);
			else
				return result;
		}

		public unsafe int ReadFloat (float[] pcm, int offset, int count, out int linkIndex) {
			if (offset + count > pcm.Length)
				throw new ArgumentException("Offset + count is larger than pcm.Length.", "count");

			int result;
			fixed (float* ptr = &pcm[offset]) {
				result = op_read_float(_opusFile, (IntPtr)ptr, count, out linkIndex);
			}
			if (result < 0)
				throw new OpusException((OpusError)result);
			else
				return result;
		}

		public int ReadFloat (float[] pcm, int offset, int count) {
			int unused;
			return ReadFloat (pcm, offset, count, out unused);
		}

		public unsafe int ReadFloatStereo (float[] pcm, int offset, int count) {
			if (offset + count > pcm.Length)
				throw new ArgumentException("Offset + count is larger than pcm.Length.", "count");

			int result;
			fixed (float* ptr = &pcm[offset]) {
				result = op_read_float_stereo(_opusFile, (IntPtr)ptr, count);
			}
			if (result < 0)
				throw new OpusException((OpusError)result);
			else
				return result;
		}

		public void RawSeek (long byteOffset) {
			int result = op_raw_seek(_opusFile, byteOffset);
			if (result < 0)
				throw new OpusException((OpusError)result);
		}

		public void PcmSeek (long pcmOffset) {
			int result = op_pcm_seek(_opusFile, pcmOffset);
			if (result < 0)
				throw new OpusException((OpusError)result);
		}

		public void Dispose () {
			if (_opusFile != IntPtr.Zero) {
				op_free(_opusFile);
				lock (_streams) {
					_streams.Remove(_opusFile);
				}
				_opusFile = IntPtr.Zero;
			}

			if (!_leaveOpen)
				_sourceStream.Dispose();
		}

#if DEBUG
		~OggOpusFile () {
			System.Diagnostics.Debug.WriteLineIf(
				_opusFile != IntPtr.Zero,
				string.Format("Undisposed {0} detected", this.GetType().ToString())
			);
		}
#endif
	}
}
