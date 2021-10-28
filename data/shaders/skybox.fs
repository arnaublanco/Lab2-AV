
const float GAMMA = 2.2;
const float INV_GAMMA = 1.0 / GAMMA;

varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;
varying vec4 v_color;
uniform samplerCube u_texture;
uniform vec3 u_camera_position;

// degamma
vec3 gamma_to_linear(vec3 color)
{
	return pow(color, vec3(GAMMA));
}

// gamma
vec3 linear_to_gamma(vec3 color)
{
	return pow(color, vec3(INV_GAMMA));
}

vec3 toneMap(vec3 color)
{
    return color / (color + vec3(1.0));
}

void main()
{
	vec3 direction = v_world_position - u_camera_position;
	gl_FragColor = vec4(linear_to_gamma(toneMap(textureCube(u_texture, direction).xyz)),1.0);
}
