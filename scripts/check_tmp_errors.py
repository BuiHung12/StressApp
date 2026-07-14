"""Check Unity log specifically for TextMesh Pro errors."""
import paramiko

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)

    print("=== TextMesh Pro related errors in Editor.log ===")
    stdin, stdout, stderr = client.exec_command(
        r"grep -i -C 3 'TextMeshPro\|TMP\|ImportPackage' /home/hung/.config/unity3d/Editor.log | tail -50"
    )
    print(stdout.read().decode('utf-8', errors='replace'))

    print("=== Recent error and warning lines in Editor.log ===")
    stdin, stdout, stderr = client.exec_command(
        r"grep -nE -i 'error|exception|fail' /home/hung/.config/unity3d/Editor.log | tail -30"
    )
    print(stdout.read().decode('utf-8', errors='replace'))

    client.close()

if __name__ == "__main__":
    main()
