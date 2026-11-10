// Tiles a sprite texture across a UI quad and scrolls it by a UV offset.
// Used by ScrollingPatternBackground to give an infinitely repeating,
// resolution-independent scrolling background without stretching the source image.
Shader "UI/ScrollingTiledPattern"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        // Number of times the texture is repeated across the quad (fractional values are fine).
        _Tiles ("Tiles Across Quad", Vector) = (1,1,0,0)
        // Scroll offset measured in tiles. Wraps seamlessly, so it can grow forever.
        _Scroll ("Scroll Offset (tiles)", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float4 _Tiles;
            float4 _Scroll;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // frac() keeps the sample inside the sprite's 0..1 UV range, so the
                // pattern repeats seamlessly at any tile count and scroll offset.
                float2 uv = frac(i.texcoord * _Tiles.xy + _Scroll.xy);
                return tex2D(_MainTex, uv) * i.color;
            }
            ENDCG
        }
    }

    Fallback "UI/Default"
}
