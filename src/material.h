#ifndef MATERIAL_H
#define MATERIAL_H

#include "framework.h"
#include "shader.h"
#include "camera.h"
#include "mesh.h"
#include "extra/hdre.h"
using namespace std;

class Material {
public:

	Shader* shader = NULL;
	Texture* texture = NULL;
	vec4 color;

	virtual void setUniforms(Camera* camera, Matrix44 model) = 0;
	virtual void render(Mesh* mesh, Matrix44 model, Camera * camera) = 0;
	virtual void renderInMenu() = 0;
};

class StandardMaterial : public Material {
public:

	StandardMaterial();
	~StandardMaterial();

	void setUniforms(Camera* camera, Matrix44 model);
	void render(Mesh* mesh, Matrix44 model, Camera * camera);
	void renderInMenu();
};

class PBRMaterial : public StandardMaterial {
public:

	Texture* albedo = NULL;
	Texture* metalness = NULL;
	Texture* roughness = NULL;
	Texture* emissive = NULL;
	Texture* normal = NULL;

	std::vector<Texture*> HDREs;

	Texture* LUT = NULL;

	float roughness_factor = 0.5;
	float metalness_factor = 0.5;

	PBRMaterial();
	~PBRMaterial();
	void render(Mesh* mesh, Matrix44 model, Camera* camera);
	void setUniforms(Camera* camera, Matrix44 model);
	void renderInMenu();
};

class WireframeMaterial : public StandardMaterial {
public:

	WireframeMaterial();
	~WireframeMaterial();

	void render(Mesh* mesh, Matrix44 model, Camera * camera);
};

#endif