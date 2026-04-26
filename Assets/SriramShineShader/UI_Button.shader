Shader "UI/ShinyButton_OneShot_Border_Hard"
{
    Properties
    {
        [MainTexture]_MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _TopColor ("Top Color", Color) = (0.4, 0.6, 1, 1)
        _BottomColor ("Bottom Color", Color) = (0.1, 0.2, 0.5, 1)

        _ShineColor ("Shine Color", Color) = (1,1,1,1)
        _ShineWidth ("Shine Width", Range(0.01, 0.5)) = 0.1
        _ShineStrength ("Shine Strength", Range(0,5)) = 1.0

        _BorderColor ("Border Color", Color) = (1,1,1,1)
        _BorderWidth ("Border Width", Range(0.001, 0.1)) = 0.04

        _HoverAmount ("Hover Amount", Range(0,1)) = 0
        _ManualProgress ("Manual Progress", Range(0,1)) = 0
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
                half4 _TopColor;
                half4 _BottomColor;
                half4 _ShineColor;
                half4 _BorderColor;
                half _ShineWidth;
                half _ShineStrength;
                half _BorderWidth;
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

                // base gradient
                half gradient = smoothstep(0.0h, 1.0h, i.uv.y);
                half3 baseColor = lerp(_BottomColor.rgb, _TopColor.rgb, gradient);

                // shine
                float x = i.uv.x - _ManualProgress;
                float d = abs(x - 0.5);

                half band = 1.0h - smoothstep(_ShineWidth, _ShineWidth + 0.02h, d);
                half shine = band * _ShineStrength * _HoverAmount;

                // HARD BORDER (no smoothing = guaranteed visible)
                half border =
                    step(i.uv.x, _BorderWidth) +
                    step(i.uv.y, _BorderWidth) +
                    step(1.0 - i.uv.x, _BorderWidth) +
                    step(1.0 - i.uv.y, _BorderWidth);

                border = saturate(border);

                // make border react to shine
                half borderGlow = border * (0.5h + shine);

                half3 rgb = baseColor;
                rgb += _ShineColor.rgb * shine;

                // FORCE border color on top
                rgb = lerp(rgb, _BorderColor.rgb, borderGlow);

                return half4(rgb, baseCol.a);
            }
            ENDHLSL
        }
    }
}