Shader "Custom/SpriteOutline2D"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineSize ("Outline Size (px)", Range(0,16)) = 1
        _OutlinePulseSpeed ("Outline Pulse Speed", Range(0,10)) = 3
        _OutlineSoftness ("Outline Softness", Range(0,1)) = 0.65
        _FillThreshold ("Fill Threshold", Range(0,1)) = 0.01
        [Toggle] _OutlineEnabled ("Outline Enabled", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Sprite"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _OutlineColor;
            float _OutlineSize;
            float _OutlinePulseSpeed;
            float _OutlineSoftness;
            float _FillThreshold;
            float _OutlineEnabled;
            float4 _MainTex_TexelSize;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // 原本有圖就直接畫
                if (col.a > _FillThreshold)
                    return col;

                // 外框開關關閉時直接返回
                if (_OutlineEnabled < 0.5)
                    return col;

                float2 baseTexel = _MainTex_TexelSize.xy;
                float glowRadius = max(_OutlineSize, 0.0001);
                float2 offset1 = baseTexel * glowRadius;
                float2 offset2 = baseTexel * glowRadius * 2.0;
                float2 offset3 = baseTexel * glowRadius * 3.0;

                float alpha1 = 0;
                alpha1 = max(alpha1, tex2D(_MainTex, i.uv + float2(offset1.x, 0)).a);
                alpha1 = max(alpha1, tex2D(_MainTex, i.uv + float2(-offset1.x, 0)).a);
                alpha1 = max(alpha1, tex2D(_MainTex, i.uv + float2(0, offset1.y)).a);
                alpha1 = max(alpha1, tex2D(_MainTex, i.uv + float2(0, -offset1.y)).a);
                alpha1 = max(alpha1, tex2D(_MainTex, i.uv + float2(offset1.x, offset1.y)).a);
                alpha1 = max(alpha1, tex2D(_MainTex, i.uv + float2(-offset1.x, offset1.y)).a);
                alpha1 = max(alpha1, tex2D(_MainTex, i.uv + float2(offset1.x, -offset1.y)).a);
                alpha1 = max(alpha1, tex2D(_MainTex, i.uv + float2(-offset1.x, -offset1.y)).a);

                float alpha2 = 0;
                alpha2 = max(alpha2, tex2D(_MainTex, i.uv + float2(offset2.x, 0)).a);
                alpha2 = max(alpha2, tex2D(_MainTex, i.uv + float2(-offset2.x, 0)).a);
                alpha2 = max(alpha2, tex2D(_MainTex, i.uv + float2(0, offset2.y)).a);
                alpha2 = max(alpha2, tex2D(_MainTex, i.uv + float2(0, -offset2.y)).a);

                float alpha3 = 0;
                alpha3 = max(alpha3, tex2D(_MainTex, i.uv + float2(offset3.x, 0)).a);
                alpha3 = max(alpha3, tex2D(_MainTex, i.uv + float2(-offset3.x, 0)).a);
                alpha3 = max(alpha3, tex2D(_MainTex, i.uv + float2(0, offset3.y)).a);
                alpha3 = max(alpha3, tex2D(_MainTex, i.uv + float2(0, -offset3.y)).a);

                float softMix = saturate(_OutlineSoftness);
                float glowAlpha = alpha1 * 0.60 + alpha2 * 0.30 + alpha3 * 0.10;
                glowAlpha = smoothstep(0.0, 1.0, glowAlpha);
                glowAlpha = pow(glowAlpha, lerp(0.8, 2.5, softMix));

                if (glowAlpha > 0)
                {
                    float pulse = sin(_Time.y * _OutlinePulseSpeed);
                    float pulseAlpha = 0.85 + 0.15 * pulse;
                    float4 outlineCol = _OutlineColor;
                    outlineCol.a = glowAlpha * pulseAlpha;
                    return outlineCol;
                }

                return col;
            }
            ENDCG
        }
    }
}
