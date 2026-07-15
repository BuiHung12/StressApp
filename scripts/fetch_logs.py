"""Fetch unity logs from the server."""
import paramiko

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)

    print("=== Custom movement logs (Player, NPC, FakePlayer) ===")
    stdin, stdout, stderr = client.exec_command('grep -E "\\[PlayerController\\]|\\[NPCController\\]|\\[FakePlayerController\\]" /home/hung/.config/unity3d/Editor.log | tail -n 150')
    print(stdout.read().decode('utf-8', errors='replace'))

    client.close()

if __name__ == "__main__":
    main()
