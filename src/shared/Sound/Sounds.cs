using System;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace GameStack {
	internal static class Sounds {
		static IntPtr _device;
		static ContextHandle _context;

		public static void Init () {
			var deviceName = Alc.GetString(IntPtr.Zero, AlcGetString.DefaultAllDevicesSpecifier);
			_device = Alc.OpenDevice(deviceName);
			_context = Alc.CreateContext(_device, (int[])null);
			Alc.MakeContextCurrent(_context);
			CheckError();
		}

		public static void CheckError () {
			var error = AL.GetError();
			if (error != ALError.NoError)
				throw new OpenALException(error);
		}

		public static void Shutdown () {
			Alc.DestroyContext(_context);
			Alc.CloseDevice(_device);
		}
	}

	public class OpenALException : Exception {
		public OpenALException (ALError error)
			: base(AL.GetErrorString(error)) {
		}
	}
}
