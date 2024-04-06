﻿using System;
using UnityEngine;

namespace LiteRP.Runtime
{
    [CreateAssetMenu(menuName = "Rendering/Lite Post FX Settings")]
    public class PostFXSettings : ScriptableObject
    {
        [SerializeField]
        private Shader shader = default;

        [NonSerialized]
        private Material material;

        public Material Material
        {
            get
            {
                if (material == null && shader != null)
                {
                    material = new Material(shader);
                    material.hideFlags = HideFlags.HideAndDontSave;
                }

                return material;
            }
        }

        #region Bloom

        [Serializable]
        public struct BloomSettings
        {
            [Range(0f, 16f)]
            public int maxIterations;

            [Min(1f)]
            public int downscaleLimit;

            public bool bicubicUpsampling;

            [Min(0f)]
            public float threshold;

            [Range(0f, 1f)]
            public float thresholdKnee;

            [Min(0f)]
            public float intensity;

            public bool fadeFireFlies;

            public enum Mode
            {
                Additive,
                Scattering
            }

            public Mode mode;

            [Range(0.05f, 0.95f)]
            public float scatter;
        }

        [SerializeField]
        private BloomSettings bloom = new BloomSettings()
        {
            scatter = 0.7f
        };

        public BloomSettings Bloom => bloom;

        #endregion

        #region ColorAdjustments

        [Serializable]
        public struct ColorAdjustmentsSettings
        {
            public float postExposure;

            [Range(-100f, 100f)]
            public float contrast;

            [ColorUsage(false, true)]
            public Color colorFilter;

            [Range(-180f, 180f)]
            public float hueShift;

            [Range(-100f, 100f)]
            public float saturation;
        }

        [SerializeField]
        private ColorAdjustmentsSettings colorAdjustments = new ColorAdjustmentsSettings()
        {
            colorFilter = Color.white
        };

        public ColorAdjustmentsSettings ColorAdjustments => colorAdjustments;

        #endregion

        #region WhiteBalance

        [Serializable]
        public struct WhiteBalanceSettings
        {

            [Range(-100f, 100f)]
            public float temperature, tint;
        }

        [SerializeField]
        WhiteBalanceSettings whiteBalance = default;

        public WhiteBalanceSettings WhiteBalance => whiteBalance;

        #endregion

        #region SplitToning

        [Serializable]
        public struct SplitToningSettings
        {

            [ColorUsage(false)]
            public Color shadows, highlights;

            [Range(-100f, 100f)]
            public float balance;
        }

        [SerializeField]
        SplitToningSettings splitToning = new SplitToningSettings
        {
            shadows = Color.gray,
            highlights = Color.gray
        };

        public SplitToningSettings SplitToning => splitToning;

        #endregion

        #region ChannelMixer

        [Serializable]
        public struct ChannelMixerSettings {

            public Vector3 red, green, blue;
        }
	
        [SerializeField]
        ChannelMixerSettings channelMixer = new ChannelMixerSettings {
            red = Vector3.right,
            green = Vector3.up,
            blue = Vector3.forward
        };

        public ChannelMixerSettings ChannelMixer => channelMixer;

        #endregion

        #region Shadows Midtones Hightlights

        [Serializable]
        public struct ShadowsMidtonesHighlightsSettings
        {

            [ColorUsage(false, true)]
            public Color shadows, midtones, highlights;

            [Range(0f, 2f)]
            public float shadowsStart, shadowsEnd, highlightsStart, highLightsEnd;
        }

        [SerializeField]
        ShadowsMidtonesHighlightsSettings shadowsMidtonesHighlights = new ShadowsMidtonesHighlightsSettings
            {
                shadows = Color.white,
                midtones = Color.white,
                highlights = Color.white,
                shadowsEnd = 0.3f,
                highlightsStart = 0.55f,
                highLightsEnd = 1f
            };

        public ShadowsMidtonesHighlightsSettings ShadowsMidtonesHighlights => shadowsMidtonesHighlights;

        #endregion

        #region ToneMapping

        [Serializable]
        public struct ToneMappingSettings
        {
            public enum Mode
            {
                None,
                ACES,
                Neutral,
                Reinhard
            }

            public Mode mode;
        }

        [SerializeField]
        private ToneMappingSettings toneMapping = default;

        public ToneMappingSettings ToneMapping => toneMapping;

        #endregion
    }
}