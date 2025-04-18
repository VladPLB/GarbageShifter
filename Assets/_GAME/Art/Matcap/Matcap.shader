Shader "Custom/MatCap"
{
    Properties
    {
        _BumpMap ("Bumpmap (RGB)", 2D) = "bump" {}
        _MatCap ("MatCap (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        

        Pass
        {
            Name "Unlit"

            HLSLPROGRAM
            #pragma target 3.0
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define TANGENT_SPACE_ROTATION \
            float3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) ) * v.tangent.w; \
            float3x3 rotation = float3x3( v.tangent.xyz, binormal, v.normal )
            
            sampler2D _BumpMap;
            sampler2D _MatCap;
            
            // Struct for vertex data
            struct Attributes
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 tangent : TANGENT;
            };

            // Struct for interpolated data to fragment shader
            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 TtoV0 : TEXCOORD1;
                float3 TtoV1 : TEXCOORD2;
            };

            // Vertex shader
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.position = TransformObjectToHClip(v.position.xyz);
                o.uv = v.uv;

                TANGENT_SPACE_ROTATION;
                o.TtoV0 = mul(rotation, UNITY_MATRIX_IT_MV[0].xyz);
                o.TtoV1 = mul(rotation, UNITY_MATRIX_IT_MV[1].xyz);
                return o;
            }

            // Fragment shader
            half4 frag(Varyings i) : SV_Target
            {
                float3 normal = UnpackNormal(tex2D(_BumpMap, i.uv));
                
                half2 vn;
                vn.x = dot(i.TtoV0, normal);
                vn.y = dot(i.TtoV1, normal);
                
                float4 matcapLookup = tex2D(_MatCap, vn * 0.5 + 0.5);
                
                matcapLookup.a = 1;
                return matcapLookup * 2.0;
            }
            ENDHLSL
        }
    }
    Fallback "Diffuse"
}
