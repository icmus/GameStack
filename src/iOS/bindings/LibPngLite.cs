using System;
using System.Runtime.InteropServices;

namespace GameStack.Bindings {
	public class LibPngLite {
		#if __IOS__
		const string LibName = "__Internal";
		#else
		const string LibName = "pnglite";
		#endif

		public delegate int ReadCallback (IntPtr output, int size, int numel, ref PngDataCursor user_pointer);

		public delegate int WriteCallback (IntPtr input, int size, int numel, ref PngDataCursor user_pointer);

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Png {
			public IntPtr zs;
			public IntPtr read_fun;
			public IntPtr write_fun;
			public IntPtr user_pointer;
			public IntPtr png_data;
			public int png_datalen;
			public int width;
			public int height;
			public byte depth;
			public byte color_type;
			public byte compression_method;
			public byte filter_method;
			public byte interlace_method;
			public byte bpp;
			public IntPtr readbuf;
			public int readbuflen;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		unsafe public struct PngDataCursor {
			public int Dummy;
			public byte* Data;
			public int Length;
			public int Offset;

			public PngDataCursor(byte* data, int length) {
				this.Dummy = 0;
				this.Data = data;
				this.Length = length;
				this.Offset = 0;
			}
		}

		[DllImport(LibName)]
		public static extern int png_init (IntPtr pngalloc, IntPtr pngfree);

		[DllImport(LibName)]
		public static extern int png_open_read (ref Png png, ReadCallback read_fun, ref PngDataCursor user_pointer);

		[DllImport(LibName)]
		public static extern int png_open_write (ref Png png, WriteCallback write_fun, ref PngDataCursor user_pointer);

		[DllImport(LibName)]
		public static extern int png_print_info (ref Png png);

		[DllImport(LibName)]
		public static extern IntPtr png_error_string (int error);

		[DllImport(LibName)]
		public static extern int png_get_data (ref Png png, byte[] data);

		[DllImport(LibName)]
		public static extern int png_set_data (ref Png png, int width, int height, byte depth, int color, byte[] data);
	}
}
