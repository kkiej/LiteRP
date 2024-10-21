﻿using UnityEngine;

namespace LiteRP.Runtime
{
    [System.Serializable]
    public class LiteRenderPipelineSettings
    {
        public CameraBufferSettings cameraBuffer = new()
        {
            allowHDR = true,
            renderScale = 1f,
            fxaa = new()
            {
                fixedThreshold = 0.0833f,
                relativeThreshold = 0.166f,
                subpixelBlending = 0.75f
            }
        };

        public bool
            useSRPBatcher = true,
            useLightsPerObject = true;

        public ForwardPlusSettings forwardPlus;

        public ShadowSettings shadows;

        public PostFXSettings postFXSettings;

        public enum ColorLUTResolution
        { _16 = 16, _32 = 32, _64 = 64 }

        public ColorLUTResolution colorLUTResolution = ColorLUTResolution._32;

        public Shader cameraRendererShader, cameraDebuggerShader;
    }
}