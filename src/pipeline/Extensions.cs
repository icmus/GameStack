using System;
using System.Runtime.InteropServices;


namespace GameStack.Pipeline
{
	public class Extensions
	{
		[DllImport ("libc")]
		static extern int uname (IntPtr buf);

		static Extensions () {
			IntPtr buf = IntPtr.Zero;
			try {
				buf = Marshal.AllocHGlobal(8192);

				if (uname (buf) == 0) {
					string os = Marshal.PtrToStringAnsi(buf);
					IsRunningOnMac = os == "Darwin";
				}
			} catch {
			} finally {
				if (buf != IntPtr.Zero)
					Marshal.FreeHGlobal(buf);
			}
		}

		public static bool IsRunningOnMac { get; private set; }
	}
}
