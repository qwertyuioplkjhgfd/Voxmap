#include <iostream>

#include "../libs/glad/glad.h"
#include <GLFW/glfw3.h>

#include "../libs/learnopengl.h"
#define STB_IMAGE_IMPLEMENTATION
#include "../libs/stb_image.h"

void framebuffer_size_callback(GLFWwindow *window, int width, int height);
void processInput(GLFWwindow *window);

// settings
<<<<<<< HEAD
const unsigned int SCR_WIDTH = 320;
const unsigned int SCR_HEIGHT = 180;
=======
const unsigned int SCR_WIDTH = 480;
const unsigned int SCR_HEIGHT = 270;
const int TAA = 2;
>>>>>>> 08291db... Implement trashy TAA

int main() {
  // glfw: initialize and configure
  // ------------------------------
  glfwInit();
  glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
  glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
  glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

#ifdef __APPLE__
  glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
#endif

  // glfw window creation
  // --------------------
  GLFWwindow *window =
      glfwCreateWindow(SCR_WIDTH, SCR_HEIGHT, "VoxMap", NULL, NULL);
  if (window == NULL) {
    std::cout << "Failed to create GLFW window" << std::endl;
    glfwTerminate();
    return -1;
  }
  glfwMakeContextCurrent(window);
  glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);

  // glad: load all OpenGL function pointers
  // ---------------------------------------
  if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress)) {
    std::cout << "Failed to initialize GLAD" << std::endl;
    return -1;
  }

  // build and compile our shader zprogram
  // ------------------------------------
  Shader ourShader("src/shader.vs", "src/shader.fs");

  // set up vertex data (and buffer(s)) and configure vertex attributes
  // ------------------------------------------------------------------
  float vertices[] = {// positions
                      1.0f,  1.0f,  0.0f, 1.0f,  -1.0f, 0.0f,
                      -1.0f, -1.0f, 0.0f, -1.0f, 1.0f,  0.0f};
  unsigned int indices[] = {
      0, 1, 3, // first triangle
      1, 2, 3  // second triangle
  };
  unsigned int VBO, VAO, EBO;
  glGenVertexArrays(1, &VAO);
  glGenBuffers(1, &VBO);
  glGenBuffers(1, &EBO);

  glBindVertexArray(VAO);

  glBindBuffer(GL_ARRAY_BUFFER, VBO);
  glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

  glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO);
  glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices,
               GL_STATIC_DRAW);

  // position attribute
  glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(float), (void *)0);
  glEnableVertexAttribArray(0);
  int width, height, nrChannels;

  // load and create a texture
  // -------------------------
<<<<<<< HEAD
  unsigned int texture1;
  stbi_set_flip_vertically_on_load(
      true); // tell stb_image.h to flip loaded texture's on the y-axis.
  // texture 1
  // ---------
  glGenTextures(1, &texture1);
  glBindTexture(GL_TEXTURE_2D, texture1);
  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
  unsigned char *data =
      stbi_load("maps/texture.png", &width, &height, &nrChannels, 0);
  glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA,
               GL_UNSIGNED_BYTE, data);
  stbi_image_free(data);

  // tell opengl for each sampler to which texture unit it belongs to (only has
  // to be done once)
  // -------------------------------------------------------------------------------------------
  ourShader.use(); // don't forget to activate/use the shader before setting
                   // uniforms!
  ourShader.setInt("texture1", 0);
  ourShader.setVec2("iResolution", SCR_WIDTH, SCR_HEIGHT);

  glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
  glActiveTexture(GL_TEXTURE0);
  glBindTexture(GL_TEXTURE_2D, texture1);
  glBindVertexArray(VAO);
  ourShader.use();
  glClear(GL_COLOR_BUFFER_BIT);
