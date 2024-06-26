﻿using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Runtime
{
    public partial class CameraRenderer
    {
        private ScriptableRenderContext context;

        private Camera camera;

        private const string BufferName = "Render Camera";

        private readonly CommandBuffer buffer = new CommandBuffer()
        {
            name = BufferName
        };

        private CullingResults cullingResults;

        private static readonly ShaderTagId
            UnlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
            LitShaderTagId = new ShaderTagId("CustomLit");

        private static int frameBufferId = Shader.PropertyToID("_CameraFrameBuffer");

        private Lighting lighting = new Lighting();

        private PostFXStack postFXStack = new PostFXStack();

        private bool useHDR;
        
        public void Render(ScriptableRenderContext context, Camera camera, bool allowHDR, bool useDynamicBatching,
            bool useGPUInstancing, bool useLightsPerObject, ShadowSettings shadowSettings, PostFXSettings postFXSettings,
            int colorLUTResolution)
        {
            this.context = context;
            this.camera = camera;
            
            PrepareBuffer();
            PrepareForSceneWindow();

            if (!Cull(shadowSettings.maxDistance))
            {
                return;
            }

            useHDR = allowHDR && camera.allowHDR;
            
            buffer.BeginSample(SampleName);
            ExecuteBuffer();
            lighting.Setup(context, cullingResults, shadowSettings, useLightsPerObject);
            postFXStack.Setup(context, camera, postFXSettings, useHDR, colorLUTResolution);
            buffer.EndSample(SampleName);
            Setup();
            DrawVisibleGeometry(useDynamicBatching, useGPUInstancing, useLightsPerObject);
            DrawUnsupportedShaders();
            DrawGizmosBeforeFX();
            if (postFXStack.IsActive)
            {
                postFXStack.Render(frameBufferId);
            }
            DrawGizmosAfterFX();
            Cleanup();
            Submit();
        }

        bool Cull (float maxShadowDistance)
        {
            if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
                cullingResults = context.Cull(ref p);
                return true;
            }
            return false;
        }

        private void Setup()
        {
            context.SetupCameraProperties(camera);
            CameraClearFlags flags = camera.clearFlags;

            if (postFXStack.IsActive)
            {
                if (flags > CameraClearFlags.Color)
                {
                    flags = CameraClearFlags.Color;
                }
                buffer.GetTemporaryRT(frameBufferId, camera.pixelWidth, camera.pixelHeight, 32,
                    FilterMode.Bilinear, useHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
                buffer.SetRenderTarget(frameBufferId,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            }
            
            buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color,
                flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
            buffer.BeginSample(SampleName);
            ExecuteBuffer();
        }

        private void Submit()
        {
            buffer.EndSample(SampleName);
            ExecuteBuffer();
            context.Submit();
        }
        
        private void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing, bool useLightsPerObject)
        {
            PerObjectData lightsPerObjectFlags = useLightsPerObject
                ? PerObjectData.LightData | PerObjectData.LightIndices
                : PerObjectData.None;
            var sortingSettings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(UnlitShaderTagId, sortingSettings)
            {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing,
                perObjectData = PerObjectData.ReflectionProbes | PerObjectData.Lightmaps | PerObjectData.ShadowMask |
                                PerObjectData.LightProbe | PerObjectData.OcclusionProbe |
                                PerObjectData.LightProbeProxyVolume | PerObjectData.OcclusionProbeProxyVolume |
                                lightsPerObjectFlags
            };
            drawingSettings.SetShaderPassName(1, LitShaderTagId);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            context.DrawSkybox(camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        }

        void Cleanup()
        {
            lighting.Cleanup();
            if (postFXStack.IsActive)
            {
                buffer.ReleaseTemporaryRT(frameBufferId);
            }
        }
    }
}