using System;

namespace GameStack {
	public enum KeyState {
		Down,
		Up,
		Repeat
	}

	[Flags]
	public enum KeyModifier {
		None = 0x0,
		LeftShift = 0x1,
		RightShift = 0x2,
		LeftControl = 0x40,
		RightControl = 0x80,
		LeftAlt = 0x100,
		RightAlt = 0x200,
		LeftMeta = 0x400,
		RightMeta = 0x800,
		NumLock = 0x1000,
		CapsLock = 0x2000,
		AltGr = 0x4000,
		Control = LeftControl | RightControl,
		Shift = LeftShift | RightShift,
		Alt = LeftAlt | RightAlt,
		Meta = LeftMeta | RightMeta
	}

	public class KeyEvent : EventBase {
		internal KeyEvent(KeyState state) {
		}

		public KeyState State { get; private set; }

		public KeyModifier Modifier { get; private set; }

		public int ScanCode { get; private set; }

		public char Symbol { get; private set; }

		internal KeyEvent (KeyState state, KeyModifier modifier, int scanCode, char symbol) {
			this.State = state;
			this.Modifier = modifier;
			this.ScanCode = scanCode;
			this.Symbol = symbol;
		}
	}
}
