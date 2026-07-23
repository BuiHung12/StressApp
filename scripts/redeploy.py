"""Automated 4-step deployment script for Ranger City Linux Game Server."""
import subprocess
import time
import paramiko

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"
PROJECT_PATH = "/home/hung/Applications/resolve_stress_project/project2"

def main():
    print("=== Step 1: Git Push local changes ===")
    subprocess.run(["git", "add", "-A"], check=True)
    subprocess.run(["git", "commit", "-m", "Automated deployment sync"], check=False)
    subprocess.run(["git", "push", "origin", "main"], check=True)

    print("\n=== Connecting SSH to Linux Server ===")
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)

    print("\n=== Step 2: Git Pull & Clean stale Unity processes ===")
    cmd_pull = f"cd {PROJECT_PATH} && git pull origin main && pkill -9 Unity; killall -9 Unity"
    stdin, stdout, stderr = client.exec_command(cmd_pull)
    stdout.channel.recv_exit_status()
    print("Git pull completed.")

    print("\n=== Step 3: Unity Batchmode Build Linux Standalone Server ===")
    cmd_build = f"/home/hung/Unity/Hub/Editor/6000.0.23f1/Editor/Unity -batchmode -nographics -projectPath {PROJECT_PATH} -executeMethod RangerCity.Lobby.Editor.BuildScript.BuildLinuxServer -quit -logFile {PROJECT_PATH}/Builds/Linux/build.log"
    stdin, stdout, stderr = client.exec_command(cmd_build)
    stdout.channel.recv_exit_status()
    print("Linux Game Server binary build completed.")

    print("\n=== Step 4: Restart systemd rangercity service ===")
    cmd_restart = "systemctl --user restart rangercity"
    stdin, stdout, stderr = client.exec_command(cmd_restart)
    stdout.channel.recv_exit_status()
    print("Service rangercity restarted successfully!")

    client.close()
    print("\n[SUCCESS] Server update fully completed!")

if __name__ == "__main__":
    main()
