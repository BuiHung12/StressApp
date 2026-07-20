using UnityEditor;
using UnityEngine;
using UnityEditor.Android;
using System.Xml;
using System.IO;

namespace RangerCity.Editor
{
    /// <summary>
    /// Post-processes the generated Android Gradle project to inject the cleartext traffic attribute,
    /// enabling connections to the HTTP database server on Android 9+ (API level 28+).
    /// </summary>
    public class AndroidCleartextManifestProcessor : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 100000; // Run after other manifest modifications

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            if (!File.Exists(manifestPath))
            {
                Debug.LogWarning($"[AndroidCleartextManifestProcessor] AndroidManifest.xml not found at: {manifestPath}");
                return;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(manifestPath);

                XmlElement manifestElement = doc.DocumentElement; // This is the /manifest element
                if (manifestElement == null)
                {
                    Debug.LogError("[AndroidCleartextManifestProcessor] Could not find root manifest element.");
                    return;
                }

                string androidNamespaceURI = manifestElement.GetAttribute("xmlns:android");
                if (string.IsNullOrEmpty(androidNamespaceURI))
                {
                    Debug.LogError("[AndroidCleartextManifestProcessor] Could not find android namespace URI.");
                    return;
                }

                XmlElement applicationElement = (XmlElement)doc.SelectSingleNode("/manifest/application");
                if (applicationElement == null)
                {
                    Debug.LogError("[AndroidCleartextManifestProcessor] Could not find application element.");
                    return;
                }

                // Inject android:usesCleartextTraffic="true"
                applicationElement.SetAttribute("usesCleartextTraffic", androidNamespaceURI, "true");
                
                doc.Save(manifestPath);
                Debug.Log("[AndroidCleartextManifestProcessor] Successfully added android:usesCleartextTraffic=\"true\" to AndroidManifest.xml");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AndroidCleartextManifestProcessor] Failed to modify AndroidManifest.xml: {e.Message}");
            }
        }
    }
}
