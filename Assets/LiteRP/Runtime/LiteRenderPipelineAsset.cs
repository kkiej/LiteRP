using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Runtime
{
    [CreateAssetMenu(menuName = "Rendering/Lite Render Pipeline")]
    public partial class LiteRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField]
        private LiteRenderPipelineSettings settings;

        [SerializeField, Tooltip("Moved to settings."), HideInInspector]
        CameraBufferSettings cameraBuffer = new()
        {
            allowHDR = true,
            renderScale = 1f,
            fxaa = new()
            {
                fixedThreshold = 0.0833f,
                relativeThreshold = 0.166f,
                subpixelBlending = 0.75f
            }
        };

        [SerializeField, Tooltip("Moved to settings."), HideInInspector]
        bool
            useSRPBatcher = true,
            useLightsPerObject = true;

        [SerializeField, Tooltip("Moved to settings."), HideInInspector]
        ShadowSettings shadows = default;

        [SerializeField, Tooltip("Moved to settings."), HideInInspector]
        PostFXSettings postFXSettings = default;

        public enum ColorLUTResolution
        { _16 = 16, _32 = 32, _64 = 64 }

        [SerializeField, Tooltip("Moved to settings."), HideInInspector]
        ColorLUTResolution colorLUTResolution = ColorLUTResolution._32;

        [SerializeField, Tooltip("Moved to settings."), HideInInspector]
        Shader cameraRendererShader = default;
        
        protected override RenderPipeline CreatePipeline()
        {
            if ((settings == null || settings.cameraRendererShader == null) && cameraRendererShader != null)
            {
                settings = new LiteRenderPipelineSettings()
                {
                    cameraBuffer = cameraBuffer,
                    useSRPBatcher = useSRPBatcher,
                    useLightsPerObject = useLightsPerObject,
                    shadows = shadows,
                    postFXSettings = postFXSettings,
                    colorLUTResolution =
                        (LiteRenderPipelineSettings.ColorLUTResolution)
                        colorLUTResolution,
                    cameraRendererShader = cameraRendererShader
                };
            }
            
            if (postFXSettings != null)
            {
                postFXSettings = null;
            }
            if (cameraRendererShader != null)
            {
                cameraRendererShader = null;
            }
            
            return new LiteRenderPipeline(settings);
        }
    }
}