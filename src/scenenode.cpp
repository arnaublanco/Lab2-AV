#include "scenenode.h"
#include "application.h"
#include "texture.h"
#include "utils.h"

unsigned int SceneNode::lastNameId = 0;
unsigned int mesh_selected = 0;
unsigned int background_selected = 0;
const char* backgrounds[3] = { "data/environments/snow", "data/environments/city", "data/environments/dragonvale" };
const char* meshes[2] = { "data/meshes/sphere.obj.mbin", "data/models/helmet/helmet.obj.mbin"};
const char* albedos[2] = { "data/models/ball/albedo.png", "data/models/helmet/albedo.png" };
const char* metals[2] = { "data/models/ball/metalness.png","data/models/helmet/metalness.png" };
const char* roughs[2] = { "data/models/ball/roughness.png","data/models/helmet/roughness.png" };


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
		changed |= ImGui::Combo("Mesh/Model", (int*)&mesh_selected, "SPHERE\0HELMET\0");

		if (changed) {
			mesh = Mesh::Get(meshes[mesh_selected]);
			PBRMaterial* mat = (PBRMaterial*)material; //downcasting
			mat->albedo = Texture::Get(albedos[mesh_selected]);
			mat->metalness = Texture::Get(metals[mesh_selected]);
			mat->roughness = Texture::Get(roughs[mesh_selected]);

		}
		ImGui::TreePop();

	}
}

Light::Light(const char* name) {
	this->name = name;
	this->color = vec4(1.f, 1.f, 1.f, 1.f);
}

Light::~Light() {

}

void Light::setUniforms() {
	material->shader->setUniform("u_light_pos", model * Vector4(position, 1.0).xyz);
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
		ImGui::ColorEdit3("Color", (float*)&color);
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
		glEnable(GL_DEPTH_TEST);
	}
}

void SkyBoxNode::renderInMenu() {

	bool changed = false;
	changed |= ImGui::Combo("Background", (int*)&background_selected, "SNOW\0CITY\0DRAGON VALE");

	if (changed) {
		material->texture->cubemapFromImages(backgrounds[background_selected]);
	}
}
