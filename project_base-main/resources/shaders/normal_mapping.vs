#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;

#define NUM_LIGHTS 6
out VS_OUT {
    vec3 FragPos;
    vec2 TexCoords;
    vec3 TangentLightPos[NUM_LIGHTS];
    vec3 TangentViewPos[NUM_LIGHTS];
    vec3 TangentFragPos[NUM_LIGHTS];
} vs_out;


struct LightsPos {
    vec3 direction[NUM_LIGHTS];
};

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

uniform LightsPos lightPos;
uniform vec3 viewPos;

void main()
{
    vs_out.FragPos = vec3(model * vec4(aPos, 1.0));
    vs_out.TexCoords = aTexCoords;

    mat3 normalMatrix = transpose(inverse(mat3(model)));
    vec3 T = normalize(normalMatrix * aTangent);
    vec3 N = normalize(normalMatrix * aNormal);
    T = normalize(T - dot(T, N) * N);
    vec3 B = cross(N, T);

    mat3 TBN = transpose(mat3(T, B, N));
    for(int i = 0; i < NUM_LIGHTS; i++)
    {
        vs_out.TangentLightPos[i] = TBN * lightPos.direction[i];
        vs_out.TangentViewPos[i]  = TBN * viewPos;
        vs_out.TangentFragPos[i]  = TBN * vs_out.FragPos;
    }

    gl_Position = projection * view * model * vec4(aPos, 1.0);
}