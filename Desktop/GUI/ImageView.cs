using System;
using GameStack;
using OpenTK;
using GameStack.Graphics;

namespace GameStack.Gui {
	public class ImageView : View {
		public ImageView (Sprite sprite, LayoutSpec spec = null) : base(spec) {
			this.Sprite = sprite;
			this.Color = Vector4.One;
		}

		public Sprite Sprite { get; set; }
		public Vector4 Color { get; set; }

		public override void Layout () {
			base.Layout();
			var resizable = this.Sprite as SlicedSprite;
			if (resizable != null)
				resizable.Resize(this.Size.ToVector2());
		}

		protected override void OnDraw (ref Matrix4 transform) {
			SpriteMaterial spriteMat;
			if (Color != Vector4.One && (spriteMat = Sprite.Material as SpriteMaterial) != null) {
				spriteMat.Color = Color;
				this.Sprite.Draw(ref transform);
				spriteMat.Color = Vector4.One;
			} else
				this.Sprite.Draw(ref transform);
		}
	}
}
