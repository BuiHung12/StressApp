"""Verify server files have new content."""
import paramiko

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)

    stdin, stdout, stderr = client.exec_command('find /home/hung/Applications/resolve_stress_project/project2/Assets/Scripts/Lobby/ -maxdepth 3')
    print("Remote directory structure:")
    print(stdout.read().decode())

    client.close()

if __name__ == "__main__":
    main()
