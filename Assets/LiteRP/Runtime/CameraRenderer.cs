using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace LiteRP.Runtime
{
    public class CameraRenderer
    {
        private static CameraSettings defaultCameraSettings = new CameraSettings();

        private PostFXStack postFXStack = new PostFXStack();

        private Material material;

        //public const float renderScaleMin = 0.1f, renderScaleMax = 2f;
        
        public CameraRenderer(Shader shader, Shader cameraDebuggerShader)
        {
            material = CoreUtils.CreateEngineMaterial(shader);
            CameraDebugger.Initialize(cameraDebuggerShader);
        }

        public void Dispose()
        {
            CoreUtils.Destroy(material);
            CameraDebugger.Cleanup();
        }

        public void Render(RenderGraph renderGraph, ScriptableRenderContext context, Camera camera,
            LiteRenderPipelineSettings settings)
        {
            CameraBufferSettings bufferSettings = settings.cameraBuffer;
            PostFXSettings postFXSettings = settings.postFXSettings;
            ShadowSettings shadowSettings = settings.shadows;
            
            ProfilingSampler cameraSampler;
            CameraSettings cameraSettings;
            if (camera.TryGetComponent(out LiteRenderPipelineCamera liteRPCamera))
            {
                cameraSampler = liteRPCamera.Sampler;
                cameraSettings = liteRPCamera.Settings;
            }
            else
            {
                cameraSampler = ProfilingSampler.Get(camera.cameraType);
                cameraSettings = defaultCameraSettings;
            }

            bool useColorTexture, useDepthTexture;
            if (camera.cameraType == CameraType.Reflection)
            {
                useColorTexture = bufferSettings.copyColorReflection;
                useDepthTexture = bufferSettings.copyDepthReflection;
            }
            else
            {
                useColorTexture = bufferSettings.copyColor && cameraSettings.copyColor;
                useDepthTexture = bufferSettings.copyDepth && cameraSettings.copyDepth;
            }
            
            if (cameraSettings.overridePostFX)
            {
                postFXSettings = cameraSettings.postFXSettings;
            }

            bool hasActivePostFX = postFXSettings != null && postFXSettings.AreApplicableTo(camera);

            float renderScale = cameraSettings.GetRenderScale(bufferSettings.renderScale);
            bool useScaledRendering = renderScale < 0.99f || renderScale > 1.01f;
            
#if UNITY_EDITOR
            if (camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
                useScaledRendering = false;
            }
#endif

            if (!camera.TryGetCullingParameters(out ScriptableCullingParameters scriptableCullingParameters))
            {
                return;
            }
            scriptableCullingParameters.shadowDistance = Mathf.Min(shadowSettings.maxDistance, camera.farClipPlane);
            CullingResults cullingResults = context.Cull(ref scriptableCullingParameters);

            //bool useHDR = bufferSettings.allowHDR && camera.allowHDR;
            bufferSettings.allowHDR &= camera.allowHDR;
            Vector2Int bufferSize = default;
            if (useScaledRendering)
            {
                renderScale = Mathf.Clamp(renderScale, 0.1f, 2f);
                bufferSize.x = (int)(camera.pixelWidth * renderScale);
                bufferSize.y = (int)(camera.pixelHeight * renderScale);
            }
            else
            {
                bufferSize.x = camera.pixelWidth;
                bufferSize.y = camera.pixelHeight;
            }
            
            //bufferSettings.fxaa.enabled &= cameraSettings.allowFXAA;
            bufferSettings.fxaa.enabled = cameraSettings.allowFXAA;

            bool useIntermediateBuffer = true;

            var renderGraphParameters = new RenderGraphParameters()
            {
                commandBuffer = CommandBufferPool.Get(),
                currentFrameIndex = Time.frameCount,
                executionName = cameraSampler.name,
                rendererListCulling = true,
                scriptableRenderContext = context
            };

            using (renderGraph.RecordAndExecute(renderGraphParameters))
            {
                using var _ = new RenderGraphProfilingScope(renderGraph, cameraSampler);
                LightResources lightResources = LightingPass.Record(renderGraph, cullingResults, bufferSize,
                    settings.forwardPlus, shadowSettings, cameraSettings.maskLights ?
                        cameraSettings.renderingLayerMask : -1);
                CameraRendererTextures textures = SetupPass.Record(renderGraph, useIntermediateBuffer, useColorTexture,
                    useDepthTexture, bufferSettings.allowHDR, bufferSize, camera);

                GeometryPass.Record(renderGraph, camera, cullingResults, cameraSettings.renderingLayerMask, true,
                    textures, lightResources);
                
                SkyboxPass.Record(renderGraph, camera, textures);

                var copier = new CameraRendererCopier(material, camera, cameraSettings.finalBlendMode);
                CopyAttachmentsPass.Record(renderGraph, useColorTexture, useDepthTexture, copier, textures);

                GeometryPass.Record(renderGraph, camera, cullingResults, cameraSettings.renderingLayerMask, false,
                    textures, lightResources);
                
                UnsupportedShadersPass.Record(renderGraph, camera, cullingResults);
                
                if (hasActivePostFX)
                {
                    postFXStack.BufferSettings = bufferSettings;
                    postFXStack.BufferSize = bufferSize;
                    postFXStack.Camera = camera;
                    postFXStack.FinalBlendMode = cameraSettings.finalBlendMode;
                    postFXStack.Settings = postFXSettings;
                    PostFXPass.Record(renderGraph, postFXStack, (int)settings.colorLUTResolution, cameraSettings.keepAlpha, textures);
                }
                else if (useIntermediateBuffer)
                {
                    FinalPass.Record(renderGraph, copier, textures);
                }
                DebugPass.Record(renderGraph, settings, camera, lightResources);
                GizmosPass.Record(renderGraph, useIntermediateBuffer, copier, textures);
            }
            
            context.ExecuteCommandBuffer(renderGraphParameters.commandBuffer);
            context.Submit();
            CommandBufferPool.Release(renderGraphParameters.commandBuffer);
        }
    }
}