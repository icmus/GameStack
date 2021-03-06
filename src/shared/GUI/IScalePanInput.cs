﻿using System;
using GameStack.Gui;
using OpenTK;

namespace GameStack.Gui
{
	public interface IScalePanInput : IInputTarget {
		void OnScalePan (ITouchInput target, FrameArgs frame, Vector2 where, Vector2 delta, float scale);
	}
}

