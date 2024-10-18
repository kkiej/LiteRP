using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Runtime
{
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public class LiteRenderPipelineCamera : MonoBehaviour
    {
        [SerializeField]
        private CameraSettings settings = default;

        private ProfilingSampler sampler;

        public ProfilingSampler Sampler => sampler ??= new(GetComponent<Camera>().name);
        
        public CameraSettings Settings => settings ??= new CameraSettings();
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void OnEnable() => sampler = null;
#endif
    }
}