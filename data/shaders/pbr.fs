#define PI 3.14159265359
#define RECIPROCAL_PI 0.3183098861837697

uniform samplerCube u_texture_prem_0;
uniform samplerCube u_texture_prem_1;
uniform samplerCube u_texture_prem_2;
uniform samplerCube u_texture_prem_3;
uniform samplerCube u_texture_prem_4;
uniform samplerCube u_texture;

// Variables coming from the CPU
uniform vec3 u_camera_position;
uniform vec3 light_pos;

uniform mat4 u_model;
uniform mat4 u_viewprojection;

//this will store the color for the pixel shader
varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;
varying vec4 v_color;

const float GAMMA = 2.2;
const float INV_GAMMA = 1.0 / GAMMA;

uniform sampler2D u_albedo;
uniform sampler2D u_metalness; //aixo ha de ser un sampler2D
uniform sampler2D u_roughness;

//uniform float f0;

struct PBRMat
{
	vec4 albedo;
	vec4 roughness;
	vec4 metalness;
}pbr_mat;

struct Vectors{
	vec3 L;
	vec3 N;
	vec3 V;
	vec3 R;
	vec3 H;
}vectors;

void computeVectors(){
	vectors.L = normalize(light_pos - v_world_position);
	vectors.N = normalize(v_normal);
	vectors.V = normalize(u_camera_position - v_world_position);
	vectors.R = reflect(-vectors.L,vectors.N);
	vectors.H = normalize(vectors.V + vectors.L); 
	
}

void GetMaterialProperties(){
	pbr_mat.metalness = texture2D(u_metalness, v_uv); //texture2D(u_metalness, v_uv);
	pbr_mat.roughness = texture2D(u_roughness, v_uv);
	pbr_mat.albedo = texture2D(u_albedo, v_uv);
}

vec3 getPixelColor(vec3 N, vec3 L, vec3 V){
	float NdotL = max(0.0, dot(vectors.N,vectors.L));
	float NdotV = max(0.0, dot(vectors.N,vectors.V));
	float f_specular = 1/(4*NdotL*NdotV);
	return vec3(1.0);
}

void main()
{
	computeVectors();
	GetMaterialProperties();
	//gl_FragColor = vec4(getPixelColor,1.0);
	gl_FragColor = vec4(1.0);
}