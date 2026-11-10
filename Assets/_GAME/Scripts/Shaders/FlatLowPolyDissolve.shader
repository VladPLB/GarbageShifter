// URP/FlatLowPoly + radial dissolve (replacement for LB/Dissolve).
// Dissolve properties keep LB/Dissolve names and semantics, so SecondaryWeaponDisolver works as is:
// _DissolveAmount 0 = fully dissolved, 1 = fully visible (within _MaxDistance of _Center).
Shader "URP/FlatLowPolyDissolve"
{
    Properties
    {
        [Header(Base)]
        [MainTexture] _BaseMap("Base Map (palette)", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        [Toggle(_VERTEX_COLOR_ON)] _UseVertexColor("Use Vertex Color", Float) = 0

        [Header(Dissolve)]
        [NoScaleOffset] _DissolveTex("Dissolve Noise", 2D) = "gray" {}
        _NoiseScale("Noise Scale (tiles per meter)", Float) = 20
        _DissolveAmount("Dissolve Amount", Range(0,1)) = 1
        _MaxDistance("Max Distance", Range(0.001,1)) = 1
        _Center("Center (object space x1000)", Vector) = (0, 0, 0, 0)
        _NoiseStrength("Noise Strength", Range(0,1)) = 0.2
        [HDR] _EdgeColor("Edge Color", Color) = (1, 0.5, 0, 1)
        _EdgeWidth("Edge Width", Range(0,0.2)) = 0.05
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2

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
            "RenderType" = "TransparentCutout"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
        }

        Pass
        {
            Name "FlatForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_Cull]
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

            #define FLAT_LOWPOLY_DISSOLVE
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
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            #define FLAT_LOWPOLY_DISSOLVE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "FlatLowPolyCore.hlsl"
            ENDHLSL
        }
    }

    FallBack Off
}
