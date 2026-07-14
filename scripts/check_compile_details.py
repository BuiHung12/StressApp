"""Check full compile status in Editor.log on server."""
import paramiko

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)

    print("=== Compiler errors or warnings in Editor.log ===")
    stdin, stdout, stderr = client.exec_command(
        r"grep -nE -i 'error CS[0-9]+:|Compilation failed' /home/hung/.config/unity3d/Editor.log | tail -50"
    )
    print(stdout.read().decode('utf-8', errors='replace'))

    print("=== Last 10 lines of Editor.log ===")
    stdin, stdout, stderr = client.exec_command(
        "tail -n 20 /home/hung/.config/unity3d/Editor.log"
    )
    print(stdout.read().decode('utf-8', errors='replace'))

    client.close()

if __name__ == "__main__":
    main()
