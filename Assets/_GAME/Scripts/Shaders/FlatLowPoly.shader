// Casual low-poly shader for mobile (URP).
// - Flat (faceted) shading computed from screen-space derivatives: mesh normals are not used at all,
//   so faces stay hard even on smooth-imported meshes.
// - No shadow casting/receiving, no normal maps.
// - Colored "shadow" side instead of darkening to black, height gradient, faceted rim, saturation boost.
// Shading code lives in FlatLowPolyCore.hlsl (shared with URP/FlatLowPolyDissolve).
Shader "URP/FlatLowPoly"
{
    Properties
    {
        [Header(Base)]
        [MainTexture] _BaseMap("Base Map (palette)", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        [Toggle(_VERTEX_COLOR_ON)] _UseVertexColor("Use Vertex Color", Float) = 0

        [Header(Lighting)]
        [Toggle(_FIXED_LIGHT_ON)] _UseFixedLight("Use Fixed Light Direction", Float) = 1
        _LightDirection("Fixed Light Direction (world)", Vector) = (0.4, 0.8, -0.45, 0)
        _LightColorInfluence("Main Light Color Influence", Range(0,1)) = 0
        _ShadeColor("Shade Color", Color) = (0.55, 0.5, 0.85, 1)
        _ShadeStrength("Shade Strength", Range(0,1)) = 0.6
        _Wrap("Light Wrap", Range(0,1)) = 0.5
        _Bands("Bands (0 = smooth)", Range(0,6)) = 0
        _AdditionalLightsStrength("Point/Spot Lights Strength", Range(0,4)) = 1

        [Header(Rim)]
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimStrength("Rim Strength", Range(0,1)) = 0.25
        _RimPower("Rim Power", Range(0.5,8)) = 3

        [Header(Height Gradient)]
        _GradientColor("Bottom Color", Color) = (0.6, 0.55, 0.9, 1)
        _GradientStrength("Strength", Range(0,1)) = 0
        _GradientMinY("Bottom Y (world)", Float) = 0
        _GradientMaxY("Top Y (world)", Float) = 2

        [Header(Color Grading)]
        _Saturation("Saturation", Range(0,2)) = 1.2
        _Brightness("Brightness", Range(0,2)) = 1.05

        [Header(Emission)]
        [Toggle(_EMISSION)] _UseEmission("Use Emission", Float) = 0
        [NoScaleOffset] _EmissionMap("Emission Map (Base Map UV)", 2D) = "white" {}
        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "FlatForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex FlatVert
            #pragma fragment FlatFrag

            #pragma shader_feature_local _VERTEX_COLOR_ON
            #pragma shader_feature_local _FIXED_LIGHT_ON
            #pragma shader_feature_local _EMISSION
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "FlatLowPolyCore.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "FlatLowPolyCore.hlsl"
            ENDHLSL
        }
    }

    FallBack Off
}
