using System;
using GameStack;
using GameStack.Graphics;

namespace Basics {
	public class CheckerShader : Shader {
		public CheckerShader () : base(VertSrc, FragSrc) {
		}

		const string VertSrc = @"#version 150

uniform mat4 WorldViewProjection;

in vec4 Position;
in vec2 MultiTexCoord0;

out vec2 texCoord0;

void main() {
    // slice the sprite out of the texture
    texCoord0 = MultiTexCoord0;
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
	}
}

