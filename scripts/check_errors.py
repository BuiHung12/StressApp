"""Check Unity Editor.log on server for compile errors."""
import paramiko

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)

    # Check Unity Editor.log for recent errors
    print("=== Recent compile errors ===")
    stdin, stdout, stderr = client.exec_command(
        r"grep -i 'error CS\|CompilerOutput\|Script compilation' /home/hung/.config/unity3d/Editor.log 2>/dev/null | tail -30"
    )
    print(stdout.read().decode())

    # Also check if all .cs files are on server
    print("=== Script files on server ===")
    stdin, stdout, stderr = client.exec_command(
        'ls -la /home/hung/Applications/project2/Assets/Scripts/Lobby/*.cs'
    )
    print(stdout.read().decode())

    client.close()

if __name__ == "__main__":
    main()
