// Shared code for URP/FlatLowPoly and URP/FlatLowPolyDissolve.
// Define FLAT_LOWPOLY_DISSOLVE before including to enable the dissolve variant.
#ifndef FLAT_LOWPOLY_CORE_INCLUDED
#define FLAT_LOWPOLY_CORE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    float4 _LightDirection;
    half _LightColorInfluence;
    half4 _ShadeColor;
    half _ShadeStrength;
    half _Wrap;
    half _Bands;
    half _AdditionalLightsStrength;
    half4 _RimColor;
    half _RimStrength;
    half _RimPower;
    half4 _GradientColor;
    half _GradientStrength;
    float _GradientMinY;
    float _GradientMaxY;
    half _Saturation;
    half _Brightness;
    half4 _EmissionColor;
#if defined(FLAT_LOWPOLY_DISSOLVE)
    float4 _Center;
    float _NoiseScale;
    float _MaxDistance;
    float _DissolveAmount;
    half _NoiseStrength;
    half _EdgeWidth;
    half4 _EdgeColor;
#endif
CBUFFER_END

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D(_EmissionMap);
SAMPLER(sampler_EmissionMap);

#if defined(FLAT_LOWPOLY_DISSOLVE)
TEXTURE2D(_DissolveTex);
SAMPLER(sampler_DissolveTex);

// Noise is projected in object space (scaled to world units, so it sticks to the moving object
// and keeps density regardless of scale) along the dominant axis of the face: one sample per pixel,
// independent of mesh UVs (palette atlases squash UVs into tiny islands).
half SampleDissolveNoise(float3 positionWS)
{
    float4x4 objectToWorld = GetObjectToWorldMatrix();
    float3 scale = float3(length(objectToWorld._m00_m10_m20), length(objectToWorld._m01_m11_m21), length(objectToWorld._m02_m12_m22));
    float3 p = TransformWorldToObject(positionWS) * scale * _NoiseScale;

    float3 faceNormal = abs(cross(ddx(p), ddy(p)));
    float2 uv = faceNormal.x > faceNormal.y && faceNormal.x > faceNormal.z ? p.yz
        : faceNormal.y > faceNormal.z ? p.xz
        : p.xy;
    return SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, uv).r;
}

// Radial dissolve from _Center (object space, stored x1000 for inspector convenience) with noise jitter.
float DissolveValue(float3 positionWS)
{
    float3 center = TransformObjectToWorld(_Center.xyz * 0.001);
    return length(positionWS - center) / _MaxDistance + SampleDissolveNoise(positionWS) * _NoiseStrength;
}

// Clips dissolved pixels and returns the edge mask (1 on the edge band, 0 on the intact surface).
half ApplyDissolve(float3 positionWS)
{
    float value = DissolveValue(positionWS);
    clip(_DissolveAmount + _EdgeWidth - value);
    return step(_DissolveAmount, value);
}
#endif

// ---------------------------------------------------------------- Lighting

// Wrapped (optionally banded) N.L shared by main and additional lights.
half FlatNdotL(float3 normalWS, half3 lightDir)
{
    half ndl = saturate((dot(normalWS, lightDir) + _Wrap) / (1 + _Wrap));
    if (_Bands > 0.5)
    {
        ndl = ceil(ndl * _Bands) / _Bands;
    }
    return ndl;
}

#if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
half3 FlatAdditionalLight(uint lightIndex, float3 positionWS, float3 normalWS)
{
    // Shadow mask of 1 = no shadows; cookies are applied when _LIGHT_COOKIES is on.
    Light light = GetAdditionalLight(lightIndex, positionWS, half4(1, 1, 1, 1));
    #if defined(_LIGHT_LAYERS)
    if (!IsMatchingLightLayer(light.layerMask, GetMeshRenderingLayer()))
    {
        return 0;
    }
    #endif
    return light.color * (light.distanceAttenuation * FlatNdotL(normalWS, light.direction));
}
#endif

// Point/spot (and extra directional) lights, same loop as URP Lit incl. Forward+.
// Vertex light mode is evaluated per pixel too: the flat normal only exists in the fragment shader.
half3 FlatAdditionalLights(float3 positionWS, float3 normalWS, float4 positionCS)
{
    half3 result = 0;
    #if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
    // LIGHT_LOOP_BEGIN expects an InputData named inputData (used by the Forward+ cluster iterator).
    InputData inputData = (InputData)0;
    inputData.positionWS = positionWS;
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(positionCS);

    #if USE_FORWARD_PLUS
    [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
        result += FlatAdditionalLight(lightIndex, positionWS, normalWS);
    }
    #endif

    uint pixelLightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(pixelLightCount)
        result += FlatAdditionalLight(lightIndex, positionWS, normalWS);
    LIGHT_LOOP_END
    #endif
    return result * _AdditionalLightsStrength;
}

