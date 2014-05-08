using System;
using System.Drawing;
using OpenTK;
using GameStack.Graphics;

namespace GameStack.Gui {
	public enum ButtonState {
		Normal,
		Over,
		Pressed,
		Active,
		ActivePressed,
		Disabled,
		Outside,
		ActiveOutside
	}

	public class Button : View, IPointerInput {
		ButtonState _state;
		RadioGroup _group;
		Label _label;

		public Button (LayoutSpec spec = null) : base(spec) {
			Color = Vector4.One;
		}

		public Button (RadioGroup group, LayoutSpec spec = null) : base(spec) {
			group.Add (this);
			_group = group;
			Color = Vector4.One;
		}

		public Sprite NormalSprite { get; set; }

		public Sprite OverSprite { get; set; }

		public Sprite PressedSprite { get; set; }

		public Sprite ActiveSprite { get; set; }

		public Sprite ActivePressedSprite { get; set; }

		public Sprite DisabledSprite { get; set; }

		public Vector4 Color { get; set; }

		public bool IsToggle { get; set; }


		public ButtonState State {
			get {
				return _state;
			}
			set { 
				if (_state != value) {
					var oldValue = _state;
					_state = value;
					if (this.StateChanged != null)
						this.StateChanged (this, new ButtonStateChangedEventArgs (value, oldValue));
				}
			}
		}

		public bool IsEnabled {
			get {
				return this.State != ButtonState.Disabled;
			}
			set {
				if (value != (this.State != ButtonState.Disabled))
					this.State = value ? ButtonState.Normal : ButtonState.Disabled;
			}
		}

		public bool IsActive {
			get {
				return this.State == ButtonState.Active || this.State == ButtonState.ActivePressed || this.State == ButtonState.ActiveOutside;
			}
			set {
				this.State = value ? ButtonState.Active : ButtonState.Normal;
			}
		}

		public event EventHandler<ButtonStateChangedEventArgs> StateChanged;
		public event EventHandler Clicked;

		Sprite CurrentSprite {
			get {
				switch (this.State) {
					case ButtonState.Disabled:
						return this.DisabledSprite ?? this.NormalSprite;
					case ButtonState.Pressed:
						return (this.PressedSprite ?? this.OverSprite) ?? this.NormalSprite;
					case ButtonState.Active:
					case ButtonState.ActiveOutside:
						return this.ActiveSprite ?? this.NormalSprite;
					case ButtonState.ActivePressed:
						return ((this.ActivePressedSprite ?? this.PressedSprite) ?? this.ActiveSprite) ?? this.NormalSprite;
					case ButtonState.Over:
						return this.OverSprite ?? this.NormalSprite;
					default:
						return this.NormalSprite;
				}
			}
		}

		public void SetLabel (string text, BitmapFont font, HorizontalAlignment halign, VerticalAlignment valign, Color color) {
			if (_label != null)
				this.RemoveView (_label);
			_label = new Label (text, font);
			_label.Color = color;
			_label.HorizontalAlignment = halign;
			_label.VerticalAlignment = valign;
			this.AddView (_label);
		}

		public void SetLabel (string text, BitmapFont font, HorizontalAlignment halign, VerticalAlignment valign) {
			if (_label != null)
				this.RemoveView (_label);
			_label = new Label (text, font);
			_label.Color = System.Drawing.Color.White;
			_label.HorizontalAlignment = halign;
			_label.VerticalAlignment = valign;
			this.AddView (_label);
		}

		public void SetLabel (string text, BitmapFont font) {
			if (_label != null)
				this.RemoveView (_label);
			_label = new Label (text, font);
			_label.Color = System.Drawing.Color.White;
			_label.HorizontalAlignment = HorizontalAlignment.Center;
			_label.VerticalAlignment = VerticalAlignment.Middle;
			this.AddView (_label);
		}

		public override void Layout () {
			base.Layout ();
			this.SizeSprite (this.NormalSprite);
			this.SizeSprite (this.OverSprite);
			this.SizeSprite (this.PressedSprite);
			this.SizeSprite (this.ActiveSprite);
			this.SizeSprite (this.ActivePressedSprite);
			this.SizeSprite (this.DisabledSprite);
		}

		void OnClicked () {
			if (this.Clicked != null)
				this.Clicked (this, EventArgs.Empty);
		}

		void SizeSprite (Sprite sprite) {
			var resizable = sprite as SlicedSprite;
			if (resizable != null)
				resizable.Resize (this.Size.ToVector2 ());
		}

		protected override void OnDraw (ref Matrix4 transform) {
			var sprite = this.CurrentSprite;

			SpriteMaterial spriteMat;
			if (Color != Vector4.One && (spriteMat = sprite.Material as SpriteMaterial) != null) {
				spriteMat.Color = Color;
				sprite.Draw(ref transform);
				spriteMat.Color = Vector4.One;
			} else
				sprite.Draw(ref transform);
		}

		public override void Dispose () {
			if (_group != null)
				_group.Remove (this);

			base.Dispose();
		}

		#region IPointerInput implementation

		void IPointerInput.OnPointerEnter (Vector2 where) {
			switch (this.State) {
				case ButtonState.Active:
					break;
				case ButtonState.Normal:
					this.State = ButtonState.Over;
					break;
				case ButtonState.Outside:
					this.State = ButtonState.Pressed;
					break;
				case ButtonState.ActiveOutside:
					this.State = ButtonState.ActivePressed;
					break;
			}
		}

		void IPointerInput.OnPointerExit (Vector2 where) {
			switch (this.State) {
				case ButtonState.Over:
					this.State = ButtonState.Normal;
					break;
				case ButtonState.Pressed:
					this.State = ButtonState.Outside;
					break;
				case ButtonState.ActivePressed:
					this.State = ButtonState.ActiveOutside;
					break;
			}
		}

		void IPointerInput.OnPointerDown (Vector2 where) {
			switch (this.State) {
				case ButtonState.Disabled:
					break;
				case ButtonState.Active:
					this.State = ButtonState.ActivePressed;
					break;
				default:
					this.State = ButtonState.Pressed;
					break;
			}
		}

		void IPointerInput.OnPointerUp (Vector2 where) {
			switch (this.State) {
				case ButtonState.Outside:
					this.State = ButtonState.Normal;
					break;
				case ButtonState.Pressed:
					this.State = this.IsToggle ? ButtonState.Active : ButtonState.Over;
					this.OnClicked ();
					break;
				case ButtonState.ActivePressed:
					this.State = ButtonState.Over;
					this.OnClicked ();
					break;
			}
		}

		void IPointerInput.OnPointerMove (Vector2 where) {
		}

		#endregion

	}

	public class ButtonStateChangedEventArgs : EventArgs {
		public ButtonState State { get; private set; }

		public ButtonState OldState { get; private set; }

		internal ButtonStateChangedEventArgs (ButtonState state, ButtonState oldState) {
			this.State = state;
			this.OldState = oldState;
		}
	}
}
