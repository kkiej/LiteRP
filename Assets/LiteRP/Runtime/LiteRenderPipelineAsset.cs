using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Runtime
{
    [CreateAssetMenu(menuName = "Rendering/Lite Render Pipeline")]
    public partial class LiteRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField]
        private CameraBufferSettings cameraBuffer = new CameraBufferSettings
        {
            allowHDR = true,
            renderScale = 1f,
            fxaa = new CameraBufferSettings.FXAA
            {
                fixedThreshold = 0.0833f,
                relativeThreshold = 0.166f,
                subpixelBlending = 0.75f
            }
        };
        
        [SerializeField]
        private bool
            useSRPBatcher = true,
            useLightsPerObject = true;

        [SerializeField]
        private ShadowSettings shadows = default;

        [SerializeField]
        private PostFXSettings postFXSettings = default;

        public enum ColorLUTResolution
        {
            _16 = 16,
            _32 = 32,
            _64 = 64
        }

        [SerializeField]
        private ColorLUTResolution colorLUTResolution = ColorLUTResolution._32;

        [SerializeField]
        private Shader cameraRendererShader = default;

        [Header("Deprecated Settings")]
        [SerializeField, Tooltip("Dynamic batching is no longer used.")]
        private bool useDynamicBatching;

        [SerializeField, Tooltip("GPU instancing is always enabled.")]
        private bool useGPUInstancing;
        
        protected override RenderPipeline CreatePipeline()
        {
            return new LiteRenderPipeline(cameraBuffer, useSRPBatcher, useLightsPerObject, shadows, postFXSettings,
                (int)colorLUTResolution, cameraRendererShader);
        }
    }
}