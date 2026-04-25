Shader "UI/ShinyButton_DiagonalFlash"
{
    Properties
    {
        [MainTexture]_MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _BaseColor ("Base Color", Color) = (0.2, 0.4, 1, 1)
        _GlowColor ("Glow Color", Color) = (0.6, 0.8, 1, 1)

        _ShineColor ("Shine Color", Color) = (1,1,1,1)
        _Width ("Width", Range(0.01, 0.5)) = 0.15
        _Softness ("Softness", Range(0.01, 0.5)) = 0.1
        _Intensity ("Intensity", Range(0,5)) = 1.5

        _HoverAmount ("Hover", Range(0,1)) = 0
        _ManualProgress ("Progress", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _BaseColor;
                half4 _GlowColor;
                half4 _ShineColor;
                half _Width;
                half _Softness;
                half _Intensity;
                half _HoverAmount;
                half _ManualProgress;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = TRANSFORM_TEX(input.uv, _MainTex);
                o.color = input.color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 baseCol = tex * i.color * _Color;

                float2 uv = i.uv;

                // diagonal direction
                float diag = (uv.x + uv.y) * 0.5;

                float pos = diag - _ManualProgress;

                float d = abs(pos - 0.5);

                // soft wide glow
                float glow = 1.0 - smoothstep(_Width, _Width + _Softness, d);

                // sharper inner shine
                float core = 1.0 - smoothstep(_Width * 0.3, _Width * 0.3 + 0.02, d);

                float shine = (glow * 0.6 + core) * _Intensity * _HoverAmount;

                float3 color = _BaseColor.rgb;

                // add subtle base glow
                color += _GlowColor.rgb * glow * 0.3;

                // add main shine
                color += _ShineColor.rgb * shine;

                return float4(color, baseCol.a);
            }
            ENDHLSL
        }
    }
}