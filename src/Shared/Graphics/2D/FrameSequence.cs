using System;
using System.Collections.Generic;
using System.Drawing;
using GameStack;
using OpenTK;
using OpenTK.Graphics;

namespace GameStack.Graphics {
	public class FrameSequence<T> : IUpdater {
		List<T> _frames = new List<T> ();
		double _time, _frameTime;
		int _frame;
		bool _loop;
		bool _firstUpdate = true;

		public FrameSequence (int framesPerSecond, bool loop, params T[] frames) {
			_frameTime = 1.0 / (double)framesPerSecond;
			_loop = loop;
			_frames.AddRange (frames);
		}

		public bool IsDone { get; private set; }

		public T Current { get { return _frames [_frame]; } }

		public void Update (FrameArgs frame) {
			if (this.IsDone)
				return;
			if (_firstUpdate) {
				_firstUpdate = false;
				return;
			}

			_time += frame.DeltaTime;
			_frame = (int)Math.Floor (_time / _frameTime);
			if (_frame >= _frames.Count)
				_frame = _loop ? _frame % _frames.Count : _frames.Count;
			if (_frame >= _frames.Count && _frame >= 0)
				this.IsDone = true;
		}

		public void Reset () {
			_frame = 0;
			_time = 0.0;
			_firstUpdate = true;
			this.IsDone = false;
		}
	}
}
