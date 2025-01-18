Shader "Gumou/VFX/SDFTest"
{
    Properties {}
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Cull Front

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
                return o;
            }

            #define _MaxStep 50

            float sdSphere( float3 p, float s )
            {
              return length(p)-s;
            }
            float sdBox( float3 p, float3 b )
            {
                float3 q = abs(p) - b;
                return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
            }
            float opSmoothUnion( float d1, float d2, float k ) {
                float h = clamp( 0.5 + 0.5*(d2-d1)/k, 0.0, 1.0 );
                return lerp( d2, d1, h ) - k*h*(1.0-h); 
            }
            
            float SceneInfo(float3 p){
                float dis = sdSphere(p-float3(0,2,0),1);
                float dis2 = sdBox(p,float3(1,1,0.6));
                dis = opSmoothUnion(dis ,dis2, 0.5);
                return dis;
            }

            #define epsilon 0.02
            
            float3 EstimateNormal(float3 p)
            {
                float x = SceneInfo(float3(p.x + epsilon, p.y, p.z)) - SceneInfo(float3(p.x - epsilon, p.y, p.z));
                float y = SceneInfo(float3(p.x, p.y + epsilon, p.z)) - SceneInfo(float3(p.x, p.y - epsilon, p.z));
                float z = SceneInfo(float3(p.x, p.y, p.z + epsilon)) - SceneInfo(float3(p.x, p.y, p.z - epsilon));
                return normalize(float3(x, y, z));
            }
            
            float4 Raymarch(float3 rayPos,float3 rayDir,float3 worldCenter,out float3 worldPos){
                float4 result = float4(0,0,0,0);
                for (int i = 0; i < _MaxStep; i++) {
                    float dis = SceneInfo(rayPos - worldCenter);
                    rayPos += rayDir * dis;
                    if (dis < epsilon)
                    {
                        result = 1;
                        result.rgb = EstimateNormal(rayPos -worldCenter);
                        worldPos = rayPos;
                        break;
                    }
                }

                return result; 
            }

            half3 LightingLambert2( Light light, half3 normal){
                half3 attenuatedLightColor = light.color * light.distanceAttenuation * light.shadowAttenuation;
                half NdotL= saturate(dot(normal, light.direction));
                NdotL = max(0.33,NdotL);
                return NdotL * attenuatedLightColor;
            }


            half4 frag(v2f i) : SV_Target
            {
                float3 rayPos = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.worldPos - _WorldSpaceCameraPos);
                float3 worldCenter = mul( unity_ObjectToWorld, float4(0,0,0,1) ).xyz;
                float3 worldPos = 0;

                float4 rm = Raymarch(rayPos,rayDir,worldCenter,worldPos);

                float3 normal = rm.rgb;
                half2 vNormal = mul(unity_WorldToCamera,normal).xy;  //VIEW SPACE NORMAL

                float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
                Light light = GetMainLight(shadowCoord, worldPos, 1);
                half3 diffuseColor =  LightingLambert2(light,  normal);

                clip(rm.a -0.1);
                return half4(diffuseColor.rgb, 1);
            }
            ENDHLSL
        }
    }
}