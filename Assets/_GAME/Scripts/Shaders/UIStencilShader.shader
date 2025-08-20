Shader "UI/WriteStencil"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _StencilRef ("Stencil Ref", Range(0,255)) = 1
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_StencilRef]
            Comp Always
            Pass Replace
            ZFail Keep
            Fail Keep
        }

        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask 0

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local __ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"


            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Cutoff;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
                float4 worldPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.worldPos = v.vertex;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Поддержка RectMask2D
                #ifdef UNITY_UI_CLIP_RECT
                if (UnityGet2DClipping(i.worldPos, _ClipRect) < 0)
                    discard;
                #endif

                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                // Пишем в трафарет только там, где альфа спрайта выше порога
                clip(c.a - _Cutoff);

                // Цвет не важен (ColorMask 0), но что-то вернуть надо
                return 0;
            }
            ENDHLSL
        }
    }
}