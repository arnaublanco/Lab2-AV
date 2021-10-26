#include "material.h"
#include "texture.h"
#include "application.h"
#include "extra/hdre.h"

StandardMaterial::StandardMaterial()
{
	color = vec4(1.f, 1.f, 1.f, 1.f);
}

StandardMaterial::~StandardMaterial()
{

}

PBRMaterial::PBRMaterial() {
}

PBRMaterial::~PBRMaterial() {

}

void PBRMaterial::setUniforms(Camera* camera, Matrix44 model) {
	shader->setUniform("u_viewprojection", camera->viewprojection_matrix);
	shader->setUniform("u_camera_position", camera->eye);
	shader->setUniform("u_model", model);
	shader->setUniform("u_time", Application::instance->time);
	shader->setUniform("u_output", Application::instance->output);

	shader->setUniform("u_color", color);
	shader->setUniform("u_exposure", Application::instance->scene_exposure);

	shader->setUniform("u_albedo", albedo, 0);
	shader->setUniform("u_metalness", metalness, 1);
	shader->setUniform("u_roughness", roughness, 2);

	shader->setUniform("u_camera_position", camera->eye);

	shader->setUniform("u_roughness_factor", roughness_factor);
	shader->setUniform("u_metalness_factor", metalness_factor);

	shader->setUniform("u_LUT", LUT, 3);

	shader->setUniform("u_texture", HDREs[0], 4);
	shader->setUniform("u_texture_prem_0", HDREs[1], 5);
	shader->setUniform("u_texture_prem_1", HDREs[2], 6);
	shader->setUniform("u_texture_prem_2", HDREs[3], 7);
	shader->setUniform("u_texture_prem_3", HDREs[4], 8);
	shader->setUniform("u_texture_prem_4", HDREs[5], 9);

}

void PBRMaterial::renderInMenu() {
	ImGui::ColorEdit3("Color", (float*)&color); // Edit 3 floats representing a color
	ImGui::SliderFloat("Roughness", &roughness_factor, 0.0f, 1.0f);
	ImGui::SliderFloat("Metalness", &metalness_factor, 0.0f, 1.0f);
}

void StandardMaterial::setUniforms(Camera* camera, Matrix44 model)
{
	//upload node uniforms
	shader->setUniform("u_viewprojection", camera->viewprojection_matrix);
	shader->setUniform("u_camera_position", camera->eye);
	shader->setUniform("u_model", model);
	shader->setUniform("u_time", Application::instance->time);
	shader->setUniform("u_output", Application::instance->output);

	shader->setUniform("u_color", color);
	shader->setUniform("u_exposure", Application::instance->scene_exposure);

	
}

void StandardMaterial::render(Mesh* mesh, Matrix44 model, Camera* camera)
{
	if (mesh && shader)
	{
		//enable shader
		shader->enable();

		//upload uniforms
		setUniforms(camera, model);

		//do the draw call
		mesh->render(GL_TRIANGLES);

		//disable shader
		shader->disable();
	}
}

void StandardMaterial::renderInMenu()
{
	ImGui::ColorEdit3("Color", (float*)&color); // Edit 3 floats representing a color
}

WireframeMaterial::WireframeMaterial()
{
	color = vec4(1.f, 1.f, 1.f, 1.f);
}

WireframeMaterial::~WireframeMaterial()
{

}

void WireframeMaterial::render(Mesh* mesh, Matrix44 model, Camera * camera)
{
	if (shader && mesh)
	{
		glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);

		//enable shader
		shader->enable();

		//upload material specific uniforms
		setUniforms(camera, model);

		//do the draw call
		mesh->render(GL_TRIANGLES);

		glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
	}
}