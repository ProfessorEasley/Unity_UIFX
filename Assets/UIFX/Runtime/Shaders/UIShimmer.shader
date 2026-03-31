// UIFX/UIShimmer
// Additive overlay shader for click-triggered shimmer effects on UGUI components.
// Designed to be used on a transparent child Image that sits above the base component.
// Blend mode is additive (One One) — brightens whatever is beneath it.

Shader "UIFX/UIShimmer"
{
    Properties
    {
        // [PerRendererData] tells UGUI to manage this per-Image rather than per-material.
        // The overlay Image's sprite texture is passed here automatically at draw time.
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _ShimmerColor    ("Shimmer Color",    Color)            = (1, 1, 1, 1)
        _ShimmerProgress ("Shimmer Progress", Float)            = -0.3
        _ShimmerWidth    ("Shimmer Width",    Range(0.01, 0.5)) = 0.15
        _ShimmerAngle    ("Shimmer Angle",    Float)            = 30
        _Intensity       ("Intensity",        Range(0, 3))      = 1.5

        // Standard UGUI masking/stencil properties — managed by the Canvas/Mask system.
        _StencilComp     ("Stencil Comparison", Float) = 8
        _Stencil         ("Stencil ID",         Float) = 0
        _StencilOp       ("Stencil Operation",  Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask",  Float) = 255
        _ColorMask       ("Color Mask",         Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull     Off
        Lighting Off
        ZWrite   Off
        ZTest    [unity_GUIZTestMode]
        Blend    One One        // Additive: shimmer brightens anything beneath it
        ColorMask[_ColorMask]

        Pass
        {
            Name "UIShimmer"

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            fixed4    _ShimmerColor;
            float     _ShimmerProgress;
            float     _ShimmerWidth;
            float     _ShimmerAngle;
            float     _Intensity;
            float4    _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex        = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord      = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color         = v.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Sample sprite for alpha masking — shimmer only shows within the sprite shape
                half4 mainTex = tex2D(_MainTex, IN.texcoord);

                // Project UV onto the sweep axis using the configured angle
                float rad       = _ShimmerAngle * UNITY_PI / 180.0;
                float sweepCoord = IN.texcoord.x * cos(rad) + IN.texcoord.y * sin(rad);

                // Normalize so progress 0→1 always maps across the full rect regardless of angle
                float normFactor = abs(cos(rad)) + abs(sin(rad));
                sweepCoord /= max(normFactor, 0.001);

                // Soft-edged band: ramps up to _ShimmerProgress then ramps back down
                float p    = _ShimmerProgress;
                float w    = max(_ShimmerWidth, 0.001);
                float band = smoothstep(p - w, p, sweepCoord)
                           - smoothstep(p, p + w, sweepCoord);

                // Combined alpha: sprite shape * vertex alpha (CanvasGroup, etc.)
                float alpha = mainTex.a * IN.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                alpha *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(alpha - 0.001);
                #endif

                // RGB additive output; alpha 0 so the blend equation doesn't affect dst alpha
                return fixed4(_ShimmerColor.rgb * band * _Intensity * alpha, 0);
            }
            ENDCG
        }
    }
}
