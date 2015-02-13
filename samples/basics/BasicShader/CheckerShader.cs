using System;
using GameStack;
using GameStack.Graphics;

namespace Basics {
	public class CheckerShader : Shader {
		public CheckerShader () : base(VertSrc, FragSrc) {
		}

		#if __MOBILE__
		const string VertSrc = @"
uniform mat4 WorldViewProjection;

attribute vec4 Position;
attribute vec2 TexCoord0;

varying vec2 texCoord0;

void main() {
	texCoord0 = TexCoord0;
	gl_Position = WorldViewProjection * Position;
}
";

		const string FragSrc = @"
uniform mediump float NumSquares;

varying mediump vec2 texCoord0;

void main() {
	mediump float f = mod(floor(NumSquares * texCoord0.x) + 
	floor(NumSquares * texCoord0.y), 2.0);
	gl_FragColor = f < 1.0 ? vec4(0, 0, 0, 1) : vec4(1, 0, 0, 1);	
}
";
		#else
		const string VertSrc = @"#version 150
uniform mat4 WorldViewProjection;

in vec4 Position;
in vec2 TexCoord0;

out vec2 texCoord0;

void main() {
	texCoord0 = TexCoord0;
	gl_Position = WorldViewProjection * Position;
}
";

		const string FragSrc = @"#version 150
uniform float NumSquares;
in vec2 texCoord0;

out vec4 FragColor;

void main() {
	float f = mod(floor(NumSquares * texCoord0.x) + 
	floor(NumSquares * texCoord0.y), 2.0);
	FragColor = f < 1.0 ? vec4(0, 0, 0, 1) : vec4(1, 0, 0, 1);
}";
		#endif
	}
}

