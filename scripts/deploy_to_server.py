"""
Deploy Project 2 to Linux server and setup Unity project.
"""
import paramiko
import os
import stat

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"
LOCAL_PROJECT = r"d:\WORK\project\Applications\project2"
REMOTE_PROJECT = "/home/hung/Applications/project2"

def ssh_connect():
    """Connect to server via SSH."""
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    print(f"🔌 Connecting to {USER}@{SERVER}...")
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)
    print("✅ Connected!")
    return client

def run_cmd(client, cmd):
    """Execute a command and return output."""
    print(f"  ▶ {cmd}")
    stdin, stdout, stderr = client.exec_command(cmd, timeout=30)
    out = stdout.read().decode('utf-8', errors='replace').strip()
    err = stderr.read().decode('utf-8', errors='replace').strip()
    if out:
        print(f"    {out}")
    if err:
        print(f"    ⚠ {err}")
    return out, err

def upload_project(client):
    """Upload project2 via SFTP."""
    sftp = client.open_sftp()
    
    # Remove old project if exists
    print(f"\n📁 Cleaning old project at {REMOTE_PROJECT}...")
    run_cmd(client, f"rm -rf {REMOTE_PROJECT}")
    
    # Create directories and upload
    print(f"\n📤 Uploading project to {REMOTE_PROJECT}...")
    
    local_base = LOCAL_PROJECT
    
    for root, dirs, files in os.walk(local_base):
        # Get relative path
        rel_path = os.path.relpath(root, local_base).replace("\\", "/")
        remote_dir = f"{REMOTE_PROJECT}/{rel_path}" if rel_path != "." else REMOTE_PROJECT
        
        # Create remote directory
        try:
            sftp.mkdir(remote_dir)
            print(f"  📂 {remote_dir}")
        except IOError:
            pass  # Already exists
        
        # Upload files
        for f in files:
            local_file = os.path.join(root, f)
            remote_file = f"{remote_dir}/{f}"
            sftp.put(local_file, remote_file)
            size = os.path.getsize(local_file)
            print(f"  📄 {f} ({size} bytes)")
    
    sftp.close()
    print("✅ Upload complete!")

def setup_unity(client):
    """Verify Unity and setup the project."""
    print("\n🔧 Checking Unity installation...")
    
    unity_path = "/home/hung/Unity/Hub/Editor/6000.0.23f1/Editor/Unity"
    out, err = run_cmd(client, f"ls -la {unity_path}")
    
    if "No such file" in err:
        print("❌ Unity Editor not found at expected path!")
        # Try to find it
        run_cmd(client, "find /home/hung -name 'Unity' -type f 2>/dev/null | head -5")
        return False
    
    print("✅ Unity Editor found!")
    
    # Check project structure
    print("\n📋 Project structure on server:")
    run_cmd(client, f"find {REMOTE_PROJECT} -type f | head -30")
    
    # Set permissions
    print("\n🔐 Setting permissions...")
    run_cmd(client, f"chmod -R 755 {REMOTE_PROJECT}")
    
    return True

def create_scene_on_server(client):
    """Create a minimal Unity scene file on the server."""
    print("\n🎬 Creating lobby scene...")
    
    scene_content = r"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 9
  m_Fog: 0
  m_FogColor: {r: 0.5, g: 0.5, b: 0.5, a: 1}
  m_FogMode: 3
  m_FogDensity: 0.01
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {r: 0.6, g: 0.65, b: 0.7, a: 1}
  m_AmbientEquatorColor: {r: 0.4, g: 0.5, b: 0.6, a: 1}
  m_AmbientGroundColor: {r: 0.3, g: 0.3, b: 0.3, a: 1}
  m_AmbientIntensity: 1
  m_AmbientMode: 3
  m_SubtractiveShadowColor: {r: 0.42, g: 0.478, b: 0.627, a: 1}
  m_SkyboxMaterial: {fileID: 0}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {fileID: 0}
  m_SpotCookie: {fileID: 10001, guid: 0000000000000000e000000000000000, type: 0}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {fileID: 0}
  m_Sun: {fileID: 0}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 12
  m_GIWorkflowMode: 1
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_BuildSettings:
    serializedVersion: 3
    agentTypeID: 0
    agentRadius: 0.25
    agentHeight: 1.2
    agentSlope: 45
    agentClimb: 0.4
    ledgeDropHeight: 0
    maxJumpAcrossDistance: 0
    minRegionArea: 2
    manualCellSize: 0
    cellSize: 0.08333334
    manualTileSize: 0
    tileSize: 256
    buildHeightMesh: 0
    maxJobWorkers: 0
    preserveTilesOutsideBounds: 0
  m_NavMeshData: {fileID: 0}
--- !u!1 &100000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 100001}
  - component: {fileID: 100002}
  m_Layer: 0
  m_Name: LobbyManager
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &100001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &100002
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 0, type: 0}
  m_Name:
  m_EditorClassIdentifier: RangerCity.Lobby.LobbySetup
"""
    
    # Write scene file
    run_cmd(client, f"mkdir -p {REMOTE_PROJECT}/Assets/Scenes")
    
    # Use heredoc to write the file
    escaped = scene_content.replace("'", "'\\''")
    run_cmd(client, f"cat > {REMOTE_PROJECT}/Assets/Scenes/Lobby.unity << 'SCENE_EOF'\n{scene_content}\nSCENE_EOF")
    
    print("✅ Scene created!")

def main():
    print("=" * 50)
    print("🚀 Project 2 — Deploy to Server")
    print("=" * 50)
    
    try:
        client = ssh_connect()
        
        # Check server
        print("\n📊 Server info:")
        run_cmd(client, "uname -a")
        run_cmd(client, "df -h / | tail -1")
        
        # Upload
        upload_project(client)
        
        # Create scene
        create_scene_on_server(client)
        
        # Setup Unity
        unity_ready = setup_unity(client)
        
        if unity_ready:
            print("\n" + "=" * 50)
            print("✅ DEPLOY COMPLETE!")
            print("=" * 50)
            print(f"\n📍 Project location: {REMOTE_PROJECT}")
            print("\n📋 Next steps:")
            print("  1. Open TeamViewer to server")
            print("  2. Open Unity Hub")
            print("  3. Add project: /home/hung/Applications/project2")
            print("  4. Open project with Unity 6000.0.23f1")
            print("  5. Open scene: Assets/Scenes/Lobby.unity")
            print("  6. Bake NavMesh: Window > AI > Navigation > Bake")
            print("  7. Press Play! 🎮")
        else:
            print("\n⚠ Unity not found. Check server setup.")
        
        client.close()
        
    except Exception as e:
        print(f"\n❌ Error: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main()
