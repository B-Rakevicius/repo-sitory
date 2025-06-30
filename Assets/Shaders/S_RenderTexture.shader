Shader "Custom/S_RenderTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TintColor("Tint Color", Color) = (1,1,1,1)
        
        // Noise
        _NoiseScale("Noise Scale", Float) = 2
        _NoiseIntensity("Noise Intensity", Range(0, 5)) = 1
        _NoiseFrequency("Noise Frequency", Range(0.1, 10)) = 2
        _NoiseThreshold("Noise Threshold", Range(0,1)) = 0
        _NoiseColor("Noise Color", Color) = (1,1,1,1)

        
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        Zwrite On

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Includes/lygia/generative/worley.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _TintColor;
            
            float _NoiseScale;
            float _NoiseIntensity;
            float _NoiseFrequency;
            float _NoiseThreshold;
            float4 _NoiseColor;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.vertex = TransformObjectToHClip(IN.vertex);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv * _NoiseScale;
                // Sample render texture
                float4 col = tex2D(_MainTex, IN.uv);

                // Turn colors to grayscale
                float3 grayscale = (col.r+col.b+col.g) * _TintColor;

                // Calculate noise
                float2 noise;
                noise.x = worley(uv*0.4) * _NoiseIntensity;
                noise.y = worley(uv*0.2) * _NoiseIntensity;
                float finalNoise = noise.x+noise.y;
                finalNoise = saturate(finalNoise - _NoiseThreshold);
                _NoiseColor *= finalNoise;

                grayscale += _NoiseColor;
                
                return float4(grayscale, col.a);
            }
            ENDHLSL
        }
    }
}
