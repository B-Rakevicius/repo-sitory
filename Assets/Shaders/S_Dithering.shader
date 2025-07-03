Shader"Custom/S_Dithering"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Level("Bayer Level", Int) = 1
        _Spread("Spread", Range(0,1)) = 0.5
        
        _RedColorCount("Red Color Count", int) = 2
        _GreenColorCount("Green Color Count", int) = 2
        _BlueColorCount("Blue Color Count", int) = 2
        
        // Will probably need a color palette texture
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

    Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Threshold maps
            static const int bayer2[2*2] = {
                0, 2,
                3, 1
            };

            static const int bayer4[4*4] = {
                0, 8, 2, 10,
                12, 4, 14, 6,
                3, 11, 1, 9,
                15, 7, 13, 5
            };

            static const int bayer8[8*8] = {
                0, 32, 8, 40, 2, 34, 10, 42,
                48, 16, 56, 24, 50, 18, 58, 26,
                12, 44, 4, 36, 14, 46, 6, 38,
                60, 28, 52, 20, 62, 30, 54, 22,
                3, 35, 11, 43, 1, 33, 9, 41,
                51, 19, 59, 27, 49, 17, 57, 25,
                15, 47, 7, 39, 13, 45, 5, 37,
                63, 31, 55, 23, 61, 29, 53, 21
            };

            float getBayer2(int x, int y)
            {
                return float(bayer2[(x%2) + (y%2) * 2]) * (1.0f/4.0f) - 0.5f;
            }

            float getBayer4(int x, int y)
            {
                return float(bayer4[(x%4) + (y%4) * 4]) * (1.0f/16.0f) - 0.5f;
            }

            float getBayer8(int x, int y)
            {
                return float(bayer8[(x%8) + (y%8) * 8]) * (1.0f/64.0f) - 0.5f;
            }

            sampler2D _MainTex;
            float4 _MainTex_ST;

            int _Level;
            float _Spread;

            int _RedColorCount;
            int _GreenColorCount;
            int _BlueColorCount;

            float4 frag (Varyings IN) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);

                float2 pixelCoords = IN.texcoord.xy * _BlitTexture_TexelSize.zw;

                float bayerValues[3] = {0, 0, 0};
                bayerValues[0] = getBayer2(pixelCoords.x, pixelCoords.y);
                bayerValues[1] = getBayer4(pixelCoords.x, pixelCoords.y);
                bayerValues[2] = getBayer8(pixelCoords.x, pixelCoords.y);

                float4 color = col + _Spread * bayerValues[_Level];

                color.r = floor(color.r * (_RedColorCount - 1) + 0.5) / (_RedColorCount - 1);
                color.g = floor(color.g * (_GreenColorCount - 1) + 0.5) / (_GreenColorCount - 1);
                color.b = floor(color.b * (_BlueColorCount - 1) + 0.5) / (_BlueColorCount - 1);

                return color;
            }
            ENDHLSL
        }
    }
}