=======
  unsigned int mapTexture;
  int width, height, nrChannels;
  // tell stb_image.h to flip loaded texture's on the y-axis.
  stbi_set_flip_vertically_on_load(true);
  glGenTextures(1, &mapTexture);
  glBindTexture(GL_TEXTURE_2D, mapTexture);
  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
  unsigned char *data = stbi_load("maps/texture.png", &width, &height, &nrChannels, 0);
  glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB8UI, width, height, 0, GL_RGB_INTEGER, GL_UNSIGNED_BYTE, data);
  stbi_image_free(data);

  // shader configuration
  // --------------------
  marchShader.use();
  marchShader.setVec2("iResolution", SCR_WIDTH/TAA, SCR_HEIGHT/TAA);
  marchShader.setInt("mapTexture", 0);

  filterShader.use();
  filterShader.setInt("marchTexture0", 0);
  filterShader.setInt("marchTexture1", 1);
  filterShader.setInt("marchTexture2", 2);
  filterShader.setInt("marchTexture3", 3);

  // framebuffer configuration
  // -------------------------
  unsigned int frameBuffer[TAA*TAA];
  unsigned int frameTexture[TAA*TAA];
  GLenum textureEnum[TAA*TAA] = { 
    GL_TEXTURE0, GL_TEXTURE1, GL_TEXTURE2, GL_TEXTURE3 
  };
  for(int i = 0; i < TAA*TAA; i++) {
    unsigned int buffer;
    glGenFramebuffers(1, &buffer);
    glBindFramebuffer(GL_FRAMEBUFFER, buffer);
    // create a color attachment texture
    unsigned int texture;
    glGenTextures(1, &texture);
    glBindTexture(GL_TEXTURE_2D, texture);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, SCR_WIDTH, SCR_HEIGHT, 0, GL_RGB, GL_UNSIGNED_BYTE, NULL);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, texture, 0);
    // now that we actually created the framebuffer and added all attachments we want to check if it is actually complete now
    if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
      std::cout << "ERROR::FRAMEBUFFER:: Framebuffer is not complete!" << std::endl;
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    frameBuffer[i] = buffer;
    frameTexture[i] = texture;
  }

  // draw as wireframe
  //glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);


>>>>>>> 08291db... Implement trashy TAA
  // render loop
  // -----------
  float deltaTime = 0.0f;
  float lastFrame = 0.0f;
  float t = glfwGetTime();
  unsigned int frame = 0;
  unsigned int frameBufferIndex = 0;
  while (!glfwWindowShouldClose(window)) {
    processInput(window);
<<<<<<< HEAD

    // render container
    float t = glfwGetTime() / 3;
<<<<<<< HEAD
    ourShader.setFloat("iTime", t);
    ourShader.setVec3("camRot", t, t, t);
    ourShader.setVec3("camPos", t - 100, t, t + 6);

    glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);
=======
=======
    t = glfwGetTime();
    frame++;
    frameBufferIndex = frame % (TAA*TAA);
>>>>>>> 08291db... Implement trashy TAA
    // uniforms!

    // render
    // ------
    // bind to framebuffer and draw scene as we normally would to color texture
    glBindFramebuffer(GL_FRAMEBUFFER, frameBuffer[frameBufferIndex]);
    marchShader.use();
    marchShader.setVec2("iResolution", SCR_WIDTH, SCR_HEIGHT);
    marchShader.setFloat("iTime", t);
    marchShader.setVec3("iCamRot", 0, 0, t/10.);
    marchShader.setVec3("iCamPos", -200, 0, 3.5);
    glActiveTexture(GL_TEXTURE0);
    glBindTexture(GL_TEXTURE_2D, mapTexture);
    glBindVertexArray(quadVertexArray);
    glDrawArrays(GL_TRIANGLES, 0, 6);

    // now bind back to default framebuffer and draw a quad plane with the attached framebuffer color texture
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    filterShader.use();
    filterShader.setInt("iFrame", frame);
    filterShader.setInt("iTAA", TAA);

    glBindVertexArray(quadVertexArray);
    // use the color attachment texture as the texture of the quad plane
    for(int i = 0; i < TAA*TAA; i++){
      glActiveTexture(textureEnum[i]);
      glBindTexture(GL_TEXTURE_2D, frameTexture[i]);
    }
    glDrawArrays(GL_TRIANGLES, 0, 6);
>>>>>>> 0e15432... Fix camera rotation

    // glfw: swap buffers and poll IO events (keys pressed/released, mouse moved
    // etc.)
    // -------------------------------------------------------------------------------
    glfwSwapBuffers(window);
    glfwPollEvents();
  }

  // optional: de-allocate all resources once they've outlived their purpose:
  // ------------------------------------------------------------------------
  glDeleteVertexArrays(1, &VAO);
  glDeleteBuffers(1, &VBO);
  glDeleteBuffers(1, &EBO);

  // glfw: terminate, clearing all previously allocated GLFW resources.
  // ------------------------------------------------------------------
  glfwTerminate();
  return 0;
}

// process all input: query GLFW whether relevant keys are pressed/released this
// frame and react accordingly
// ---------------------------------------------------------------------------------------------------------
void processInput(GLFWwindow *window) {
  if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
    glfwSetWindowShouldClose(window, true);
}

// glfw: whenever the window size changed (by OS or user resize) this callback
// function executes
// ---------------------------------------------------------------------------------------------
void framebuffer_size_callback(GLFWwindow *window, int width, int height) {
  // make sure the viewport matches the new window dimensions; note that width
  // and height will be significantly larger than specified on retina displays.
  glViewport(0, 0, width, height);
}
