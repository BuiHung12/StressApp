"""Read Lobby.unity lines around MonoBehaviour."""
import paramiko

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)

    print("=== MonoBehaviour lines 98 to 140 ===")
    stdin, stdout, stderr = client.exec_command(
        "sed -n '98,140p' /home/hung/Applications/project2/Assets/Scenes/Lobby.unity"
    )
    print(stdout.read().decode('utf-8', errors='replace'))

    client.close()

if __name__ == "__main__":
    main()
