#define PI 3.14159265359
#define RECIPROCAL_PI 0.3183098861837697
#define epsilon 1e-6

uniform samplerCube u_texture_prem_0;
uniform samplerCube u_texture_prem_1;
uniform samplerCube u_texture_prem_2;
uniform samplerCube u_texture_prem_3;
uniform samplerCube u_texture_prem_4;
uniform samplerCube u_texture;

// Variables coming from the CPU
uniform vec3 u_camera_position;
uniform vec3 u_light_pos;

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
uniform sampler2D u_emissive;
uniform sampler2D u_normal;

uniform float u_roughness_factor;
uniform float u_metalness_factor;

uniform sampler2D u_LUT;

uniform vec4 u_color;

uniform float u_light_intensity;

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

mat3 cotangent_frame(vec3 N, vec3 p, vec2 uv){
	// get edge vectors of the pixel triangle
	vec3 dp1 = dFdx( p );
	vec3 dp2 = dFdy( p );
	vec2 duv1 = dFdx( uv );
	vec2 duv2 = dFdy( uv );

	// solve the linear system
	vec3 dp2perp = cross( dp2, N );
	vec3 dp1perp = cross( N, dp1 );
	vec3 T = dp2perp * duv1.x + dp1perp * duv2.x;
	vec3 B = dp2perp * duv1.y + dp1perp * duv2.y;

	// construct a scale-invariant frame
	float invmax = inversesqrt( max( dot(T,T), dot(B,B) ) );
	return mat3( T * invmax, B * invmax, N );
}

vec3 perturbNormal( vec3 N, vec3 V, vec2 texcoord, vec3 normal_pixel ){
	#ifdef USE_POINTS
	return N;
	#endif

	// assume N, the interpolated vertex normal and
	// V, the view vector (vertex to eye)
	//vec3 normal_pixel = texture2D(normalmap, texcoord ).xyz;
	normal_pixel = normal_pixel * 255./127. - 128./127.;
	mat3 TBN = cotangent_frame(N, V, texcoord);
	return normalize(TBN * normal_pixel);
}

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
	vectors.L = normalize(u_light_pos - v_world_position);
	vectors.N = normalize(v_normal);
	vectors.V = normalize(u_camera_position - v_world_position);
	vectors.N = perturbNormal(vectors.N, vectors.V, v_uv, texture2D(u_normal,v_uv).xyz);
	vectors.R = normalize(reflect(-vectors.V,vectors.N));
	vectors.H = normalize(vectors.V + vectors.L); 
	
}
float computeGeometry(){
	
	float NdotH = max(0.0,dot(vectors.N, vectors.H));
	float NdotV = max(0.0,dot(vectors.N, vectors.V));
	float VdotH = max(0.0,dot(vectors.V,vectors.H));
	float NdotL = max(0.0,dot(vectors.N, vectors.L));

	float k = pow(pbr_mat.roughness + 1.0,2.0)/8.0;
	float G1 = NdotV/(NdotV*(1.0 - k) + k);
	float G2 = NdotL/(NdotL*(1.0 - k) + k);
	float G = G1*G2;

	//float first_term = (2.0*NdotH*NdotV)/(VdotH + epsilon);
	//float second_term = (2.0*NdotH*NdotL)/(VdotH + epsilon);
	//float G = min(1.0,min(first_term, second_term));

	return G;
}

vec3 toneMap(vec3 color)
{
    return color / (color + vec3(1.0));
}

float computeDistribution(){
	float alpha = pow(pbr_mat.roughness, 2.0);
	float NdotH = max(0.0,dot(vectors.N, vectors.H));
	float D = pow(alpha,2.0)/(PI*pow(pow(NdotH,2.0)*(pow(alpha,2.0)-1.0) + 1.0,2.0));
	return D;
}

vec3 computeFresnel(){
	float NdotL = max(0.0,dot(vectors.L, vectors.N));
	vec3 F = pbr_mat.f0 + (1.0 - pbr_mat.f0)*pow(1.0-NdotL, 5.0);
	return F;
}

void GetMaterialProperties(){
	pbr_mat.metalness = texture2D(u_metalness, v_uv).x*u_metalness_factor;
	pbr_mat.roughness = texture2D(u_roughness, v_uv).y*u_roughness_factor;
	pbr_mat.albedo = texture2D(u_albedo, v_uv)*u_color;
	pbr_mat.f0 = mix(vec3(0.04), pbr_mat.albedo.xyz, pbr_mat.metalness);
}

vec3 getPixelColor(){

	vec3 albedo = gamma_to_linear(pbr_mat.albedo.xyz);

	float NdotL = max(0.0,dot(vectors.N,vectors.L));
	float NdotV = max(0.0,dot(vectors.N,vectors.V));

	float G = computeGeometry();
	float D = computeDistribution();
	vec3 F = computeFresnel();

	vec3 f_specular = (F*G*D)/(4.0*NdotL*NdotV + epsilon);
	vec3 f_diffuse = mix(albedo, vec3(0.0), pbr_mat.metalness)/PI;
	vec3 f = f_specular + f_diffuse;

	vec2 LUT_coord = vec2(max(0.0,dot(vectors.N,vectors.V)),pbr_mat.roughness);
	vec3 brdf2D = texture2D(u_LUT, clamp(LUT_coord,0.01,0.99)).xyz;

	vec3 specularSample = getReflectionColor(vectors.R, pbr_mat.roughness); 
	float cosTheta = max(0.0,dot(vectors.V,vectors.N));
	vec3 Ks = FresnelSchlickRoughness(cosTheta, pbr_mat.f0, pbr_mat.roughness);
	vec3 SpecularBRDF = Ks*brdf2D.x + brdf2D.y; 
	vec3 SpecularIBL = specularSample * SpecularBRDF;

	vec3 diffuseSample = getReflectionColor(vectors.N, 1.0);
	vec3 diffuseColor = f_diffuse;
	vec3 DiffuseIBL = diffuseSample * diffuseColor;
	DiffuseIBL *= (vec3(1.0) - Ks);

	vec3 IBL = SpecularIBL + DiffuseIBL;

	vec4 test = texture2D(u_normal,v_uv);

	return u_light_intensity*NdotL*f + IBL;
}

void main()
{
	computeVectors();
	GetMaterialProperties();

	vec3 emissive = gamma_to_linear(texture2D(u_emissive, v_uv).xyz);
	
	gl_FragColor = vec4(linear_to_gamma(getPixelColor() + emissive),1.0);
}