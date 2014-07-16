using System;
using OpenTK;

namespace GameStack.Gui {
	public interface IPointerInput {
		void OnPointerEnter (Vector2 where);

		void OnPointerExit (Vector2 where);

		void OnPointerDown (Vector2 where);

		void OnPointerUp (Vector2 where);

		void OnPointerMove (Vector2 where);
	}
}
