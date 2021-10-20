#define PI 3.14159265359
#define RECIPROCAL_PI 0.3183098861837697
#define epsilon 0.1

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
	float roughness;
	float metalness;
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
	vectors.R = normalize(reflect(-vectors.L,vectors.N));
	vectors.H = normalize(vectors.V + vectors.L); 
	
}
float computeGeometry(){
	
	float NdotH = max(0.0,dot(vectors.N, vectors.H));
	float NdotV = max(0.0,dot(vectors.N, vectors.V));
	float VdotH = max(0.0,dot(vectors.V,vectors.H));
	float NdotL = max(0.0,dot(vectors.N, vectors.L));

	//float k = pow((pbr_mat.roughness + 1.0),2)/8;
	//float first_term = NdotV/(NdotV*(1-k) + k);
	//float second_term = NdotL/(NdotL*(1-k) + k);
	//float G = first_term*second_term;

	float first_term = (2.0*NdotH*NdotV)/(VdotH + epsilon);
	float second_term = (2.0*NdotH*NdotL)/(VdotH + epsilon);
	float G = min(1.0,min(first_term, second_term));

	return G;
}

float computeDistribution(){
	float alpha = pow(pbr_mat.roughness, 2.0);
	float NdotH = max(0.0,dot(vectors.N, vectors.H));
	float D = pow(alpha,2.0)/(PI*pow(pow(NdotH,2.0)*(pow(alpha,2.0)-1.0) + 1.0,2.0));
	return D;
}
vec3 computeFresnel(){
	vec3 f0 = mix(u_color.xyz, vec3(0.04), pbr_mat.metalness);
	float NdotL = max(0.0,dot(vectors.L, vectors.N));
	vec3 F = f0 + (1.0 - f0)*pow(1.0-NdotL, 5.0);
	return F;
}

void GetMaterialProperties(){
	pbr_mat.metalness = texture2D(u_metalness, v_uv).x*u_metalness_factor;
	pbr_mat.roughness = texture2D(u_roughness, v_uv).x*u_roughness_factor;
	pbr_mat.albedo = texture2D(u_albedo, v_uv);
}

vec3 getPixelColor(){
	float NdotL = max(0.0,dot(vectors.N,vectors.L));
	float NdotV = max(0.0,dot(vectors.N,vectors.V));
	float G = computeGeometry();
	float D = computeDistribution();
	vec3 F = computeFresnel();
	vec3 f_specular = (F*G*D)/(4.0*NdotL*NdotV + epsilon);
	vec3 f_diffuse = mix(vec3(0.0), u_color.xyz, pbr_mat.metalness)/PI;
	vec3 f = f_specular + f_diffuse;
	return f;
}

void main()
{
	//vec4 color = pbr_mat.albedo;
	computeVectors();
	GetMaterialProperties();
	gl_FragColor = vec4(getPixelColor(),1.0);
	//gl_FragColor = vec4(1.0);
}