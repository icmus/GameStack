#pragma warning disable 0219

using System;
using SDL2;
using GameStack;
using GameStack.Desktop;
using System.Runtime.InteropServices;

namespace Temp {
	class MainClass {
		public static void Main (string[] args) {
			if (SDL.SDL_Init(SDL.SDL_INIT_NOPARACHUTE | SDL.SDL_INIT_VIDEO) < 0)
				throw new SDL2Exception();

			var gameView = new SDL2GameView("Temp", 640, 480, false);
			var scene = new MyScene(gameView);

			var loop = new SDL2EventLoop();
			loop.Event += (object sender, SDL2EventArgs e) => {
				if (e.Event.type == SDL.SDL_EventType.SDL_KEYDOWN && e.Event.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE)
					loop.Dispose();
			};

			#if !DEBUG
			SDL.SDL_ShowCursor(0);
			#endif

			loop.AddView(gameView);
			loop.EnterLoop();

			gameView.Dispose();

			SDL.SDL_Quit();
		}
	}
}
