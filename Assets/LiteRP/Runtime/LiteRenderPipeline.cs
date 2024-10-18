﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace LiteRP.Runtime
{
    public partial class LiteRenderPipeline : RenderPipeline
    {
        private CameraRenderer renderer;

        private CameraBufferSettings cameraBufferSettings;

        private readonly RenderGraph renderGraph = new("LiteRP Render Graph");

        private bool useSRPBatcher, useLightsPerObject;

        private ShadowSettings shadowSettings;

        private PostFXSettings postFXSettings;

        private int colorLUTResolution;
        
        public LiteRenderPipeline(CameraBufferSettings cameraBufferSettings, bool useSRPBatcher,
            bool useLightsPerObject, ShadowSettings shadowSettings, PostFXSettings postFXSettings,
            int colorLUTResolution, Shader cameraRendererShader)
        {
            this.colorLUTResolution = colorLUTResolution;
            this.cameraBufferSettings = cameraBufferSettings;
            this.postFXSettings = postFXSettings;
            this.shadowSettings = shadowSettings;
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
                renderer.Render(renderGraph, context, cameras[i], cameraBufferSettings, useLightsPerObject,
                    shadowSettings, postFXSettings, colorLUTResolution);
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