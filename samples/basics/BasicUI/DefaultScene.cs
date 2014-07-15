using System;
using GameStack;
using GameStack.Graphics;
using GameStack.Gui;
using OpenTK;

namespace Basics {
	public class DefaultScene : Scene, IHandler<Start> {
		Camera _cam;
		RootView _root;
		Atlas _sprites;
		BitmapFont _font;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.LowerLeft);
			_sprites = new Atlas("sprites.atlas");
			_font = new BitmapFont("helvetica.font");

			// create our UI root. adding it to the scene allows events
			// like touch and resize to filter through the UI hierarchy.
			// it also automatically disposes the hierarchy.
			_root = new RootView(e.Size, 0f);
			this.Add(_root);

			// we'll create two side-by-side panels that will automatically
			// resize properly if the window size changes (more relevant with
			// orientation changes but also on desktop if you enable window
			// resize).
			var left = new ImageView(_sprites["box"], new LayoutSpec {
				Left = p => 20,
				Top = p => 20,
				Width = p => Mathf.Floor((p.Width - 60f) / 2f),
				Height = p => p.Height - 40
			});
			var right = new ImageView(_sprites["box"], new LayoutSpec {
				Right = p => 20,
				Top = p => 20,
				Bottom = p => 20,
				Width = p => Mathf.Floor((p.Width - 60f) / 2f),
			});
			_root.AddView(left);
			_root.AddView(right);

			// we'll set the text on this label when then button is clicked
			var label = new Label("", _font, new LayoutSpec {
				Top = p => 60,
				Height = p => 50,
				Left = p => 15,
				Right = p => 15
			});
			left.AddView(label);

			// create a button and hook up a click handler to set the label
			var button = new Button(new LayoutSpec {
				Top = p => 10,
				Left = p => 10,
				Right = p => 10,
				Height = p => 50
			}) {
				NormalSprite = _sprites["button-normal"],
				PressedSprite = _sprites["button-pressed"]
			};
			button.SetLabel("Click Me!", _font,
				HorizontalAlignment.Center, VerticalAlignment.Middle);
			button.Clicked += (sender, arg) => 
				label.Text = "Thanks for clicking!";
			left.AddView(button);

			// create radio buttons to play with
			var rgroup = new RadioGroup();
			for (var i = 0; i < 3; i++) {
				var ii = i;
				var radio = new Button(rgroup, new LayoutSpec {
					Top = p => 10 + 65 * ii,
					Left = p => 10,
					Right = p => 10,
					Height = p => 50
				}) {
					NormalSprite = _sprites["button-normal"],
					PressedSprite = _sprites["button-pressed"],
					ActiveSprite = _sprites["button-active"]
				};
				radio.SetLabel("Radio " + i, _font);
				radio.Tag = i;
				if (i == 0)
					radio.IsActive = true;
				right.AddView(radio);
			}
			rgroup.Changed += (sender, args) =>
				label.Text = "Radio " 
				+ ((RadioGroup)sender).Active.Tag.ToString();

			// create a toggle button that does nothing much
			var toggle = new Button(new LayoutSpec {
				Left = p => 10,
				Right = p => 10,
				Bottom = p => 10,
				Height = p => 50
			}) {
				NormalSprite = _sprites["button-normal"],
				PressedSprite = _sprites["button-pressed"],
				ActiveSprite = _sprites["button-active"],
				IsToggle = true
			};
			toggle.SetLabel("Toggle Me!", _font);
			toggle.Clicked += (sender, args) =>
				label.Text = toggle.IsActive ? "Toggled ON" : "Toggled OFF";
			right.AddView(toggle);
		}
			
		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) { // using the camera...
				// not much to do here, just draw the UI root
				// we could also apply transforms or draw the UI
				// to a frame buffer object for post effects
				_root.Draw();
			}
		}

		public override void Dispose () {
			if (_sprites != null)
				_sprites.Dispose();
			if (_font != null)
				_font.Dispose();

			base.Dispose();
		}
	}
}
