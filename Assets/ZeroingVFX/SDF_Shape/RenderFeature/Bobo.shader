Shader "Unlit/Bobo"
{
    Properties
    {
        _MatcapTex ("Matcap Texture", 2D) = "white" {}
        _Reflection("Reflection",Float) = 0.7
        _BaseColor("Base Col ",Color) = (1.0,1.0,1.0,1.0)
        [HDR]_Specular("Specular Colr",Color) = (1,1,1,1)
        _Scale("Scale",Float)= 0
        _WrapScale("WrapScale",Float)= 0.01
        _Size("Size",Float)= 1
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline""Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include"Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "gVolume.hlsl"
            #include "gNoise.hlsl"
            #include "gLighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal :NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS :TEXCOORD2;
            };

            //sampler2D _MainTex;
            sampler2D _MatcapTex;
            float4 _MainTex_ST;
            float _Reflection;
            half4 _BaseColor;
            float4 _SpecularColor;
            half _Scale;
            half _WrapScale;
            float _Size;

            v2f vert (appdata v)
            {
                v2f o;
                float3 position = v.vertex +_Time.y * 0.5;
                v.vertex.xy +=v.normal.xy*_Scale +SimplexNoise(position)*_WrapScale;
                o.vertex = TransformObjectToHClip(v.vertex);
                
                o.uv = v.uv;
                o.normalWS = TransformObjectToWorldNormal(v.normal);
                o.positionWS = TransformObjectToWorld(v.vertex.xyz);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float3 newNormal = i.normalWS*pow(length(i.normalWS)*(1-(clamp(_Size,1,4)-1)/100),_Size);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.positionWS);
                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                Light light = GetMainLight(shadowCoord, i.positionWS, 1);
                half3 diffuseColor =  LightingLambert2(light,  newNormal);
                half3 specular =  LightingSpecular2(light,  newNormal,viewDir,_SpecularColor,_SpecularColor.a);
                //env col
                float3 irradiance = SampleSH(newNormal);
                float3 specularEnvCol =GlossyEnvironmentReflection(reflect(-viewDir,newNormal), (1-_Reflection), 1.0);

                half3 finalCol = (diffuseColor + ( lerp(irradiance,specularEnvCol,_Reflection) )*_Reflection)  * _BaseColor + specular;
                half2 vNormal = mul(unity_WorldToCamera,i.normalWS).xy;
                //half4 col = tex2D(_MainTex, i.uv);
                half4 mc = tex2D(_MatcapTex, vNormal*pow(length(vNormal)*(1-(clamp(_Size,1,4)-1)/100),pow(_Size,1.3))*0.5+0.5);
                half4 Col =half4(finalCol,1)+mc;
                Col.a = _BaseColor.a*mc.a;
                return Col;
            }
            ENDHLSL
        }
    }
}
