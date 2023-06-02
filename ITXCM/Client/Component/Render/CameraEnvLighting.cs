using UnityEngine; // 版本 2021.3.15f1c1  后续版本 RenderSettings.customReflection 变化了
using UnityEngine.Rendering;

namespace ITXCM
{
    /// <summary>
    /// 相机环境光处理
    /// </summary>
    [AddComponentMenu("ITXCM/Render/Camera Environment Lighting")]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraEnvLighting : MonoBehaviour
    {
        [SerializeField]
        private DefaultReflectionMode reflectionMode = DefaultReflectionMode.Custom;
        [SerializeField]
        private AmbientMode ambientMode = AmbientMode.Trilight;
        [SerializeField]
        private float ambientIntensity;
        [SerializeField]
        private Color ambientLight;
        [SerializeField]
        private Color ambientSkyColor;
        [SerializeField]
        private Color ambientEquatorColor;
        [SerializeField]
        private Color ambientGroundColor;
        [SerializeField]
        private Cubemap customReflection;
        [SerializeField]
        private float reflectionIntensity = 1f;
        [SerializeField]
        private bool fog;
        private DefaultReflectionMode savedReflectionMode;
        private AmbientMode savedAmbientMode;
        private float savedAmbientIntensity;
        private Color savedAmbientLight;
        private Color savedAmbientSkyColor;
        private Color savedAmbientEquatorColor;
        private Color savedAmbientGroundColor;
        private Texture savedCustomReflection;
        private float savedReflectionIntensity;
        private bool savedFog;

        public AmbientMode AmbientMode
        {
            get => this.ambientMode;
            set => this.ambientMode = value;
        }

        public float AmbientIntensity
        {
            get => this.ambientIntensity;
            set => this.ambientIntensity = value;
        }

        public Color AmbientLight
        {
            get => this.ambientLight;
            set => this.ambientLight = value;
        }

        public Color AmbientSkyColor
        {
            get => this.ambientSkyColor;
            set => this.ambientSkyColor = value;
        }

        public Color AmbientEquatorColor
        {
            get => this.ambientEquatorColor;
            set => this.ambientEquatorColor = value;
        }

        public Color AmbientGroundColor
        {
            get => this.ambientGroundColor;
            set => this.ambientGroundColor = value;
        }

        public Cubemap CustomReflection
        {
            get => this.customReflection;
            set => this.customReflection = value;
        }

        public float ReflectionIntensity
        {
            get => this.reflectionIntensity;
            set => this.reflectionIntensity = value;
        }

        public bool Fog
        {
            get => this.fog;
            set => this.fog = value;
        }

        private void OnPreRender()
        {
            this.savedReflectionMode = RenderSettings.defaultReflectionMode;
            this.savedAmbientMode = RenderSettings.ambientMode;
            this.savedAmbientIntensity = RenderSettings.ambientIntensity;
            this.savedAmbientLight = RenderSettings.ambientLight;
            this.savedAmbientSkyColor = RenderSettings.ambientSkyColor;
            this.savedAmbientEquatorColor = RenderSettings.ambientEquatorColor;
            this.savedAmbientGroundColor = RenderSettings.ambientGroundColor;
            this.savedCustomReflection = RenderSettings.customReflection;
            this.savedReflectionIntensity = RenderSettings.reflectionIntensity;
            this.savedFog = RenderSettings.fog;

            RenderSettings.defaultReflectionMode = this.reflectionMode;
            RenderSettings.ambientMode = this.ambientMode;
            RenderSettings.ambientIntensity = this.ambientIntensity;
            RenderSettings.ambientLight = this.ambientLight;
            RenderSettings.ambientSkyColor = this.ambientSkyColor;
            RenderSettings.ambientEquatorColor = this.ambientEquatorColor;
            RenderSettings.ambientGroundColor = this.ambientGroundColor;
            RenderSettings.customReflection = this.customReflection;
            RenderSettings.reflectionIntensity = this.reflectionIntensity;
            RenderSettings.fog = this.fog;
        }

        private void OnPostRender()
        {
            RenderSettings.defaultReflectionMode = this.savedReflectionMode;
            RenderSettings.ambientMode = this.savedAmbientMode;
            RenderSettings.ambientIntensity = this.savedAmbientIntensity;
            RenderSettings.ambientLight = this.savedAmbientLight;
            RenderSettings.ambientSkyColor = this.savedAmbientSkyColor;
            RenderSettings.ambientEquatorColor = this.savedAmbientEquatorColor;
            RenderSettings.ambientGroundColor = this.savedAmbientGroundColor;
            RenderSettings.customReflection = this.savedCustomReflection;
            RenderSettings.reflectionIntensity = this.savedReflectionIntensity;
            RenderSettings.fog = this.savedFog;
        }
    }
}
