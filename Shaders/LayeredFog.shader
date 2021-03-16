Shader "Voodoocado/LayeredFog"
{
    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
    TEXTURE2D_SAMPLER2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture);

    float4 _FogColor;
    float _FogMode;
    float _FogOpacity;
    float _FogDistance;
    float _DebugMode;
    float _FogHeightMin;
    float _FogHeightMax;
    float _FogDensityBelowMin;
    float _FogDensityAboveMax;
    float4x4 _CamToWorld;
    float4x4 _ViewProjectInverse;

    float _LightIntensity;
    float3 _LightDir;
    float3 _LightColor;

    float _AmbientIntensity;
    float3 _AmbientColor;

    float _SpecularIntensity;
    float _SpecularPower;

    // inverse of y = x²(3-2x)
    float inverse_smoothstep(float x)
    {
        return 0.5 - sin(asin(1.0 - 2.0 * x) / 3.0);
    }

    struct VertToFrag
    {
        float4 vertex    : SV_Position;
        float2 texcoord  : TEXCOORD0;
        float4 cameraDir : TEXCOORD1;
    };

    VertToFrag Vert(AttributesDefault v)
    {
        VertToFrag o;
        o.vertex = float4(v.vertex.xy, 0.0, 1.0);
        o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);
        o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);

        // Figure out the camera direction
        o.cameraDir = mul(_ViewProjectInverse, float4(o.texcoord.xy * 2.0 - 1.0, 1, 1.0));

        return o;
    }

    float4 Frag(VertToFrag i) : SV_Target
    {
        // Input
        float4 originalColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
        float4 depthTexture = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord);
        float4 depthNormals = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.texcoord);

        i.cameraDir = normalize(i.cameraDir);

        // Extract the normal and depth from the combined normal/depth texture 
        half3 normal = mul((float3x3)_CamToWorld, DecodeViewNormalStereo(depthNormals));

        // Calculate the world position from the depth and the camera transform
        float depth = min(1000, LinearEyeDepth(depthTexture.x));
        float depth01 = min(1000.0, Linear01Depth(depthTexture.x));

        //float3 worldPosition = depth * normalize(i.cameraDir) + _WorldSpaceCameraPos;
        float3 worldPosition = depth01 * (i.cameraDir.xyz / i.cameraDir.w) + _WorldSpaceCameraPos;

        // Start calculating the different shader steps.
        float fogDensity = 0.0;

        // Calculate how far and how dense fog we have traveled through the fog.
        float x1 = min(_WorldSpaceCameraPos.y, worldPosition.y);
        float x2 = max(_WorldSpaceCameraPos.y, worldPosition.y);
        float m1 = _FogHeightMin;
        float m2 = _FogHeightMax;
        float f1 = saturate((x1 - m1) / (m2 - m1));
        float f2 = saturate((x2 - m1) / (m2 - m1));
        float fogTravelled =
            depth *
            lerp(_FogDensityBelowMin, 
                _FogDensityAboveMax,
                    (saturate((x2 - m2) / (x2 - x1)) +
                    0.5 * (m2 - m1) * (f2 * f2 - f1 * f1) / (x2 - x1)));

        switch (_FogMode)
        {
            // Linear
            case 1:
                fogDensity = saturate(fogTravelled / _FogDistance);
                break;

            // Exponential. Which is weird.
            case 2:
                fogDensity = saturate(1 - 1 / (exp(fogTravelled / _FogDistance)));
                break;

            // Diminishing
            case 3:
                // Kind of the opposite to exponential actually
                fogDensity = smoothstep(0, 1, saturate(1.0 - 1.0 / (3 * fogTravelled / _FogDistance + 1)));
                break;
        }

        // Potentially show the input
        switch (_DebugMode)
        {
        case 1:
            depth /= 1000;
            return float4(depth, depth, depth, 1.0);
        case 2:
            depth01;
            return float4(depth01, depth01, depth01, 1.0);
        case 3:
            return float4(normal, 1.0);
        case 4:
            worldPosition = (worldPosition / 10) % 1;
            return float4(worldPosition, 1.0);
        case 5:
            return float4(fogDensity, fogDensity, fogDensity, 1.0);
        case 6:
            return float4(i.cameraDir.xyz, 1.0);
        }

        // Apply the Fog
        float finalFogAmount = saturate(_FogOpacity * fogDensity * _FogColor.a);
        float3 finalColor =
            lerp(originalColor.rgb,
                _FogColor.rgb,
                finalFogAmount);

        return float4(finalColor, 1.0);
    }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag

            ENDHLSL
        }
    }
}