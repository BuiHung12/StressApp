"""Fetch unity logs from the server."""
import paramiko

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)

    print("=== Last 50 lines of Unity Editor.log ===")
    stdin, stdout, stderr = client.exec_command('tail -n 50 /home/hung/.config/unity3d/Editor.log')
    print(stdout.read().decode('utf-8', errors='replace'))

    print("=== Custom logs containing [LobbyUI] or [Player] ===")
    stdin, stdout, stderr = client.exec_command('grep -E "\\[LobbyUI\\]|\\[Player\\]" /home/hung/.config/unity3d/Editor.log | tail -30')
    print(stdout.read().decode('utf-8', errors='replace'))

    client.close()

if __name__ == "__main__":
    main()
