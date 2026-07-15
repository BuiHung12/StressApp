"""Check git status on server."""
import paramiko

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    print("Connecting...")
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)

    print("Checking git on server...")
    stdin, stdout, stderr = client.exec_command(
        'cd /home/hung/Applications/project2 && git status'
    )
    print("STDOUT:")
    print(stdout.read().decode())
    print("STDERR:")
    print(stderr.read().decode())

    client.close()

if __name__ == "__main__":
    main()
