using System;
using OpenTK;

namespace GameStack.Gui {
	public interface ITouchInput : IInputTarget {
		void OnPointerEnter (ITouchInput target, FrameArgs frame, Vector2 where);

		void OnPointerExit (ITouchInput target, FrameArgs frame, Vector2 where);

		void OnPointerDown (ITouchInput target, FrameArgs frame, Vector2 where);

		void OnPointerUp (ITouchInput target, FrameArgs frame, Vector2 where);

		void OnPointerMove (ITouchInput target, FrameArgs frame, Vector2 where);
	}
}
