Shader "Hidden/ToyShaderPostProcess"
{
   Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color Tint", Color) = (1, 1, 1, 1)
        _Shine ("Shine Intensity", Float) = 1.0
        _Rim("Rim Intensity", Float) = 1.0
        _SecondaryColor ("Secondary Color", Color) = (.25,.5, .7, 1)
        _PixelSize ("Pixel Size", Float) = 0.01
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        
        Pass
        {
            Name "CelShadingPost"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            float4 _BaseColor;
            float4 _SecondaryColor;
            float _Shine;
            float _Rim;
            float _PixelSize;
            
            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                
                // Pixelation effect
                float2 pixelatedUV = uv;
                if (_PixelSize > 0.001)
                {
                    pixelatedUV = floor(uv / _PixelSize) * _PixelSize;
                }
                
                // Sample the original screen color (with objects' textures)
                half4 originalColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, pixelatedUV);
                
                // Sample depth
                float depth = SampleSceneDepth(uv);
                
                // Check if this is background
                #if UNITY_REVERSED_Z
                    bool isBackground = depth < 0.0001;
                #else
                    bool isBackground = depth > 0.9999;
                #endif
                
                if (isBackground)
                {
                    return originalColor;
                }
                
                // Sample world normal
                float3 normal = SampleSceneNormals(uv);
                float normalLength = length(normal);
                
                if (normalLength < 0.1)
                {
                    return originalColor;
                }
                
                normal = normalize(normal);
                
                // Reconstruct world position
                float3 worldPos = ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP);
                
                // Get main light
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 viewDir = normalize(GetCameraPositionWS() - worldPos);
                
                // Cel-shaded diffuse
                float NdotL = saturate(dot(normal, lightDir));
                
                // Quantize into bands
                float celBands = 2.0;
                float celDiffuse = floor(NdotL * celBands) / celBands;
                celDiffuse = max(celDiffuse, 0.2); // Minimum ambient
                
                // Specular
                float3 halfVector = normalize(lightDir + viewDir);
                float NdotH = saturate(dot(normal, halfVector));
                float specularPower = lerp(50.0, 150.0, _Shine);
                float specular = pow(NdotH, specularPower);
                specular = step(0.5, specular) * _Shine;
                specular *= step(0.3, NdotL); // Only on lit areas
                
                // Rim lighting
                float NdotV = saturate(dot(normal, viewDir));
                float rim = 1.0 - NdotV;
                rim = pow(rim, 3.0);
                rim *= _Rim * step(0.3, NdotL);
                
                // Combine lighting
                float3 lighting = celDiffuse * mainLight.color;
                float3 specularColor = _SecondaryColor.rgb * specular;
                float3 rimColor = _SecondaryColor.rgb * rim;
                
                // Multiply original color with cel shading lighting
                float3 finalColor = originalColor.rgb ;
                
                
                // Apply color tint
                finalColor *= _BaseColor.rgb;
                
                return half4(finalColor, originalColor.a);
            }
            ENDHLSL
        }
    }
}