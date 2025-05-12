using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(menuName = "Rendering/Skybox Settings", fileName = "SkyboxSettings")]
    public class SkyboxSettings : ScriptableObject
    {
        [SerializeField] private Material skyboxMaterial;

        public void Setup()
        {
            if (skyboxMaterial != null)
            {
                RenderSettings.skybox = skyboxMaterial;
                DynamicGI.UpdateEnvironment();
            }
            else
            {
                Debug.LogWarning("SkyboxSettings: No skybox material assigned.");
            }
        }
    }
}