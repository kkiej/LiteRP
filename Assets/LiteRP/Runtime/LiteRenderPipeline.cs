using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Runtime
{
    public partial class LiteRenderPipeline : RenderPipeline
    {
        private CameraRenderer renderer;

        private CameraBufferSettings cameraBufferSettings;

        private bool useDynamicBatching, useGPUInstancing, useSRPBatcher, useLightsPerObject;

        private ShadowSettings shadowSettings;

        private PostFXSettings postFXSettings;

        private int colorLUTResolution;
        
        public LiteRenderPipeline(CameraBufferSettings cameraBufferSettings, bool useDynamicBatching,
            bool useGPUInstancing, bool useSRPBatcher, bool useLightsPerObject, ShadowSettings shadowSettings,
            PostFXSettings postFXSettings, int colorLUTResolution, Shader cameraRendererShader)
        {
            this.colorLUTResolution = colorLUTResolution;
            this.cameraBufferSettings = cameraBufferSettings;
            this.postFXSettings = postFXSettings;
            this.shadowSettings = shadowSettings;
            this.useDynamicBatching = useDynamicBatching;
            this.useGPUInstancing = useGPUInstancing;
            this.useLightsPerObject = useLightsPerObject;
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
            GraphicsSettings.lightsUseLinearIntensity = true;
            InitializeForEditor();
            renderer = new CameraRenderer(cameraRendererShader);
        }
        
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            
        }

        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                renderer.Render(context, cameras[i], cameraBufferSettings, useDynamicBatching, useGPUInstancing, useLightsPerObject,
                    shadowSettings, postFXSettings, colorLUTResolution);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposeForEditor();
            renderer.Dispose();
        }
    }
}