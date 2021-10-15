uniform vec3 u_camera_pos;

uniform mat4 u_model;
uniform mat4 u_viewprojection;

//this will store the color for the pixel shader
varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;
varying vec4 v_color;

uniform vec4 u_color;

void main()
{
	vec3 L = normalize(light_pos - v_world_position);
	vec3 N = normalize(v_normal);
	vec3 V = normalize(u_camera_position - v_world_position);
	vec3 R = reflect(-L,N);
	vec3 H = normalize(V+L);
	gl_FragColor = u_color;
}
