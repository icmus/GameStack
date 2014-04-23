using System;
using System.Runtime.InteropServices;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;

[assembly: LinkerSafe]
[assembly: LinkWith ("liblodepng.a", LinkTarget.ArmV7 | LinkTarget.Simulator, ForceLoad = true)]

namespace GameStack.Bindings {
	public class LibLodePng {
		[DllImport ("__Internal", EntryPoint = "lodepng_decode32")]
		public static extern int Decode32 (out IntPtr image, out int width, out int height, byte[] data, IntPtr size);

		[DllImport ("__Internal", EntryPoint = "lodepng_encode32")]
		public static extern int Encode32 (out IntPtr png, out IntPtr pngSize, byte[] image, int width, int height);

		[DllImport ("__Internal", EntryPoint = "lodepng_error_text")]
		public static extern string ErrorText (int error);

		[DllImport ("libc", EntryPoint = "free")]
		public static extern int Free (IntPtr p);
	}
}
