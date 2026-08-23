Shader "Custom/Monochrome"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MonochromeAmount ("Monochrome Amount", Range(0, 1)) = 0
        _MonochromeColor ("Monochrome Color", Color) = (1, 1, 1, 1)
        _WhiteThreshold ("White Threshold", Range(0, 1)) = 0.9
        _PreserveWhite ("Preserve White", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MonochromeAmount;
            float4 _MonochromeColor;
            float _WhiteThreshold;
            float _PreserveWhite;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // Check if color is pure white (all channels close to 1)
                float minChannel = min(min(texColor.r, texColor.g), texColor.b);
                float maxChannel = max(max(texColor.r, texColor.g), texColor.b);
                float isPureWhite = step(_WhiteThreshold, minChannel) * step(maxChannel, 1.0);
                
                // Calculate grayscale using standard luminance formula
                float gray = dot(texColor.rgb, fixed3(0.299, 0.587, 0.114));
                
                // Apply monochrome color
                fixed3 monoColor = gray * _MonochromeColor.rgb;
                
                // Blend between original and monochrome based on _MonochromeAmount
                fixed3 finalColor = lerp(texColor.rgb, monoColor, _MonochromeAmount);
                
                // If pure white and preserve white is enabled, keep it pure white
                finalColor = lerp(finalColor, fixed3(1.0, 1.0, 1.0), isPureWhite * _PreserveWhite);
                
                // Preserve original alpha and apply vertex color
                return fixed4(finalColor, texColor.a) * i.color;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
