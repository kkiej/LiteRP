using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Runtime
{
    public partial class LiteRenderPipeline : RenderPipeline
    {
        private CameraRenderer renderer = new CameraRenderer();

        private bool allowHDR;

        private bool useDynamicBatching, useGPUInstancing, useSRPBatcher, useLightsPerObject;

        private ShadowSettings shadowSettings;

        private PostFXSettings postFXSettings;

        private int colorLUTResolution;
        
        public LiteRenderPipeline(bool allowHDR, bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher,
            bool useLightsPerObject, ShadowSettings shadowSettings, PostFXSettings postFXSettings, int colorLUTResolution)
        {
            this.colorLUTResolution = colorLUTResolution;
            this.allowHDR = allowHDR;
            this.postFXSettings = postFXSettings;
            this.shadowSettings = shadowSettings;
            this.useDynamicBatching = useDynamicBatching;
            this.useGPUInstancing = useGPUInstancing;
            this.useLightsPerObject = useLightsPerObject;
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
            GraphicsSettings.lightsUseLinearIntensity = true;
            InitializeForEditor();
        }
        
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            
        }

        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                renderer.Render(context, cameras[i], allowHDR, useDynamicBatching, useGPUInstancing, useLightsPerObject,
                    shadowSettings, postFXSettings, colorLUTResolution);
            }
        }
    }
}