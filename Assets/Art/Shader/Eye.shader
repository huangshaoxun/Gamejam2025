Shader "Unlit/Eye"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor("Base Col",Color) = (1.0,1.0,1.0,1.0)
        _PositionRangeY("PositionRangeY",Range(-1,1)) = 0
        _PositionRangeX("PositionRangeX",Range(-1,1)) = 0
        _PositionScaleX("PositionScaleX",Range(0,1)) = 1
        _PositionScaleY("PositionScaleY",Range(0,1)) = 1
        _StencilRef("StencilRef", Range(0, 255)) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("StencilComp", Float) = 8
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            Stencil
				{
					Ref[_StencilRef]
					Comp[_StencilComp]
					Pass replace
				}
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
            half4 _BaseColor;
            float4 _MainTex_ST;
            float _PositionRangeY;
            float _PositionRangeX;
            float _PositionScaleX;
            float _PositionScaleY;

            v2f vert (appdata v)
            {
                v2f o;
                v.vertex.xy += float2(_PositionRangeX*_PositionScaleX,_PositionRangeY*_PositionScaleY);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _BaseColor;
               
                return col;
            }
            ENDCG
        }
    }
}
