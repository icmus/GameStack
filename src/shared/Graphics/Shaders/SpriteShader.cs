using System;
using OpenTK;

namespace GameStack.Graphics {
	public sealed class SpriteShader : Shader {
		public SpriteShader () : base(VertSrc, FragSrc) {
		}

#if __MOBILE__
		const string VertSrc = @"
uniform mat4 WorldViewProjection;

attribute vec4 Position;
attribute vec4 Color;
attribute vec2 TexCoord0;

varying vec4 color;
varying vec2 texCoord0;

void main() {
    // slice the sprite out of the texture
    texCoord0 = TexCoord0;
    color = Color;
    gl_Position = WorldViewProjection * Position;
}
";
		const string FragSrc = @"
uniform sampler2D Texture;
uniform lowp vec4 Tint;

varying mediump vec2 texCoord0;
varying lowp vec4 color;

void main() {
	gl_FragColor = texture2D(Texture, texCoord0) * color * Tint;
    if(gl_FragColor.a == 0.0)
        discard;
}
";
#else
		const string VertSrc = @"#version 150

uniform mat4 WorldViewProjection;

in vec4 Position;
in vec4 Color;
in vec2 TexCoord0;

out vec4 color;
out vec2 texCoord0;

void main() {
	// slice the sprite out of the texture
	texCoord0 = TexCoord0;
	color = Color;
	gl_Position = WorldViewProjection * Position;
}
";

		const string FragSrc = @"#version 150

uniform sampler2D Texture;
uniform vec4 Tint;

in vec2 texCoord0;
in vec4 color;

out vec4 FragColor;

void main() {
	vec4 c = texture(Texture, texCoord0);
	FragColor = c * color * Tint;
	if(FragColor.a == 0.0)
		discard;
}
";
#endif
	}
}
