Shader "UI/TransitionStripeDrop"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0, 0, 0, 1)
        _Progress ("Progress", Range(0, 1)) = 0
        _Softness ("Softness", Range(0.001, 0.2)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _MainTex_ST;
            float _Progress;
            float _Softness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;
                float progress = saturate(_Progress);
                // Push each edge past the center by the feather width so the fades overlap instead of leaving a seam.
                float doorWidth = progress * (0.5 + _Softness);

                float leftMask = 1.0 - smoothstep(doorWidth - _Softness, doorWidth, uv.x);
                float rightDistance = 1.0 - uv.x;
                float rightMask = 1.0 - smoothstep(doorWidth - _Softness, doorWidth, rightDistance);

                float mask = saturate(max(leftMask, rightMask));
                float alpha = mask * i.color.a;

                return fixed4(i.color.rgb, alpha);
            }
            ENDCG
        }
    }
}
