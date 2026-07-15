"""Auto-import TMP Essentials on server via Unity batch mode."""
import paramiko

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"

IMPORT_SCRIPT = '''
using UnityEngine;
using UnityEditor;
using TMPro;

public class AutoImportTMP
{
    [InitializeOnLoadMethod]
    static void ImportTMPEssentials()
    {
        // Check if TMP already imported
        string tmpPath = "Assets/TextMesh Pro";
        if (System.IO.Directory.Exists(tmpPath))
        {
            Debug.Log("[AutoImportTMP] TMP Essentials already imported.");
            return;
        }

        Debug.Log("[AutoImportTMP] Importing TMP Essentials...");
        
        // Use TMP's built-in import
        string packagePath = "Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage";
        if (System.IO.File.Exists(packagePath))
        {
            AssetDatabase.ImportPackage(packagePath, false);
            Debug.Log("[AutoImportTMP] TMP Essentials import triggered!");
        }
        else
        {
            Debug.LogWarning("[AutoImportTMP] Could not find TMP package at: " + packagePath);
            // Try alternative path
            string altPath = UnityEditor.PackageManager.PackageInfo
                .FindForAssetPath("Packages/com.unity.textmeshpro")?.resolvedPath;
            if (altPath != null)
            {
                string fullPath = System.IO.Path.Combine(altPath, "Package Resources", "TMP Essential Resources.unitypackage");
                if (System.IO.File.Exists(fullPath))
                {
                    AssetDatabase.ImportPackage(fullPath, false);
                    Debug.Log("[AutoImportTMP] TMP Essentials imported from: " + fullPath);
                }
            }
        }
    }
}
'''

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    print("Connecting...")
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)

    sftp = client.open_sftp()

    # Create Editor folder
    try:
        sftp.mkdir("/home/hung/Applications/resolve_stress_project/project2/Assets/Editor")
    except:
        pass

    # Write auto-import script
    with sftp.open("/home/hung/Applications/resolve_stress_project/project2/Assets/Editor/AutoImportTMP.cs", "w") as f:
        f.write(IMPORT_SCRIPT)
    print("  ✅ AutoImportTMP.cs uploaded to Editor/")

    sftp.close()
    print("Done! When Unity recompiles, TMP will auto-import.")
    client.close()

if __name__ == "__main__":
    main()
