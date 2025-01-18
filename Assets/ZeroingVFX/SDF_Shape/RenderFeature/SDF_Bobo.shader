Shader "Gumou/RF/SDF_Bobo"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("_Color", Color) = (1,1,1,1)
        _MatCap ("MatCap", 2D) = "white" {}

    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma target 4.5
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

           

            StructuredBuffer<Shape> shapes;
            int numShapes;


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
            sampler2D _SourceTex;
            sampler2D _MatcapTex;
            float4 _Color,_Specular;
            float _Reflection;
            float _MaxStep;
            float _Epsilon;
            float _Warp,_WarpScale;
            float _DepthBlend;

            float4x4 IVP;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }


            float GetShapeDistance(Shape shape, float3 p)
            {
                float dis = 0;
                if (shape.shapeType == 0)
                {
                    dis = sdSphere(p - shape.position, shape.size.x);
                }
                else if (shape.shapeType == 1)
                {
                    dis = sdBox(p - shape.position, shape.size);
                }
                else if (shape.shapeType == 2)
                {
                    dis = sdCapsule(p - shape.position, shape.size, shape.custom.rgb, shape.custom.a);
                }

                float3 noiseP = p*_WarpScale;
                noiseP.y += _Time.y * 0.5;
                dis += SimplexNoise( noiseP ) * 0.02 * _Warp;

                return dis;
            }
       
            float4 SceneInfoCol(float3 p,float3 depthPos,out float3 col)
            {
                float dst = GetShapeDistance(shapes[0], p);
                col = shapes[0].colour;
                
                for (int i = 1; i < numShapes; i++)
                {
                    float newdst = GetShapeDistance(shapes[i], p);
                    float3 newcol = shapes[i].colour;
                    float4 coldst = CombineDistanceColor(dst, newdst,col,newcol, shapes[i]);
                    dst = coldst.a;
                    col = coldst.rgb;
                }
                float dstDepth = length(p - depthPos)  ;
                // dstD , 和depth混合后的距离, 因为depth无处不在所以最后会所有地方都有 volume,所以需要一个sdf volume和depth的渐变alpha
                float dstD = opSmoothUnion(dstDepth,dst,_DepthBlend);
                // sdf volume和depth的渐变alpha
                float aa = 0.5 + 0.5*(dst-dstDepth)/_DepthBlend;
                aa = (1-saturate(aa)) * 2;

                return float4(aa, dst, dstDepth, dstD);
            }
            float4 SceneInfo(float3 p,float3 depthPos)
            {
                float dst = GetShapeDistance(shapes[0], p);

                for (int i = 1; i < numShapes; i++)
                {
                    float newdst = GetShapeDistance(shapes[i], p);
                    dst = CombineDistance(dst, newdst, shapes[i]);
                }
                float dstDepth = length(p - depthPos)  ;
                // dstD , 和depth混合后的距离, 因为depth无处不在所以最后会所有地方都有 volume,所以需要一个sdf volume和depth的渐变alpha
                float dstD = opSmoothUnion(dstDepth,dst,_DepthBlend);
                // sdf volume和depth的渐变alpha
                float aa = 0.5 + 0.5*(dst-dstDepth)/_DepthBlend;
                aa = (1-saturate(aa)) * 2;
                return float4(aa,0,0,dstD);
            }

            float3 EstimateNormal(float3 p,float3 depthPos)
            {
                float x = SceneInfo(float3(p.x + _Epsilon, p.y, p.z),depthPos).w - SceneInfo(float3(p.x - _Epsilon, p.y, p.z),depthPos).w;
                float y = SceneInfo(float3(p.x, p.y + _Epsilon, p.z),depthPos).w - SceneInfo(float3(p.x, p.y - _Epsilon, p.z),depthPos).w;
                float z = SceneInfo(float3(p.x, p.y, p.z + _Epsilon),depthPos).w - SceneInfo(float3(p.x, p.y, p.z - _Epsilon),depthPos).w;
                return normalize(float3(x, y, z));
            }
            
            float4 Raymarch(float3 rayPos, float3 rayDir, float3 depthPos, out float3 outPos,out float3 outCol)
            {
                float4 result = 0;
                for (int i = 0; i < _MaxStep; i++)
                {
                    if ( length(rayPos-_WorldSpaceCameraPos) > length(depthPos-_WorldSpaceCameraPos)) {
                        break;
                    }
                    float4 si = SceneInfoCol(rayPos,depthPos,outCol);
                    float ds = si.w;
                    rayPos += rayDir * ds;
                    if (ds <= _Epsilon)
                    {
                        result = si.x;
                        result.rgb = EstimateNormal(rayPos,depthPos);
                        outPos = rayPos;
                        break;
                    }
                    
                }
                // density = saturate(density);
                return result;
            }
            float4 Raymarch(float3 rayPos, float3 rayDir,float3 depthPos,out float3 worldPos)
            {
                float4 result = float4(0, 0, 0, 0);
                for (int i = 0; i < _MaxStep; i++)
                {
                    if ( length(rayPos-_WorldSpaceCameraPos) > length(depthPos-_WorldSpaceCameraPos)) {
                        break;
                    }
                    float4 si = SceneInfo(rayPos,depthPos);
                    float dis = si.w;
                    rayPos += rayDir * dis;
                    if (dis < _Epsilon)
                    {
                        result = si.x;
                        result.rgb = EstimateNormal(rayPos,depthPos);
                        worldPos = rayPos;
                        break;
                    }
                }

                return result;
            }


            half4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float depth = SampleSceneDepth(i.uv);
                float4 H = float4(uv * 2 - 1, depth, 1);
                float4 D = mul(IVP, H);
                D = D / D.w;
                float3 depthPos = D.rgb;
                float3 rayDir = normalize(depthPos - _WorldSpaceCameraPos);
                float3 rayPos = _WorldSpaceCameraPos;
                float3 worldPos = 0;
                float3 sdfCol ;

                float4 rm = Raymarch(rayPos, rayDir,depthPos,worldPos,sdfCol);

                float distance = length(worldPos - _WorldSpaceCameraPos);
                float3 normal = rm.rgb;
                half2 vNormal = mul(unity_WorldToCamera,normal).xy;  //VIEW SPACE NORMAL

                float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
                Light light = GetMainLight(shadowCoord, worldPos, 1);
                half3 diffuseColor =  LightingLambert2(light,  normal);
                half3 specular =  LightingSpecular2(light,  normal,viewDir,_Specular,_Specular.a);

                //env col
                float3 irradiance = SampleSH(normal);
                float3 specularEnvCol =GlossyEnvironmentReflection(reflect(-viewDir,normal), (1-_Reflection), 1.0);

                half3 finalCol = (diffuseColor + ( lerp(irradiance,specularEnvCol,_Reflection) )*_Reflection)  * _Color *sdfCol  + specular;

                float dFade = saturate(length(depthPos - worldPos)/0.5) ;
                float2 bgUV =  i.uv + vNormal * 0.1 * exp(-distance*0.2) * dFade;
                float4 bgCol =  tex2D(_SourceTex,bgUV);

                //matcap
                half4 mc = tex2D(_MatcapTex,vNormal*0.5+0.5);
                mc.rgb *= mc.a;
                finalCol += mc.rgb;
                

                half4 col =  _Color;
                col.rgb = lerp(bgCol.rgb,finalCol.rgb,_Color.a * sdfCol);
                col.a = rm.a;
                
                return col;
            }
            ENDHLSL
        }


        Pass{
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _MainTex;
            sampler2D _SourceTex;


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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_SourceTex, i.uv);
                half4 sdf = tex2D(_MainTex, i.uv);
                col.rgb = sdf.rgb * sdf.a + col.rgb * (1 - sdf.a);
                return half4(col.rgb, 1);
            }
            ENDHLSL
        }




    }
}