using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Runtime
{
    [CreateAssetMenu(menuName = "Rendering/Lite Render Pipeline")]
    public class LiteRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField]
        private bool allowHDR = true;
        
        [SerializeField]
        private bool
            useDynamicBatching = true,
            useGPUInstancing = true,
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
        
        protected override RenderPipeline CreatePipeline()
        {
            return new LiteRenderPipeline(allowHDR, useDynamicBatching, useGPUInstancing,
                useSRPBatcher, useLightsPerObject, shadows, postFXSettings, (int)colorLUTResolution);
        }
    }
}