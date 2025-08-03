Shader "Lit/ToyShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (.25,.7, .25, 1)
        _Shine ("Shine Intensity", Float ) = 1.0
        _Rim("Rim Intensity", Float ) = 1.0
        _SecondaryColor ("Secondary Color", Color) = (.25,.5, .7, 1)

        _PixelSize ("Pixel Size", Float) = 0.01
    }
    SubShader
    {
        Tags { 
                "RenderType"="Opaque"
                "LightMode" = "UniversalForward"
                "RenderPipeline" = "UniversalPipeline" }

      

       Cull Off ZWrite On

        LOD 100

                Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS); // safer than raw TransformObjectToWorldNormal
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 normal = SafeNormalize(IN.normalWS); // avoids NaNs
                float2 packedNormal = PackNormalOctQuadEncode(normal);
                return float4(packedNormal, 0.0, 0.0);
            }
            ENDHLSL
        }


        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _NORMALMAP

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv: TEXCOORD0;

                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                
                float2 uv: TEXCOORD0;
                half3 lightAmount : TEXCOORD2;
                float4 positionWS  : TEXCOORD1;
                float3 worldNormal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _BaseColor;
            float4 _SecondaryColor;
            float _Shine;
            float _Rim;
            float _PixelSize;

            float4 GetDiffuse()
            {

            }

            float4 GetSpecular()
            {

            }

            float4 GetAmbient()
            {

            }

            

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = mul(unity_ObjectToWorld, IN.positionOS);

                // Get the VertexNormalInputs of the vertex, which contains the normal in world space
                VertexNormalInputs positions = GetVertexNormalInputs(IN.positionOS);

                // Get the properties of the main light
                Light light = GetMainLight();

                OUT.worldNormal = mul(unity_ObjectToWorld, IN.normal);
                // Calculate the amount of light the vertex receives
                OUT.lightAmount = LightingLambert(light.color, light.direction, positions.normalWS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Set the fragment color to the interpolated amount of light
                Light light = GetMainLight();
                // Diffuse 
                float3 normal =  normalize(IN.worldNormal);
                float3 lightDir = normalize(light.direction);
                float NdotL = saturate(dot(normal,lightDir));
                float toonDiffuse = floor(NdotL * 3) / 2.0; // quantize into 4 bands
                toonDiffuse = saturate(toonDiffuse);
               
                float3 viewDir =  normalize(_WorldSpaceCameraPos.xyz - IN.positionWS.xyz);
                //Specular
                 float3 halfway =  SafeNormalize((lightDir + viewDir));
                 float specularPower = lerp(50, 150, saturate(_Shine));
                float spec = pow(saturate(dot(normal, halfway)), specularPower);
                spec *= _Shine;
                 spec *= NdotL*_Shine;

                //Rim
                float NDotV  = dot(normal,  viewDir );
                float rim = 1- saturate(NDotV);
                rim *= pow(_Rim, rim)*NdotL ;

                float3 rimColor = _SecondaryColor.rgb * rim;
            



                float2 pixelUV = floor(IN.uv / _PixelSize) * _PixelSize;
                pixelUV = saturate(pixelUV); 
                half4 sample = tex2D(_MainTex, pixelUV);


               float3 diffuseColor = _BaseColor * NdotL;
                float3 specularColor = _SecondaryColor.rgb * spec;
                float3 color = light.color * (toonDiffuse + spec + rim);
                return float4 (sample.rgb * color, 1) ;
            }
            ENDHLSL
        }

        
    }
}
