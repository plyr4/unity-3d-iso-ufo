#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

namespace ProPixelizer
{
    /// <summary>
    /// Provides some simple verifications to ensure that the asset is being used correctly,
    /// and provides messages to users to help them figure out when it is not.
    /// </summary>
    public class ProPixelizerVerification
    {
        public static void Check()
        {
            #if UNITY_EDITOR
            if (QualitySettings.renderPipeline as UniversalRenderPipelineAsset == null)
            {
                Debug.LogError("You have not currently set the render pipeline asset in the 'Quality' project settings. ProPixelizer requires a Universal Render Pipeline Asset.");
            }

            if (GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset == null)
            {
                Debug.LogError("You have not currently set the render pipeline asset in the 'Graphics' project settings. ProPixelizer requires a Universal Render Pipeline Asset.");
            }

            if (GraphicsSettings.renderPipelineAsset != QualitySettings.renderPipeline)
            {
                Debug.LogWarning("You are currently using different render pipeline assets for the graphics settings (Project Settings > Graphics) and quality settings (Project Settings > Quality). This may lead to unintended behavior for ProPixelizer.");
            }

            // In the future, I hope that Unity makes ShaderGraphPreferences public so that I can use that rather than hard-coded names here.
            int variantLimit = EditorPrefs.GetInt("UnityEditor.ShaderGraph.VariantLimit", 128);
            if (variantLimit < 256)
            {
                Debug.LogWarning(string.Format(
                    "The ShaderGraph Variant Limit is currently set to a value of {0}. " +
                    "The ProPixelizer appearance shader will not compile unless this variant " +
                    "is raised, and your shaders will appear pink. Please increase this limit, " +
                    "e.g. to 256, by changing Preferences > ShaderGraph > Shader Variant Limit. " +
                    "Afterwards, reimport the ProPixelizer folder.",
                    variantLimit));
            }
            #endif
        }
    }
}