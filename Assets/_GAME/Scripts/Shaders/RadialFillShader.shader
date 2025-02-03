Shader "LB/RadialFill"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorEmpty("Color Empty", Color) = (1,1,1,1)
        _ColorFilled("Color Filled", Color) = (1,1,1,1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
        _AngleOffset ("AngleOffset", Range(0,360)) = 0
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off 
        ZWrite Off
        
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _ColorEmpty;
            fixed4 _ColorFilled;
            fixed _FillAmount;
            fixed _AngleOffset;
            float2 _Center;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 dir = i.uv - _Center;
                float angle = atan2(dir.y, dir.x) / UNITY_PI;
                angle = (angle + 1) * 0.5;
                float startOffset = _AngleOffset / 360; 
                angle = frac(angle - startOffset);

                fixed4 col = lerp(_ColorEmpty, _ColorFilled, step(angle, _FillAmount));
                return tex2D(_MainTex, i.uv) * col;
            }
            ENDCG
        }
    }
}