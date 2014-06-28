using System;
using GameStack.Desktop;
using SDL2;

namespace Samples.Match3 {
	class MainClass {
		public static void Main (string[] args) {
			if (SDL.SDL_Init(SDL.SDL_INIT_NOPARACHUTE | SDL.SDL_INIT_VIDEO) < 0)
				throw new SDL2Exception();

			using (var loop = new SDL2EventLoop()) {
				using (var view = new SDL2GameView("Match3", 1024, 768, false, true, 0, 0)) {
					using (var scene = new Game(view)) {
						loop.Event += (object sender, SDL2EventArgs e) => {
							if (e.Event.type == SDL.SDL_EventType.SDL_KEYDOWN && e.Event.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE)
								loop.Dispose();
						};
						loop.AddView(view);
						loop.EnterLoop();
					}
				}
			}
			SDL.SDL_Quit();
		}
	}
}
