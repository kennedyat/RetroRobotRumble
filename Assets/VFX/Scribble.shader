Shader "Hidden/Scribble"
{
    Properties
    {
        _MainTex ("Current Frame", 2D) = "white" {}
        _NewTex ("Next Frame", 2D) = "white" {}
        _Progress ("Transition Progress", Range(0, 1)) = 0
        _Freq ("Scribble Frequency", Float) = 20
        _Amp ("Scribble Amplitude", Float) = 0.05

        _Height ("Scribble Amplitude", Float) = 0.05
        _Width ("Scribble Amplitude", Float) = 0.05

        
    }

    SubShader
    {
        Tags { "RenderType"="Transparent"  "RenderPipeline" = "UniversalPipeline"}

         ZTest Always Cull Front ZWrite Off
        Pass
        {
            Name "ScribbleTransition"
           

            HLSLPROGRAM
             #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            sampler2D _MainTex;
            sampler2D _NewTex;
            float _Progress;
            float _Freq;
            float _Amp;
            float _Height;
            float _Width;
            float3 scribble = 0;
            float xOffset=0;
             float4 _CameraOpaqueTexture_TexelSize;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = float4(input.positionOS.xy, 0.0, 1.0);
                output.uv = input.uv;
                return output;
            }
            float rand(float2 co)
        {
            return frac(sin(dot(co, float2(12.9898,78.233))) * 43758.5453);
        }

        // Rotate UV to apply a stronger, more consistent tilt
        float2 rotateUV(float2 uv, float angle)
        {
            float s = sin(angle);
            float c = cos(angle);
            uv -= 0.5;
            float2 rotated = float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
            return rotated + 0.5;
        }

        half4 frag(Varyings input) : SV_Target
        {
            float2 uv = input.uv;

            float4 current = tex2D(_MainTex, uv);
            float4 next = tex2D(_NewTex, uv);

            // Rotate UV for tilt
            float tiltAngle = radians(20); // try 15–45 degrees
            float2 tiltedUV = rotateUV(uv, tiltAngle);

            // Define segment index (controls where frequency switches)
            float segWidth = 0.1; // width of each sine section
            float segIndex = floor(tiltedUV.x / segWidth);

            // Generate a consistent frequency per segment
            float randomFreq = lerp(_Freq * 0.5, _Freq * 2.0, rand(float2(segIndex, 3.71)));
            float phase = _Progress * 10.0;

            // Compute sine wave based on segment frequency
            float yScribble = sin((tiltedUV.x) * randomFreq + phase) * _Amp + 0.5;

            // Hard-edged scribble line
            float lineThickness = 0.3*_Progress * 3;
            float mask = step(abs((tiltedUV.y - yScribble)*rand(uv)), lineThickness);

            float3 color = lerp(current.rgb, next.rgb, mask);
            return float4(color, 1);
        }
            ENDHLSL
        }
    }
}
