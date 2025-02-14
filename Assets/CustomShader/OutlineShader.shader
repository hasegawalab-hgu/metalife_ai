Shader "Unlit/OutlineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineThickness ("Outline Thickness", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            ZWrite Off
            ZTest LEqual
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha // 透明度を考慮したブレンド

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
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
            fixed4 _OutlineColor;
            float _OutlineThickness;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // メインテクスチャを取得
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // 不透明な部分はそのまま返す
                if (col.a > 0.0)
                {
                    return col; // 元の色をそのまま返す
                }

                // 輪郭の厚さが0の場合、元のテクスチャの色をそのまま返す
                if (_OutlineThickness <= 0.0)
                {
                    return fixed4(0, 0, 0, 0); // 透明な色を返す
                }

                // 輪郭の厚さを調整するためのオフセット
                float2 offset = _OutlineThickness * float2(1.0 / _MainTex_ST.x, 1.0 / _MainTex_ST.y);

                // 透明な部分の周囲をサンプリングして、輪郭を描く条件を設定
                float alpha = tex2D(_MainTex, i.uv + float2(-offset.x, 0)).a +
                              tex2D(_MainTex, i.uv + float2(offset.x, 0)).a +
                              tex2D(_MainTex, i.uv + float2(0, -offset.y)).a +
                              tex2D(_MainTex, i.uv + float2(0, offset.y)).a;

                // 輪郭を描く条件
                if (alpha > 0.0)
                {
                    return _OutlineColor; // 輪郭の色
                }

                return fixed4(0, 0, 0, 0); // 透明な色
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
