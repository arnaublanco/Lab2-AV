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
uniform sampler2D u_metalness; 
uniform sampler2D u_roughness;

uniform float u_roughness_factor;
uniform float u_metalness_factor;

uniform vec4 u_color;

//uniform float f0;

struct PBRMat
{
	vec4 albedo;
	vec4 roughness;
	vec4 metalness;
	vec3 f0;
	vec3 c_diff;
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
float computeGeometry(){
	float NdotH = dot(vectors.N, vectors.H);
	float NdotV = dot(vectors.N, vectors.V);
	float NdotL = dot(vectors.N, vectors.L);
	float VdotH = dot(vectors.V, vectors.H);

	float first_term = 2.0*NdotH*NdotV/NdotH;
	float second_term = 2.0*NdotH*NdotL/VdotH;

	float G = min(1.0, min(first_term, second_term));

	return G;
}

float computeDistribution(){
	float alpha = pow(u_roughness_factor, 2.0);
	float NdotH = dot(vectors.N, vectors.H);
	float alpha_squared = pow(alpha,2.0);
	float D = alpha_squared/(PI*pow(pow(NdotH,2.0)*(alpha_squared-1.0) + 1.0,2.0));
	return D;
}
vec3 computeFresnel(){
	vec3 F = pbr_mat.f0 + (1.0 - pbr_mat.f0)*(pow(1.0-dot(vectors.L, vectors.N), 5.0));
	return F;
}

void GetMaterialProperties(vec4 color){
	pbr_mat.metalness = texture2D(u_metalness, v_uv);
	pbr_mat.roughness = texture2D(u_roughness, v_uv);
	pbr_mat.albedo = texture2D(u_albedo, v_uv);
	pbr_mat.f0 = mix(u_color.xyz, vec3(0.04), u_metalness_factor);
	pbr_mat.c_diff = mix(vec3(0.0), u_color.xyz, u_metalness_factor);
}

vec3 getPixelColor(){
	float NdotL = max(0.0, dot(vectors.N,vectors.L));
	float NdotV = max(0.0, dot(vectors.N,vectors.V));
	float G = computeGeometry();
	float D = computeDistribution();
	vec3 F = computeFresnel();
	vec3 f_specular = F*G*D/(4.0*NdotL*NdotV);
	vec3 f_diffuse = pbr_mat.c_diff/PI;
	vec3 f = f_specular + f_diffuse;
	return f;
}

void main()
{
	vec4 color = pbr_mat.albedo;
	computeVectors();
	GetMaterialProperties(color);
	gl_FragColor = vec4(getPixelColor(),1.0);
}