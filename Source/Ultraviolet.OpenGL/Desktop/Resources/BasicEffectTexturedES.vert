﻿#includeres "Ultraviolet.OpenGL.Resources.HeaderES.verth" executing
#includeres "Ultraviolet.OpenGL.Resources.SrgbConversion.verth" executing

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;
uniform vec4 DiffuseColor;
uniform bool SrgbColor;

DECLARE_INPUT_POSITION;		// uv_Position0
DECLARE_INPUT_TEXCOORD;		// uv_TextureCoordinate0

DECLARE_OUTPUT_COLOR;		// vColor
DECLARE_OUTPUT_TEXCOORD;	// vTextureCoordinate

void main()
{
    gl_Position        = uv_Position0 * World * View * Projection;
	vColor             = SrgbColor ? srgb2linear(DiffuseColor) : DiffuseColor;
    vTextureCoordinate = vec2(uv_TextureCoordinate0.x, 1.0 - uv_TextureCoordinate0.y);
}