using System;
using System.IO;
using System.Runtime.InteropServices;


namespace GameStack.Pipeline
{
	public static class GraphicsMagick
	{
		public enum Filter
		{
			Undefined,
			Point,
			Box,
			Triangle,
			Hermite,
			Hanning,
			Hamming,
			Blackman,
			Gaussian,
			Quadratic,
			Cubic,
			Catrom,
			Mitchell,
			Lanczos,
			Bessel,
			Sinc,
		};

		[DllImport("libGraphicsMagickWand.so", EntryPoint = "MagickResizeImage")]
		public static extern bool ResizeImage(IntPtr mgck_wand, IntPtr columns, IntPtr rows, Filter filter_type, double blur);

		[DllImport("libGraphicsMagickWand.so", EntryPoint = "NewMagickWand")]
		public static extern IntPtr NewWand();

		[DllImport("libGraphicsMagickWand.so", EntryPoint = "DestroyMagickWand")]
		public static extern IntPtr DestroyWand(IntPtr wand);

		[DllImport("libGraphicsMagickWand.so", EntryPoint = "MagickWriteImageBlob")]
		public static extern IntPtr WriteImageBlob(IntPtr wand, out IntPtr length);

		[DllImport("libGraphicsMagickWand.so", EntryPoint = "MagickReadImageBlob")]
		public static extern bool ReadImageBlob(IntPtr wand, IntPtr blob, IntPtr length);

		[DllImport("libGraphicsMagickWand.so", EntryPoint = "MagickRelinquishMemory")]
		public static extern IntPtr RelinquishMemory(IntPtr resource);

		[DllImport("libGraphicsMagickWand.so", EntryPoint = "MagickGetImageWidth")]
		public static extern long GetWidth(IntPtr wand);

		[DllImport("libGraphicsMagickWand.so", EntryPoint = "MagickGetImageHeight")]
		public static extern long GetHeight(IntPtr wand);

		public static bool ReadImageBlob(IntPtr wand, Stream blobStream)
		{
			var blob = new byte[blobStream.Length];
			var ms = new MemoryStream(blob);
			blobStream.CopyTo(ms);
			ms.Dispose();

			GCHandle pinnedArray = GCHandle.Alloc(blob, GCHandleType.Pinned);
			IntPtr pointer = pinnedArray.AddrOfPinnedObject();

			bool bRetv = ReadImageBlob(wand, pointer, (IntPtr)blob.Length);

			pinnedArray.Free();

			return bRetv;
		}

		public static byte[] WriteImageBlob(IntPtr wand)
		{
			IntPtr len;
			IntPtr buf = WriteImageBlob(wand, out len);

			var dest = new byte[len.ToInt32()];
			Marshal.Copy(buf, dest, 0, len.ToInt32());
			RelinquishMemory(buf);

			return dest;
		}

	}
}
