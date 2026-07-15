using UnityEditor;
using UnityEngine;

namespace RangerCity.Editor
{
    [InitializeOnLoad]
    public static class ConfigureInsecureHttp
    {
        static ConfigureInsecureHttp()
        {
            if (PlayerSettings.insecureHttpOption != InsecureHttpOption.AlwaysAllowed)
            {
                PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
                AssetDatabase.SaveAssets();
                Debug.Log("[ConfigureInsecureHttp] Set PlayerSettings.insecureHttpOption to AlwaysAllowed.");
            }
        }
    }
}
