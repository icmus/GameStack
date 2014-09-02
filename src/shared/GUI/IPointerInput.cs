using System;
using OpenTK;

namespace GameStack.Gui {
	public interface IPointerInput {
		void OnPointerEnter (FrameArgs frame, Vector2 where);

		void OnPointerExit (FrameArgs frame, Vector2 where);

		void OnPointerDown (FrameArgs frame, Vector2 where);

		void OnPointerUp (FrameArgs frame, Vector2 where);

		void OnPointerMove (FrameArgs frame, Vector2 where);
	}
}
