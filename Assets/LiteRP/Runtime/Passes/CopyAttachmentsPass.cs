﻿using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace LiteRP.Runtime
{
    public class CopyAttachmentsPass
    {
        static readonly ProfilingSampler sampler = new("Copy Pass");

        private bool copyColor, copyDepth;

        private CameraRendererCopier copier;

        private TextureHandle colorAttachment, depthAttachment, colorCopy, depthCopy;
        
        private static readonly int
            colorCopyID = Shader.PropertyToID("_CameraColorTexture"),
            depthCopyID = Shader.PropertyToID("_CameraDepthTexture");
        
        void Render(RenderGraphContext context)
        {
            CommandBuffer buffer = context.cmd;
            if (copyColor)
            {
                copier.Copy(buffer, colorAttachment, colorCopy, false);
                buffer.SetGlobalTexture(colorCopyID, colorCopy);
            }
            if (copyDepth)
            {
                copier.Copy(buffer, depthAttachment, depthCopy, true);
                buffer.SetGlobalTexture(depthCopyID, depthCopy);
            }
            if (CameraRendererCopier.RequiresRenderTargetResetAfterCopy)
            {
                buffer.SetRenderTarget(
                    colorAttachment,
                    RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                    depthAttachment,
                    RenderBufferLoadAction.Load, RenderBufferStoreAction.Store
                );
            }
            context.renderContext.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }
        
        public static void Record(RenderGraph renderGraph, bool copyColor, bool copyDepth, CameraRendererCopier copier,
            in CameraRendererTextures textures)
        {
            using RenderGraphBuilder builder = renderGraph.AddRenderPass(sampler.name, out CopyAttachmentsPass pass, sampler);
            pass.copyColor = copyColor;
            pass.copyDepth = copyDepth;
            pass.copier = copier;

            pass.colorAttachment = builder.ReadTexture(textures.colorAttachment);
            pass.depthAttachment = builder.ReadTexture(textures.depthAttachment);
            if (copyColor)
            {
                pass.colorCopy = builder.WriteTexture(textures.colorCopy);
            }

            if (copyDepth)
            {
                pass.depthCopy = builder.WriteTexture(textures.depthCopy);
            }
            builder.SetRenderFunc<CopyAttachmentsPass>(static (pass, context) => pass.Render(context));
        }
    }
}