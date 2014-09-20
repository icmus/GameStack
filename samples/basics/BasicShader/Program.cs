using System;
using GameStack.Desktop;
using SDL2;

namespace Basics {
	public class Program {
		public static void Main (string[] args) {
			if ((SDL.SDL_Init((SDL.SDL_INIT_NOPARACHUTE | SDL.SDL_INIT_VIDEO)) < 0)) {
				throw new SDL2Exception();
			}
			using (var loop = new SDL2EventLoop()) {
				using (var view = new SDL2GameView(WindowMode.Window, "Basics", 640, 480, true)) {
					using (var scene = new DefaultScene(view)) {
						view.Event += (object sender, SDL2EventArgs e) => {
							if (e.Event.type == SDL.SDL_EventType.SDL_KEYDOWN && e.Event.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE)
								view.Dispose();
						};
						view.EnterLoop();
					}
				}
			}

			SDL.SDL_Quit();
		}
	}
}

