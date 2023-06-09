#version 330 core
out vec4 FragColor;

in vec2 TexCoords;
in vec3 FragPos;

struct PointLight {
    vec3 position;

    vec3 specular;
    vec3 diffuse;
    vec3 ambient;

    float constant;
    float linear;
    float quadratic;
};

struct SpotLight {
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;

    float constant;
    float linear;
    float quadratic;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct DirLight {
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

#define NUM_LIGHTS 6
uniform sampler2D texture1;
uniform vec3 viewPosition;
uniform PointLight pointLight[NUM_LIGHTS];
uniform SpotLight spotLight[NUM_LIGHTS];
uniform DirLight dirLight;

vec4 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    // vec3 reflectDir = reflect(-lightDir, normal);
    // float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    // attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    // blending
    vec4 texColor = vec4(texture(texture1, TexCoords));
    if(texColor.a < 0.1)
        discard;
    // combine results
    vec4 ambient = vec4(light.ambient, 1.0) * texColor;
    vec4 diffuse = vec4(light.diffuse, 1.0) * diff * texColor;
    vec4 specular = vec4(light.specular, 1.0) * spec * texColor;
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
}

vec4 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
 {
     vec3 lightDir = normalize(light.position - fragPos);
     // diffuse shading
     float diff = max(dot(normal, lightDir), 0.0);
     // specular shading
     vec3 halfwayDir = normalize(lightDir + viewDir);
     float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
     // attenuation
     float distance = length(light.position - fragPos);
     float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
     // spotlight intensity
     float theta = dot(lightDir, normalize(-light.direction));
     float epsilon = light.cutOff - light.outerCutOff;
     float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
     // blending
     vec4 texColor = vec4(texture(texture1, TexCoords));
     if(texColor.a < 0.1)
         discard;
     // combine results
     vec4 ambient = vec4(light.ambient, 1.0) * texColor;
     vec4 diffuse = vec4(light.diffuse, 1.0) * diff * texColor;
     vec4 specular = vec4(light.specular, 1.0) * spec * texColor;
     ambient *= attenuation * intensity;
     diffuse *= attenuation * intensity;
     specular *= attenuation * intensity;
     return (ambient + diffuse + specular);
 }

vec4 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(-light.direction);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    // vec3 reflectDir = reflect(-lightDir, normal);
    // float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec4 texColor = vec4(texture(texture1, TexCoords));
    if(texColor.a < 0.1)
        discard;
    // combine results
    vec4 ambient = vec4(light.ambient, 1.0) * texColor;
    vec4 diffuse = vec4(light.diffuse, 1.0) * diff * texColor;
    vec4 specular = vec4(light.specular, 1.0) * spec * texColor;
    return (ambient + diffuse + specular);
}


void main()
{
    vec3 normal = vec3(0.0f, 1.0f, 0.0f);
    vec3 viewDir = normalize(viewPosition - FragPos);
    vec4 result = CalcDirLight(dirLight, normal, viewDir);
    for(int i = 0; i < 6; i++)
    {
        result += CalcPointLight(pointLight[i], normal, FragPos, viewDir);
        result += CalcSpotLight(spotLight[i], normal, FragPos, viewDir);
    }

    FragColor = result;
}