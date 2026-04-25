Shader "UI/HoverRippleClean"
{
    Properties
    {
        [MainTexture]_MainTex ("Sprite", 2D) = "white" {}
        [MainColor]_Color ("Tint", Color) = (1,1,1,1)

        _TopColor ("Top Color", Color) = (0.40, 0.62, 1.00, 1)
        _BottomColor ("Bottom Color", Color) = (0.12, 0.22, 0.55, 1)
        _RippleColor ("Ripple Color", Color) = (1,1,1,1)

        _RippleWidth ("Ripple Width", Range(0.01, 0.5)) = 0.08
        _RippleSoftness ("Ripple Softness", Range(0.01, 0.5)) = 0.12
        _RippleStrength ("Ripple Strength", Range(0, 5)) = 1.0
        _RippleMaxRadius ("Ripple Max Radius", Range(0.1, 1.5)) = 0.85

        _ManualProgress ("Progress", Range(0,1)) = 0
        _HoverAmount ("Hover Amount", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
        }

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
                half4 _TopColor;
                half4 _BottomColor;
                half4 _RippleColor;
                half _RippleWidth;
                half _RippleSoftness;
                half _RippleStrength;
                half _RippleMaxRadius;
                half _ManualProgress;
                half _HoverAmount;
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
                half4 tint = tex * i.color * _Color;

                half gradient = smoothstep(0.0h, 1.0h, i.uv.y);
                half3 baseCol = lerp(_BottomColor.rgb, _TopColor.rgb, gradient);

                half progress = saturate(_ManualProgress);

                half fadeIn = smoothstep(0.08h, 0.18h, progress);
                half fadeOut = 1.0h - smoothstep(0.82h, 1.0h, progress);
                half envelope = fadeIn * fadeOut * _HoverAmount;

                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                float radius = progress * _RippleMaxRadius;
                float ringDistance = abs(dist - radius);

                half ring = 1.0h - smoothstep(_RippleWidth, _RippleWidth + _RippleSoftness, ringDistance);
                half ripple = ring * envelope * _RippleStrength;

                half3 rgb = baseCol + (_RippleColor.rgb * ripple);

                return half4(rgb * tint.rgb, tint.a);
            }
            ENDHLSL
        }
    }
}