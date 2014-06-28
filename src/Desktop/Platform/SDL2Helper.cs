using System;
using SDL2;

namespace GameStack.Desktop {
	public static class SDL2Helper {
	}
}
namespace SDL2 {
	public class SDL2Exception : Exception {
		public SDL2Exception ()
			: base(string.Format("SDL Error: {0}", SDL.SDL_GetError())) {
		}
	}
}
