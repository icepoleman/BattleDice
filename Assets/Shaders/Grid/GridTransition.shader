Shader "Hidden/GridTransition"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0, 0, 0, 1)
        _Step ("Step", Range(0, 1)) = 0
        [Toggle] _FadeIn ("Fade In", Float) = 0
        _EffectSize ("EffectSize", Float) = 10
        _Rotation ("Rotation (Deg)", Range(-180, 180)) = 0
        _ColumnSpeedBias ("Column Speed Bias", Range(0, 3)) = 1
        _CenterYSpeedBias ("Center Y Speed Bias", Range(0, 3)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
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
            fixed4 _Color;
            float _Step;
            float _FadeIn;
            float _EffectSize;
            float _Rotation;
            float _ColumnSpeedBias;
            float _CenterYSpeedBias;

            float2 Rotate2D(float2 p, float radians)
            {
                float s = sin(radians);
                float c = cos(radians);
                return float2(c * p.x - s * p.y, s * p.x + c * p.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 依螢幕像素切出格子，_EffectSize = 每格大約幾像素
                float cellSize = max(_EffectSize, 1.0);
                float2 cellCount = _ScreenParams.xy / cellSize;
                float2 gridUV = i.uv * cellCount;
                float2 cellId = floor(gridUV);
                float2 localUV = frac(gridUV);

                // 旋轉（以格子中心為原點）
                float2 centered = localUV - 0.5;
                centered = Rotate2D(centered, radians(_Rotation));

                // 越右邊變化越快：左欄速度 1，右欄速度 1 + bias
                float colT = cellId.x / max(cellCount.x - 1.0, 1.0);
                float xSpeed = 1.0 + colT * _ColumnSpeedBias;

                // 越靠近 Y 軸中間變化越慢：中間 1，上下邊緣 1 + bias
                float rowCenter = (cellCount.y - 1.0) * 0.5;
                float yEdgeT = saturate(abs(cellId.y - rowCenter) / max(rowCenter, 1.0));
                float ySpeed = 1.0 + yEdgeT * _CenterYSpeedBias;

                float localStep = saturate(_Step * xSpeed * ySpeed);

                // 菱形：以中心為原點的曼哈頓距離 |x|+|y|
                float cover = abs(centered.x) + abs(centered.y);

                // FadeOut：菱形放大變黑；FadeIn：菱形放大變透明（右到左揭開）
                bool showBlack = (_FadeIn > 0.5)
                    ? (cover > localStep)
                    : (cover <= localStep);

                if (!showBlack)
                    discard;

                return _Color;
            }
            ENDCG
        }
    }
}
