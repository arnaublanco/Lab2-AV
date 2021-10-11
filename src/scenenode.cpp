#include "scenenode.h"
#include "application.h"
#include "texture.h"
#include "utils.h"

unsigned int SceneNode::lastNameId = 0;
unsigned int mesh_selected = 0;
unsigned int background_selected = 0;
const char* backgrounds[3] = { "data/environments/snow", "data/environments/city", "data/environments/dragonvale" };
const char* meshes[2] = { "data/meshes/sphere.obj.mbin" };
const char* textures[2] = { "data/models/ball/metalness.png", "data/models/ball/roughness.png" };
unsigned int texture_selected = 0;


Light::Light(Vector3 position, Vector3 diffuseLight, Vector3 specularLight, Vector3 ambientLight) {
	this->position = position;
	this->ambientLight = ambientLight;
	this->diffuseLight = diffuseLight;
	this->specularLight = specularLight;
	
}

void Light::setUniforms(){
	material->shader->setUniform("light_pos", model*Vector4(position,1.0).xyz);
	material->shader->setUniform("ambientLight", ambientLight);
	material->shader->setUniform("diffuseLight", diffuseLight);
	material->shader->setUniform("specularLight", specularLight);
}

void Light::renderInMenu() {

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

	if (ImGui::TreeNode("Parameters")) {

		ImGui::DragFloat3("Ambient", (float*)&ambientLight, 0.01f, 0.0f, 1.0f);
		ImGui::DragFloat3("Diffuse", (float*)&diffuseLight, 0.01f, 0.0f, 1.0f);
		ImGui::DragFloat3("Specular", (float*)&specularLight, 0.01f, 0.0f, 1.0f);

		ImGui::TreePop();
	}
	
}


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
	if (material) {
		glEnable(GL_DEPTH_TEST);
		material->render(mesh, model, camera);
		glDisable(GL_DEPTH_TEST);
	}
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
		PhongMaterial* tmp = (PhongMaterial*)material;
		if (!tmp->isMirror) {
			bool changed = false;
			changed |= ImGui::Combo("Texture", (int*)&texture_selected, "TEXTURE1\0TEXTURE2");
			
			if (changed)
				material->texture = Texture::Get(textures[texture_selected]);
		}

		material->renderInMenu();

		ImGui::TreePop();
	}

	//Geometry
	if (mesh && ImGui::TreeNode("Geometry"))
	{
		bool changed = false;
		changed |= ImGui::Combo("Mesh", (int*)&mesh_selected, "SPHERE");

		if (changed)
			mesh = Mesh::Get(meshes[mesh_selected]);

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

void SkyBoxNode::render(Camera* camera)
{
	if (material) {
		glDisable(GL_DEPTH_TEST);
		material->render(mesh, model, camera);
		glEnable(GL_DEPTH_TEST);
	}
}

void SkyBoxNode::renderInMenu() {
	if (ImGui::TreeNode("Options")) {

		bool changed = false;
		changed |= ImGui::Combo("Background", (int*)&background_selected, "SNOW\0CITY\0DRAGON VALE");

		if (changed){
			material->texture->cubemapFromImages(backgrounds[background_selected]);
		}
		ImGui::TreePop();
	}
}
