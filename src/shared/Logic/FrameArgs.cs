using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GameStack {
	public class FrameArgs : EventArgs {
		List<EventBase> _events;
		ReadOnlyCollection<EventBase> _eventsReadOnly;

		public double Time { get; internal set; }

		public float DeltaTime { get; internal set; }

		public int EventCount { get { return _events.Count; } }

		public ReadOnlyCollection<EventBase> Events { get { return _eventsReadOnly; } }

		internal FrameArgs () {
			_events = new List<EventBase> ();
			_eventsReadOnly = _events.AsReadOnly ();
		}

		internal void Enqueue (EventBase e) {
			_events.Add (e);
		}

		internal void ClearEvents () {
			_events.Clear ();
		}
	}
}

