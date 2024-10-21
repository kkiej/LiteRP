using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace LiteRP.Runtime
{
    public readonly ref struct LightResources
    {
        public readonly ComputeBufferHandle
            directionalLightDataBuffer, otherLightDataBuffer, tilesBuffer;
	
        public readonly ShadowResources shadowResources;

        public LightResources(ComputeBufferHandle directionalLightDataBuffer, ComputeBufferHandle otherLightDataBuffer,
            ComputeBufferHandle tilesBuffer, ShadowResources shadowResources)
        {
            this.directionalLightDataBuffer = directionalLightDataBuffer;
            this.otherLightDataBuffer = otherLightDataBuffer;
            this.tilesBuffer = tilesBuffer;
            this.shadowResources = shadowResources;
        }
    }
}