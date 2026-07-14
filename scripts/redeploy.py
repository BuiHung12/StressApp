"""Upload all scripts to server maintaining folder structure."""
import paramiko
import os

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"
LOCAL_DIR = r"d:\WORK\project\Applications\project2\Assets\Scripts\Lobby"
REMOTE_DIR = "/home/hung/project2/Assets/Scripts/Lobby"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    print("Connecting...")
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)
    
    print("Cleaning old remote C# files...")
    # Clean the remote directory C# files to avoid duplicates
    stdin, stdout, stderr = client.exec_command(f"find {REMOTE_DIR} -type f -name '*.cs' -delete")
    stdout.read() # Wait for completion

    sftp = client.open_sftp()

    count = 0
    for root, dirs, files in os.walk(LOCAL_DIR):
        for f in files:
            if f.endswith('.cs'):
                local_path = os.path.join(root, f)
                rel_path = os.path.relpath(local_path, LOCAL_DIR)
                remote_path = f"{REMOTE_DIR}/{rel_path}".replace('\\', '/')
                remote_subdir = os.path.dirname(remote_path)

                # Ensure remote directory structure exists
                parts = remote_subdir.split('/')
                current = ""
                for part in parts:
                    if not part:
                        continue
                    current += "/" + part
                    try:
                        sftp.stat(current)
                    except IOError:
                        sftp.mkdir(current)

                sftp.put(local_path, remote_path)
                print(f"  [OK] {rel_path}")
                count += 1

    # Also upload manifest.json for Mirror package
    local_manifest = r"d:\WORK\project\Applications\project2\Packages\manifest.json"
    remote_manifest = "/home/hung/project2/Packages/manifest.json"
    sftp.put(local_manifest, remote_manifest)
    print(f"  [OK] manifest.json (Mirror package)")

    sftp.close()
    print(f"\n{count} scripts + manifest uploaded! Stop Play -> Click Unity -> Play again.")
    client.close()

if __name__ == "__main__":
    main()
