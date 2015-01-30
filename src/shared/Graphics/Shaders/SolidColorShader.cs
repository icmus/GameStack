using System;
using OpenTK;

namespace GameStack.Graphics {
	public sealed class SolidColorShader : Shader {
		public SolidColorShader () : base(VertSrc, FragSrc) {
		}
#if __DESKTOP__

		const string VertSrc = @"#version 150

uniform mat4 WorldViewProjection;

in vec4 Position;

void main() {
    gl_Position = WorldViewProjection * Position;
}
";

		const string FragSrc = @"#version 150

uniform vec4 Tint;

out vec4 FragColor;

void main() {
    FragColor = Tint;
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
