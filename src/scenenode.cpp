#include "scenenode.h"
#include "application.h"
#include "texture.h"
#include "utils.h"
#include <stdio.h>
#include <string.h>

unsigned int SceneNode::lastNameId = 0;
unsigned int mesh_selected = 0;
unsigned int background_selected = 0;
const char* backgrounds[3] = { "data/environments/pisa.hdre", "data/environments/san_giuseppe_bridge.hdre", "data/environments/panorama.hdre" };
const char* meshes[4] = { "data/meshes/sphere.obj.mbin", "data/models/helmet/helmet.obj.mbin", "data/models/bench/bench.obj.mbin", "data/models/lantern/lantern.obj.mbin" };
const char* albedos[4] = { "data/models/ball/albedo.png", "data/models/helmet/albedo.png", "data/models/bench/albedo.png", "data/models/lantern/albedo.png" };
const char* metals[4] = { "data/models/ball/metalness.png","data/models/helmet/metalness.png", "data/models/bench/metalness.png", "data/models/lantern/metalness.png" };
const char* roughs[4] = { "data/models/ball/roughness.png","data/models/helmet/roughness.png", "data/models/bench/roughness.png", "data/models/lantern/roughness.png" };
const char* emiss[4] = { "data/models/ball/emissive.png", "data/models/helmet/emissive.png", "data/models/bench/emissive.png", "data/models/lantern/emissive.png" };
const char* normals[4] = { "data/models/ball/normal.png", "data/models/helmet/normal.png", "data/models/bench/normal.png", "data/models/lantern/normal.png" };
int hdre_changed = 0;


SceneNode::SceneNode()
{
	this->name = std::string("Node" + std::to_string(lastNameId++));
}


SceneNode::SceneNode(const char * name)
{
	this->name = name;
}

SceneNode::~SceneNode()
{

}

void SceneNode::render(Camera* camera)
{
	if (material)
		material->render(mesh, model, camera);
}

void SceneNode::renderWireframe(Camera* camera)
{
	WireframeMaterial mat = WireframeMaterial();
	mat.render(mesh, model, camera);
}

void SceneNode::renderInMenu()
{
	//Model edit
	if (ImGui::TreeNode("Model")) 
	{
		float matrixTranslation[3], matrixRotation[3], matrixScale[3];
		ImGuizmo::DecomposeMatrixToComponents(model.m, matrixTranslation, matrixRotation, matrixScale);
		ImGui::DragFloat3("Position", matrixTranslation, 0.1f);
		ImGui::DragFloat3("Rotation", matrixRotation, 0.1f);
		ImGui::DragFloat3("Scale", matrixScale, 0.1f);
		ImGuizmo::RecomposeMatrixFromComponents(matrixTranslation, matrixRotation, matrixScale, model.m);
		
		ImGui::TreePop();
	}

	//Material
	if (material && ImGui::TreeNode("Material"))
	{
		material->renderInMenu();
		ImGui::TreePop();
	}

	//Geometry
	if (mesh && ImGui::TreeNode("Geometry"))
	{
		bool changed = false;
		changed |= ImGui::Combo("Mesh/Model", (int*)&mesh_selected, "SPHERE\0HELMET\0BENCH\0LANTERN\0");

		if (changed) {
			mesh = Mesh::Get(meshes[mesh_selected]);
			if (material) {
				PBRMaterial* mat = (PBRMaterial*)material; //downcasting
				mat->albedo = Texture::Get(albedos[mesh_selected]);
				mat->metalness = Texture::Get(metals[mesh_selected]);
				mat->roughness = Texture::Get(roughs[mesh_selected]);
				mat->normal = Texture::Get(normals[mesh_selected]);
				mat->emissive = Texture::Get(emiss[mesh_selected]);
			}
		}
		ImGui::TreePop();

	}
}

Light::Light(const char* name) {
	this->name = name;
	this->color = vec4(0.f, 0.f, 0.f, 0.f);
}

Light::~Light() {

}

void Light::setUniforms() {
	material->shader->setUniform("u_light_pos", model.getTranslation());
	material->shader->setUniform("u_light_intensity", light_intensity);
}

void Light::renderInMenu() {
	if (ImGui::TreeNode("Model"))
	{
		float matrixTranslation[3], matrixRotation[3], matrixScale[3];
		ImGuizmo::DecomposeMatrixToComponents(model.m, matrixTranslation, matrixRotation, matrixScale);
		ImGui::DragFloat3("Position", matrixTranslation, 0.1f);
		ImGui::DragFloat3("Rotation", matrixRotation, 0.1f);
		//ImGui::DragFloat3("Scale", matrixScale, 0.1f);
		ImGuizmo::RecomposeMatrixFromComponents(matrixTranslation, matrixRotation, matrixScale, model.m);

		ImGui::TreePop();
	}

	if (ImGui::TreeNode("Properties")) {
		ImGui::SliderFloat("Intensity", (float*)&light_intensity, 0.0f, 1.0f);
		ImGui::TreePop();
	}
}

SkyBoxNode::SkyBoxNode() {
	this->name = std::string("SkyBox" + std::to_string(lastNameId++));
}

SkyBoxNode::SkyBoxNode(const char* name) {
	this->name = name;

}
SkyBoxNode::~SkyBoxNode() {

}

void SkyBoxNode::render(Camera* camera) {
	if (material) {
		glDisable(GL_DEPTH_TEST);
		material->render(mesh, model, camera);
		material->shader->enable();
		material->shader->setUniform("u_texture", material->texture, 10);
		material->shader->disable();
		glEnable(GL_DEPTH_TEST);
	}

}

void SkyBoxNode::renderInMenu() {

	bool changed = false;
	changed |= ImGui::Combo("Background", (int*)&background_selected, "PISA\0GIUSEPPE_BRIDGE\0PANORAMA");

	if (changed) {
		HDRE* hdre = HDRE::Get(backgrounds[background_selected]);
		material->texture->cubemapFromHDRE(hdre,0);
		hdre_changed = 1;
	}
}
