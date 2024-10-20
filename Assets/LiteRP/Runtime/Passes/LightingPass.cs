using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace LiteRP.Runtime
{
    public partial class LightingPass
    {
        private static readonly ProfilingSampler sampler = new("Light And Shadow Pass");
        
        private const int MaxDirLightCount = 4, maxOtherLightCount = 64;

        private static readonly int
            //方向光的属性名
            DirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
            DirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
            dirLightDirectionsAndMasksID = Shader.PropertyToID("_DirectionalLightDirectionsAndMasks"),
            DirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData"),
            //其它光的属性名
            otherLightCountId = Shader.PropertyToID("_OtherLightCount"),
            otherLightColorsId = Shader.PropertyToID("_OtherLightColors"),
            otherLightPositionsId = Shader.PropertyToID("_OtherLightPositions"),
            otherLightDirectionsAndMasksId = Shader.PropertyToID("_OtherLightDirectionsAndMasks"),
            otherLightSpotAnglesId = Shader.PropertyToID("_OtherLightSpotAngles"),
            otherLightShadowDataId = Shader.PropertyToID("_OtherLightShadowData");

        private static readonly Vector4[]
            //方向光属性数组
            dirLightColors = new Vector4[MaxDirLightCount],
            dirLightDirectionsAndMasks = new Vector4[MaxDirLightCount],
            dirLightShadowData = new Vector4[MaxDirLightCount];

        private static readonly OtherLightData[] otherLightData = new OtherLightData[maxOtherLightCount];

        private CullingResults cullingResults;

        private readonly Shadows shadows = new Shadows();

        private static readonly GlobalKeyword LightsPerObjectKeyword = GlobalKeyword.Create("_LIGHTS_PER_OBJECT");

        private int dirLightCount, otherLightCount;

        private bool useLightsPerObject;
        
        public void Setup(CullingResults cullingResults, ShadowSettings shadowSettings, bool useLightsPerObject,
            int renderingLayerMask)
        {
            this.cullingResults = cullingResults;
            this.useLightsPerObject = useLightsPerObject;
            shadows.Setup(cullingResults, shadowSettings);
            SetupLights(renderingLayerMask);
        }

        private void SetupLights(int renderingLayerMask)
        {
            //从剔除结果检索灯光索引图
            NativeArray<int> indexMap = useLightsPerObject ?
                cullingResults.GetLightIndexMap(Allocator.Temp) : default;
            //从剔除结果获取可见光
            NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;

            dirLightCount = otherLightCount = 0;
            int i;
            for (i = 0; i < visibleLights.Length; i++)
            {
                int newIndex = -1;
                VisibleLight visibleLight = visibleLights[i];
                Light light = visibleLight.light;
                if ((light.renderingLayerMask & renderingLayerMask) != 0)
                {
                    switch (visibleLight.lightType)
                    {
                        case LightType.Directional:
                            if (dirLightCount < MaxDirLightCount)
                            {
                                SetupDirectionalLight(dirLightCount++, i, ref visibleLight, light);
                            }
                            break;
                        case LightType.Point:
                            if (otherLightCount < maxOtherLightCount)
                            {
                                newIndex = otherLightCount;
                                otherLightData[otherLightCount++] = OtherLightData.CreatePointLight(ref visibleLight,
                                    light, shadows.ReserveOtherShadows(light, i));
                            }
                            break;
                        case LightType.Spot:
                            if (otherLightCount < maxOtherLightCount)
                            {
                                newIndex = otherLightCount;
                                SetupSpotLight(otherLightCount++, i, ref visibleLight, light);
                            }
                            break;
                    }
                }

                if (useLightsPerObject)
                {
                    indexMap[i] = newIndex;
                }
            }
            
            if (useLightsPerObject)
            {
                //归零其他不可见光源
                for (; i < indexMap.Length; i++)
                {
                    indexMap[i] = -1;
                }
                //将重映射后的每物体光源索引告诉Unity
                cullingResults.SetLightIndexMap(indexMap);
                //释放indexMap内存
                indexMap.Dispose();
            }
        }

        private void Render(RenderGraphContext context)
        {
            CommandBuffer buffer = context.cmd;
            buffer.SetKeyword(LightsPerObjectKeyword, useLightsPerObject);
            buffer.SetGlobalInt(DirLightCountId, dirLightCount);
            if (dirLightCount > 0)
            {
                buffer.SetGlobalVectorArray(DirLightColorsId, dirLightColors);
                buffer.SetGlobalVectorArray(dirLightDirectionsAndMasksID, dirLightDirectionsAndMasks);
                buffer.SetGlobalVectorArray(DirLightShadowDataId, dirLightShadowData);
            }
            
            buffer.SetGlobalInt(otherLightCountId, otherLightCount);
            if (otherLightCount > 0)
            {
                buffer.SetGlobalVectorArray(otherLightColorsId, otherLightColors);
                buffer.SetGlobalVectorArray(otherLightPositionsId, otherLightPositions);
                buffer.SetGlobalVectorArray(otherLightDirectionsAndMasksId, otherLightDirectionsAndMasks);
                buffer.SetGlobalVectorArray(otherLightSpotAnglesId, otherLightSpotAngles);
                buffer.SetGlobalVectorArray(otherLightShadowDataId, otherLightShadowData);
            }
            
            shadows.Render(context);
            context.renderContext.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        private void SetupDirectionalLight(int index, int visibleIndex, ref VisibleLight visibleLight, Light light)
        {
            dirLightColors[index] = visibleLight.finalColor;
            Vector4 dirAndMask = -visibleLight.localToWorldMatrix.GetColumn(2);
            dirAndMask.w = light.renderingLayerMask.ReinterpretAsFloat();
            dirLightDirectionsAndMasks[index] = dirAndMask;
            dirLightShadowData[index] = shadows.ReserveDirectionalShadows(light, visibleIndex);
        }

        private void SetupSpotLight(int index, int visibleIndex, ref VisibleLight visibleLight, Light light)
        {
            otherLightColors[index] = visibleLight.finalColor;
            Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
            position.w = 1f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f);
            otherLightPositions[index] = position;
            Vector4 dirAndMask = -visibleLight.localToWorldMatrix.GetColumn(2);
            dirAndMask.w = light.renderingLayerMask.ReinterpretAsFloat();
            otherLightDirectionsAndMasks[index] = dirAndMask;
            
            float innerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.innerSpotAngle);
            float outerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * visibleLight.spotAngle);
            float angleRangeInv = 1f / Mathf.Max(innerCos - outerCos, 0.001f);
            otherLightSpotAngles[index] = new Vector4(angleRangeInv, -outerCos * angleRangeInv);
            otherLightShadowData[index] = shadows.ReserveOtherShadows(light, visibleIndex);
        }
        
        private void SetupPointLight(int index, int visibleIndex, ref VisibleLight visibleLight, Light light)
        {
            otherLightColors[index] = visibleLight.finalColor;
            Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
            position.w = 1f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f);
            otherLightPositions[index] = position;

            otherLightSpotAngles[index] = new Vector4(0f, 1f);

            Vector4 dirAndMask = Vector4.zero;
            dirAndMask.w = light.renderingLayerMask.ReinterpretAsFloat();
            otherLightDirectionsAndMasks[index] = dirAndMask;
            otherLightShadowData[index] = shadows.ReserveOtherShadows(light, visibleIndex);
        }
        
        public static ShadowTextures Record(RenderGraph renderGraph, CullingResults cullingResults,
            ShadowSettings shadowSettings, bool useLightsPerObject, int renderingLayerMask)
        {
            using RenderGraphBuilder builder = renderGraph.AddRenderPass(sampler.name, out LightingPass pass, sampler);
            pass.Setup(cullingResults, shadowSettings, useLightsPerObject, renderingLayerMask);
            builder.SetRenderFunc<LightingPass>(static (pass, context) => pass.Render(context));
            builder.AllowPassCulling(false);
            return pass.shadows.GetRenderTextures(renderGraph, builder);
        }
    }
    
    public static class ReinterpretExtensions
    {
        [StructLayout(LayoutKind.Explicit)]
        struct IntFloat
        {
            [FieldOffset(0)]
            public int intValue;

            [FieldOffset(0)]
            public float floatValue;
        }
        
        public static float ReinterpretAsFloat(this int value)
        {
            return value;
        }
    }
}