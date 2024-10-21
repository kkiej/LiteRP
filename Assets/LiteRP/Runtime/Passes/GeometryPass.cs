﻿using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace LiteRP.Runtime
{
    public class GeometryPass
    {
        static readonly ProfilingSampler
            samplerOpaque = new("Opaque Pass"),
            samplerTransparent = new("Transparent Pass");

        static readonly ShaderTagId[] shaderTagIds =
        {
            new("SRPDefaultUnlit"),
            new("LiteRPLit")
        };

        RendererListHandle list;

        void Render(RenderGraphContext context)
        {
            context.cmd.DrawRendererList(list);
            context.renderContext.ExecuteCommandBuffer(context.cmd);
            context.cmd.Clear();
        }

        public static void Record(RenderGraph renderGraph, Camera camera, CullingResults cullingResults,
            int renderingLayerMask, bool opaque, in CameraRendererTextures textures, in LightResources lightData)
        {
            ProfilingSampler sampler = opaque ? samplerOpaque : samplerTransparent;
            
            using RenderGraphBuilder builder = renderGraph.AddRenderPass(sampler.name, out GeometryPass pass, sampler);

            pass.list = builder.UseRendererList(renderGraph.CreateRendererList(
                new RendererListDesc(shaderTagIds, cullingResults, camera)
                {
                    sortingCriteria = opaque ? SortingCriteria.CommonOpaque : SortingCriteria.CommonTransparent,
                    rendererConfiguration = PerObjectData.ReflectionProbes | PerObjectData.Lightmaps |
                                            PerObjectData.ShadowMask | PerObjectData.LightProbe |
                                            PerObjectData.OcclusionProbe | PerObjectData.LightProbeProxyVolume |
                                            PerObjectData.OcclusionProbeProxyVolume,
                    renderQueueRange = opaque ? RenderQueueRange.opaque : RenderQueueRange.transparent,
                    renderingLayerMask = (uint)renderingLayerMask
                }));

            builder.ReadWriteTexture(textures.colorAttachment);
            builder.ReadWriteTexture(textures.depthAttachment);
            if (!opaque)
            {
                if (textures.colorCopy.IsValid())
                {
                    builder.ReadTexture(textures.colorCopy);
                }

                if (textures.depthCopy.IsValid())
                {
                    builder.ReadTexture(textures.depthCopy);
                }
            }

            builder.ReadComputeBuffer(lightData.directionalLightDataBuffer);
            builder.ReadComputeBuffer(lightData.otherLightDataBuffer);
            if (lightData.tilesBuffer.IsValid())
            {
                builder.ReadComputeBuffer(lightData.tilesBuffer);
            }
            builder.ReadTexture(lightData.shadowResources.directionalAtlas);
            builder.ReadTexture(lightData.shadowResources.otherAtlas);
            builder.ReadComputeBuffer(lightData.shadowResources.directionalShadowCascadesBuffer);
            builder.ReadComputeBuffer(lightData.shadowResources.directionalShadowMatricesBuffer);
            builder.ReadComputeBuffer(lightData.shadowResources.otherShadowDataBuffer);
            
            builder.SetRenderFunc<GeometryPass>(static (pass, context) => pass.Render(context));
        }
    }
}