using System;
using System.Collections.Generic;
using OpenTK;

namespace GameStack {
	public interface IGameView : IGameViewEventSource, IDisposable {
		Vector2 Size { get; }
		float PixelScale { get; }
		bool IsPaused { get; }

		void EnableGesture (GestureType type);
	}

	public interface IGameViewEventSource {
		event EventHandler<FrameArgs> Update;
		event EventHandler<FrameArgs> Render;
		event EventHandler Destroyed;
	}

	public interface IGameViewEventHandler {
		void OnUpdate (object sender, FrameArgs args);
		void OnRender (object sender, FrameArgs args);
		void OnDestroy (object sender, EventArgs args);
	}
}
