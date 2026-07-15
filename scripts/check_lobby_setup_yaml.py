"""Show LobbySetup component properties in Lobby.unity."""
import paramiko

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)

    print("=== LobbySetup component properties ===")
    stdin, stdout, stderr = client.exec_command(
        "grep -A 20 'm_EditorClassIdentifier: RangerCity.Lobby.LobbySetup' /home/hung/Applications/resolve_stress_project/project2/Assets/Scenes/Lobby.unity"
    )
    print(stdout.read().decode('utf-8', errors='replace'))

    client.close()

if __name__ == "__main__":
    main()
