Shader "Unlit/Outline"
{
    Properties
    {
        _Color ("Outline Color", Color) = (0,0, 0, 1)
        _Scale("Scale", Range(0,100)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 100

        Cull Back ZWrite Off  ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
             #pragma multi_compile _CAMERAOPAQUETEXTURE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

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

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
            float4 _CameraOpaqueTexture_TexelSize;
            float4 _Color;
            float _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 alphaBlend(float4 top, float4 bottom)
            {
                float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
                float alpha = top.a + bottom.a * (1 - top.a);
                return float4(color, alpha);
            }

            float getNormalSobel(float2 uv, float2 offset)
            {
                float3 n0 = SampleSceneNormals(uv + float2(-offset.x, -offset.y)).xyz;
                float3 n1 = SampleSceneNormals(uv + float2(0, -offset.y)).xyz;
                float3 n2 = SampleSceneNormals(uv + float2(offset.x, -offset.y)).xyz;
                float3 n3 = SampleSceneNormals(uv + float2(-offset.x, 0)).xyz;
                float3 n4 = SampleSceneNormals(uv + float2(offset.x, 0)).xyz;
                float3 n5 = SampleSceneNormals(uv + float2(-offset.x, offset.y)).xyz;
                float3 n6 = SampleSceneNormals(uv + float2(0, offset.y)).xyz;
                float3 n7 = SampleSceneNormals(uv + float2(offset.x, offset.y)).xyz;

                float3 sobelH = (n2 + 2 * n4 + n7) - (n0 + 2 * n3 + n5);
                float3 sobelV = (n0 + 2 * n1 + n2) - (n5 + 2 * n6 + n7);

                return length(sobelH) + length(sobelV);
            }

            float4 frag (v2f i) : SV_Target
            {
              
              float minOffset = 1.0 / 3840.0; // ~1 pixel at 1080p, adjust as needed
                float2 offset = max(_CameraOpaqueTexture_TexelSize.xy * _Scale, float2(minOffset, minOffset));
                float2 uv = i.uv;

                // Depth sobel
                float depth0 = SampleSceneDepth(uv + float2(-offset.x, -offset.y));
                float depth1 = SampleSceneDepth(uv + float2(0, -offset.y));
                float depth2 = SampleSceneDepth(uv + float2(offset.x, -offset.y));
                float depth3 = SampleSceneDepth(uv + float2(-offset.x, 0));
                float depth4 = SampleSceneDepth(uv + float2(offset.x, 0));
                float depth5 = SampleSceneDepth(uv + float2(-offset.x, offset.y));
                float depth6 = SampleSceneDepth(uv + float2(0, offset.y));
                float depth7 = SampleSceneDepth(uv + float2(offset.x, offset.y));

                float depthVertical = (depth0 + 2 * depth1 + depth2) - (depth5 + 2 * depth6 + depth7);
                float depthHorizontal = (depth2 + 2 * depth4 + depth7) - (depth0 + 2 * depth3 + depth5);
                // Add thresholds
                float depthThreshold = 0.05; // tweakable — lower = more sensitive
                float normalThreshold = 0.05;   // tweakable — lower = more sensitive

                // Edge logic
                float depthEdge = max(0, abs(depthVertical) + abs(depthHorizontal) - depthThreshold);
                float normalEdge = max(0, getNormalSobel(uv, offset) - normalThreshold);

                float edgeStrength = depthEdge  + normalEdge ;
                float combinedEdge = saturate(edgeStrength *.4 ); // amplify for visibility
                float4 outline = float4(_Color.rgb, combinedEdge * _Color.a);
         

                float4 sceneColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv);
                return  alphaBlend(outline, sceneColor);
            }

            ENDHLSL
        }
    }
}
