Shader "LB/Dissolve"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}  // Основная текстура
        _DissolveTex ("Dissolve Texture", 2D) = "gray" {}  // Текстура для распада
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0  // Управление эффектом
        _MaxDistance ("MaxDistance", Range(0, 1)) = 1 
        _Center ("Center", Vector) = (0, 0, 0)
        _EdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1)  // Цвет свечения на границе
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05  // Толщина границы свечения
    }

    SubShader
    {
        Tags { "Queue"="Geometry" } // Вместо "Transparent"
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _DissolveTex;
            float _MaxDistance;
            float4 _Center;
            float _DissolveAmount;
            float4 _EdgeColor;
            float _EdgeWidth;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // Координаты в мировом пространстве
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 c = _Center * 0.001;
                c.w = 1;
                float3 center = mul(unity_ObjectToWorld ,c ).xyz;
                float distanceToCenter = length(i.worldPos - center);
                float normalizedDistance = distanceToCenter / _MaxDistance;
                float dissolveMask = tex2D(_DissolveTex, i.uv).r;
                
                float dissolveValue = normalizedDistance + dissolveMask * 0.2;

                float max = _DissolveAmount + _EdgeWidth;
                float edge = smoothstep(_DissolveAmount, max, dissolveValue);
                
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 edgeColor = _EdgeColor * (1 - edge);

                if (dissolveValue > max)
                    discard;
                col = lerp(col, edgeColor, step(_DissolveAmount, dissolveValue));

                return col;
            }
            ENDCG
        }
    }
}