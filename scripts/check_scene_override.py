"""Search Lobby.unity on the server for character scale or scene properties."""
import paramiko

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)

    print("=== Search for character scale or variables in Lobby.unity ===")
    stdin, stdout, stderr = client.exec_command(
        "grep -nC 5 -i 'scale' /home/hung/project2/Assets/Scenes/Lobby.unity"
    )
    print(stdout.read().decode('utf-8', errors='replace'))

    print("=== Search for LobbySetup component in Lobby.unity ===")
    stdin, stdout, stderr = client.exec_command(
        "grep -nC 15 -i 'LobbySetup' /home/hung/project2/Assets/Scenes/Lobby.unity"
    )
    print(stdout.read().decode('utf-8', errors='replace'))

    client.close()

if __name__ == "__main__":
    main()
