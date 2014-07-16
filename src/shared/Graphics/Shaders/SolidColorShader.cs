using System;
using System.Drawing;
using OpenTK;

namespace GameStack.Graphics {
	public sealed class SolidColorShader : Shader {
		public SolidColorShader () : base(VertSrc, FragSrc) {
		}
#if __DESKTOP__

		const string VertSrc = @"#version 120

uniform mat4 WorldViewProjection;

attribute vec4 Position;

void main() {
    gl_Position = WorldViewProjection * Position;
}
";

		const string FragSrc = @"#version 120

uniform vec4 Tint;

void main() {
    gl_FragColor = Tint;
}
";

#else
		const string VertSrc = @"
uniform mat4 WorldViewProjection;

attribute vec4 Position;

void main() {
    gl_Position = WorldViewProjection * Position;
}
";
		const string FragSrc = @"
uniform lowp vec4 Tint;

void main() {
	gl_FragColor = Tint;
}
";
#endif
	}
}
