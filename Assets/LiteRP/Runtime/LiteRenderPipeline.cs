using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace LiteRP.Runtime
{
    public partial class LiteRenderPipeline : RenderPipeline
    {
        private CameraRenderer renderer;

        private readonly RenderGraph renderGraph = new("LiteRP Render Graph");

        private bool useSRPBatcher;

        private readonly LiteRenderPipelineSettings settings;
        
        public LiteRenderPipeline(LiteRenderPipelineSettings settings)
        {
            this.settings = settings;
            GraphicsSettings.useScriptableRenderPipelineBatching = settings.useSRPBatcher;
            GraphicsSettings.lightsUseLinearIntensity = true;
            InitializeForEditor();
            renderer = new CameraRenderer(settings.cameraRendererShader, settings.cameraDebuggerShader);
        }
        
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            
        }

        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                renderer.Render(renderGraph, context, cameras[i], settings);
            }
            renderGraph.EndFrame();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposeForEditor();
            renderer.Dispose();
            renderGraph.Cleanup();
        }
    }
}