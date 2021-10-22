#define PI 3.14159265359
#define RECIPROCAL_PI 0.3183098861837697
#define epsilon 0.01

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

uniform sampler2D u_LUT;

uniform vec4 u_color;

uniform vec3 f0;

struct PBRMat
{
	vec4 albedo;
	float roughness;
	float metalness;
	vec3 f0;
}pbr_mat;

struct Vectors{
	vec3 L;
	vec3 N;
	vec3 V;
	vec3 R;
	vec3 H;
}vectors;

vec3 getReflectionColor(vec3 r, float roughness)
{
	float lod = roughness * 5.0;

	vec4 color;

	if(lod < 1.0) color = mix( textureCube(u_texture, r), textureCube(u_texture_prem_0, r), lod );
	else if(lod < 2.0) color = mix( textureCube(u_texture_prem_0, r), textureCube(u_texture_prem_1, r), lod - 1.0 );
	else if(lod < 3.0) color = mix( textureCube(u_texture_prem_1, r), textureCube(u_texture_prem_2, r), lod - 2.0 );
	else if(lod < 4.0) color = mix( textureCube(u_texture_prem_2, r), textureCube(u_texture_prem_3, r), lod - 3.0 );
	else if(lod < 5.0) color = mix( textureCube(u_texture_prem_3, r), textureCube(u_texture_prem_4, r), lod - 4.0 );
	else color = textureCube(u_texture_prem_4, r);

	return color.rgb;
}

vec3 FresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

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
	pbr_mat.f0 = mix(vec3(0.04), u_color.xyz, pbr_mat.metalness);
	float NdotL = max(0.0,dot(vectors.L, vectors.N));
	vec3 F = pbr_mat.f0 + (1.0 - pbr_mat.f0)*pow(1.0-NdotL, 5.0);
	return F;
}

void GetMaterialProperties(){
	pbr_mat.metalness = texture2D(u_metalness, v_uv).x*u_metalness_factor;
	pbr_mat.roughness = texture2D(u_roughness, v_uv).x*u_roughness_factor;
	pbr_mat.albedo = texture2D(u_albedo, v_uv);
}

vec3 getPixelColor(){

	vec2 LUT_coord = vec2(max(0.0,dot(vectors.N,vectors.V)),pbr_mat.roughness);
	vec3 brdf2D = texture2D(u_LUT, v_uv).xyz;

	vec3 specularSample = getReflectionColor(v_world_position,pbr_mat.roughness);
	float cosTheta = max(0.0,dot(vectors.N,vectors.L));
	vec3 SpecularBRDF = FresnelSchlickRoughness(cosTheta, pbr_mat.f0, pbr_mat.roughness)*brdf2D.x + brdf2D.y;
	vec3 SpecularIBL = specularSample * SpecularBRDF;

	float NdotL = max(0.0,dot(vectors.N,vectors.L));
	float NdotV = max(0.0,dot(vectors.N,vectors.V));

	float G = computeGeometry();
	float D = computeDistribution();
	vec3 F = computeFresnel();

	vec3 f_specular = (F*G*D)/(4.0*NdotL*NdotV + epsilon);
	vec3 f_diffuse = mix(u_color.xyz, vec3(0.0), pbr_mat.metalness)/PI;
	vec3 f = f_specular + f_diffuse;

	return f;
}

void main()
{
	vec4 color = pbr_mat.albedo;
	computeVectors();
	GetMaterialProperties();
	float NdotL = max(0.0,dot(vectors.N,vectors.L));
	gl_FragColor = vec4(NdotL*getPixelColor(),1.0);
}