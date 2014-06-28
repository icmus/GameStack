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
	public enum OpusError {
		OP_FALSE = -1,
		OP_HOLE = -3,
		OP_EREAD = -128,
		OP_EFAULT = -129,
		OP_EIMPL = -130,
		OP_EINVAL = -131,
		OP_ENOTFORMAT = -132,
		OP_EBADHEADER = -133,
		OP_EVERSION = -134,
		OP_EBADLINK = -137,
		OP_EBADTIMESTAMP = -139,
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int ReadFunc (IntPtr stream, IntPtr ptr, int nbytes);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int SeekFunc (IntPtr stream, long offset, SeekOrigin whence);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate long TellFunc (IntPtr stream);

	[StructLayout(LayoutKind.Sequential)]
	internal struct _OpusFileCallbacks {
		public IntPtr read;
		public IntPtr seek;
		public IntPtr tell;
		public IntPtr close;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct OpusHead {
		public int Version;
		public int ChannelCount;
		[MarshalAs(UnmanagedType.U4)]
		public long PreSkip;
		[MarshalAs(UnmanagedType.U4)]
		public int InputSampleRate;
		public int OutputGain;
		public int MappingFamily;
		public int StreamCount;
		public int CoupledCount;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
		public byte[] Mapping;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct _OpusTags {
		public IntPtr user_comments;
		public IntPtr comment_lengths;
		public int comments;
		public IntPtr vendor;
	}

	public class OpusTags {
		List<KeyValuePair<string, string>> _comments;
		string _vendor;

		internal OpusTags (_OpusTags opusTags) {
			var commentPtrs = new IntPtr[opusTags.comments];
			Marshal.Copy(opusTags.user_comments, commentPtrs, 0, opusTags.comments);
			var lengths = new int[opusTags.comments];
			Marshal.Copy(opusTags.comment_lengths, lengths, 0, opusTags.comments);

			byte[] buf;
			_comments = new List<KeyValuePair<string, string>>();
			for (int i = 0; i < opusTags.comments; i++) {
				buf = new byte[lengths[i]];
				Marshal.Copy(commentPtrs[i], buf, 0, lengths[i]);
				var str = System.Text.Encoding.UTF8.GetString(buf);
				var halves = str.Split(new[] { '=' }, 2);
				_comments.Add(new KeyValuePair<string, string>(halves[0], halves[1]));
			}

			int len = 0;
			while (Marshal.ReadByte(opusTags.vendor, len) != 0)
				len++;
			buf = new byte[len];
			Marshal.Copy(opusTags.vendor, buf, 0, len);
			_vendor = System.Text.Encoding.UTF8.GetString(buf);
		}

		public List<KeyValuePair<string, string>> Comments { get { return _comments; } }
		public string Vendor { get { return _vendor; } }
	}

	public class OpusException : Exception {
		public OpusException (OpusError error) : base(error.ToString()) {
		}
	}
}