// ---------------------------------------------------------------- Forward

struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    // Camera-relative world position: keeps derivatives precise far from the origin.
    float3 positionRWS : TEXCOORD1;
    half4 color : TEXCOORD2;
    half fogFactor : TEXCOORD3;
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings FlatVert(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS = positionInputs.positionCS;
    output.positionRWS = positionInputs.positionWS - _WorldSpaceCameraPos;
    output.uv = input.uv;
    output.color = input.color;
    output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
    return output;
}

half4 FlatFrag(Varyings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float3 positionWS = input.positionRWS + _WorldSpaceCameraPos;

    #if defined(FLAT_LOWPOLY_DISSOLVE)
    half edgeMask = ApplyDissolve(positionWS);
    #endif

    // Face normal from derivatives. Sign differs between graphics APIs,
    // so just orient it toward the camera (also makes back faces look right with Cull Off).
    float3 viewDirWS = -normalize(input.positionRWS);
    float3 normalWS = normalize(cross(ddx(input.positionRWS), ddy(input.positionRWS)));
    normalWS *= dot(normalWS, viewDirWS) < 0 ? -1 : 1;

    float2 baseUV = TRANSFORM_TEX(input.uv, _BaseMap);
    half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseUV) * _BaseColor;
    #if defined(_VERTEX_COLOR_ON)
    albedo *= input.color;
    #endif

    Light mainLight = GetMainLight();
    #if defined(_FIXED_LIGHT_ON)
    half3 lightDir = normalize(_LightDirection.xyz);
    #else
    half3 lightDir = mainLight.direction;
    #endif

    half ndl = FlatNdotL(normalWS, lightDir);

    // Shade side is tinted toward _ShadeColor instead of black: keeps colors juicy.
    half3 shade = lerp(1, _ShadeColor.rgb, _ShadeStrength * (1 - ndl));
    half3 lightColor = lerp(1, mainLight.color, _LightColorInfluence);
    half3 color = albedo.rgb * shade * lightColor;

    half gradientT = saturate((positionWS.y - _GradientMinY) / max(_GradientMaxY - _GradientMinY, 0.0001));
    color *= lerp(1, _GradientColor.rgb, _GradientStrength * (1 - gradientT));

    color += albedo.rgb * FlatAdditionalLights(positionWS, normalWS, input.positionCS);

    half rim = pow(1 - saturate(dot(normalWS, viewDirWS)), _RimPower) * _RimStrength;
    color += _RimColor.rgb * rim;

    half luma = dot(color, half3(0.299, 0.587, 0.114));
    color = max(0, lerp(luma, color, _Saturation)) * _Brightness;

    // Added after grading so HDR emission is not clamped/desaturated (feeds Bloom as is).
    #if defined(_EMISSION)
    color += SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, baseUV).rgb * _EmissionColor.rgb;
    #endif

    #if defined(FLAT_LOWPOLY_DISSOLVE)
    color = lerp(color, _EdgeColor.rgb, edgeMask);
    #endif

    color = MixFog(color, input.fogFactor);
    return half4(color, 1);
}

// ---------------------------------------------------------------- DepthOnly

struct DepthAttributes
{
    float4 positionOS : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct DepthVaryings
{
    float4 positionCS : SV_POSITION;
    #if defined(FLAT_LOWPOLY_DISSOLVE)
    float3 positionWS : TEXCOORD0;
    #endif
    UNITY_VERTEX_OUTPUT_STEREO
};

DepthVaryings DepthVert(DepthAttributes input)
{
    DepthVaryings output = (DepthVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS = positionInputs.positionCS;
    #if defined(FLAT_LOWPOLY_DISSOLVE)
    output.positionWS = positionInputs.positionWS;
    #endif
    return output;
}

half DepthFrag(DepthVaryings input) : SV_Target
{
    #if defined(FLAT_LOWPOLY_DISSOLVE)
    ApplyDissolve(input.positionWS);
    #endif
    return input.positionCS.z;
}

#endif
