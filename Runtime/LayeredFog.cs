using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace Voodoocado
{
    [Serializable]
    [PostProcess(typeof(LayeredFogRenderer), PostProcessEvent.BeforeStack, "Voodoocado/LayeredFog")]
    public sealed class LayeredFog : PostProcessEffectSettings
    {
        public enum DebugMode
        {
            None,
            Depth,
            Depth01,
            Normals,
            WorldPos,
            FogDensity,
            CameraDir,
        };

        public enum LayeredFogMode
        {
            Linear = 1,
            Exponential  = 2,
            Diminishing = 3
        };

        [Serializable]
        public sealed class LayeredFogModeParameter : ParameterOverride<LayeredFogMode> { }
        public LayeredFogModeParameter layeredFogMode = new LayeredFogModeParameter { value = LayeredFogMode.Linear };
        [Range(0f, 1f)]
        public UnityEngine.Rendering.PostProcessing.FloatParameter fogOpacity = new UnityEngine.Rendering.PostProcessing.FloatParameter { value = 0.5f };
        public UnityEngine.Rendering.PostProcessing.FloatParameter fogDistance = new UnityEngine.Rendering.PostProcessing.FloatParameter { value = 100f };
        public UnityEngine.Rendering.PostProcessing.ColorParameter fogColor = new UnityEngine.Rendering.PostProcessing.ColorParameter { value = Color.grey };

        public UnityEngine.Rendering.PostProcessing.FloatParameter fogHeightMin = new UnityEngine.Rendering.PostProcessing.FloatParameter { value = 10f };
        public UnityEngine.Rendering.PostProcessing.FloatParameter fogHeightMax = new UnityEngine.Rendering.PostProcessing.FloatParameter { value = 20f };
        [Range(0f, 1f)]
        public UnityEngine.Rendering.PostProcessing.FloatParameter fogDensityBelowMin = new UnityEngine.Rendering.PostProcessing.FloatParameter { value = 1f };
        [Range(0f, 1f)]
        public UnityEngine.Rendering.PostProcessing.FloatParameter fogDensityAboveMax = new UnityEngine.Rendering.PostProcessing.FloatParameter { value = 0f };

        [Serializable]
        public sealed class DebugModeParameter : ParameterOverride<DebugMode> { }
        public DebugModeParameter debugMode = new DebugModeParameter { value = DebugMode.None };
    }

    public sealed class LayeredFogRenderer : PostProcessEffectRenderer<LayeredFog>
    {
        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.DepthNormals;
        }
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(Shader.Find("Voodoocado/LayeredFog"));

            Texture depth = Shader.GetGlobalTexture("_CameraDepthTexture");
            if (depth != null)
                sheet.properties.SetTexture("_DepthTexture", depth);

            Camera camera = Camera.main;
            sheet.properties.SetMatrix("_CamToWorld", camera.cameraToWorldMatrix);
            sheet.properties.SetMatrix("_ViewProjectInverse", (camera.projectionMatrix * camera.worldToCameraMatrix).inverse);

            sheet.properties.SetFloat("_FogHeightMin", settings.fogHeightMin);
            sheet.properties.SetFloat("_FogHeightMax", settings.fogHeightMax);
            sheet.properties.SetFloat("_FogDensityBelowMin", settings.fogDensityBelowMin);
            sheet.properties.SetFloat("_FogDensityAboveMax", settings.fogDensityAboveMax);

            sheet.properties.SetFloat("_FogMode", (int)settings.layeredFogMode.value);
            sheet.properties.SetFloat("_FogOpacity", settings.fogOpacity);
            sheet.properties.SetFloat("_FogDistance", settings.fogDistance);
            sheet.properties.SetColor("_FogColor", settings.fogColor);

            sheet.properties.SetFloat("_DebugMode", (int)settings.debugMode.value);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}