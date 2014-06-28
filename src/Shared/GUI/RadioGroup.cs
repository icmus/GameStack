using System;
using System.Collections.Generic;
using System.Linq;

namespace GameStack.Gui {
	public class RadioGroup {
		List<Button> _buttons;
		bool _suppress;

		public RadioGroup () {
			_buttons = new List<Button> ();
		}

		public Button Active {
			get {
				return _buttons.FirstOrDefault (b => b.IsActive);
			}
		}

		public EventHandler<RadioGroupEventArgs> Changed;

		internal void Add (Button button) {
			_buttons.Add (button);
			button.StateChanged += OnStateChanged;
		}

		internal bool Remove (Button button) {
			button.StateChanged -= OnStateChanged;
			return _buttons.Remove (button);
		}

		internal void Clear () {
			_suppress = true;
			foreach (var b in _buttons)
				b.IsActive = false;
			_suppress = false;
		}

		void OnStateChanged (object sender, ButtonStateChangedEventArgs e) {
			var button = (Button)sender;
			if (e.State == ButtonState.Active) {
				Button previous = null;
				foreach (var other in _buttons) {
					if (other != button && other.IsActive) {
						other.IsActive = false;
						previous = other;
					}
				}
				if (this.Changed != null)
					this.Changed (this, new RadioGroupEventArgs (button, previous));
			} else if (!_suppress && (e.OldState == ButtonState.Active || e.OldState == ButtonState.ActiveOutside || e.OldState == ButtonState.ActivePressed)) {
				foreach (var other in _buttons)
					if (other.IsActive)
						return;
				button.IsActive = false;
			}
		}
	}

	public class RadioGroupEventArgs : EventArgs {
		public Button Previous { get; private set; }

		public Button Current { get; private set; }

		internal RadioGroupEventArgs (Button current, Button previous) {
			this.Current = current;
			this.Previous = previous;
		}
	}
}

