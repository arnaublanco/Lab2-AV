#ifndef SCENENODE_H
#define SCENENODE_H

#include "framework.h"
#include "shader.h"
#include "mesh.h"
#include "camera.h"
#include "material.h"

extern const char* backgrounds[3];
extern unsigned int background_selected;
extern int hdre_changed;

class SceneNode {
public:

	static unsigned int lastNameId;

	SceneNode();
	SceneNode(const char* name);
	~SceneNode();

	Material * material = NULL;
	std::string name;

	Mesh* mesh = NULL;
	Matrix44 model;

	virtual void render(Camera* camera);
	virtual void renderWireframe(Camera* camera);
	virtual void renderInMenu();
};

class Light : public SceneNode {
public:
	Vector3 position;
	Vector4 color;
	float light_intensity = 1.0;

	Light(const char* name);
	~Light();
	void setUniforms();
	void renderInMenu();
};

class SkyBoxNode : public SceneNode {
public: 
	SkyBoxNode();
	SkyBoxNode(const char* name);
	~SkyBoxNode();

	void render(Camera* camera);
	void renderInMenu();

};



#endif