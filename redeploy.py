"""Upload all scripts to server."""
import paramiko, os

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"
LOCAL_DIR = r"d:\WORK\project\Applications\project2\Assets\Scripts\Lobby"
REMOTE_DIR = "/home/hung/project2/Assets/Scripts/Lobby"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    print("Connecting...")
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)
    sftp = client.open_sftp()

    count = 0
    for f in os.listdir(LOCAL_DIR):
        if f.endswith('.cs'):
            sftp.put(os.path.join(LOCAL_DIR, f), f"{REMOTE_DIR}/{f}")
            print(f"  ✅ {f}")
            count += 1

    sftp.close()
    print(f"\n{count} scripts uploaded! Stop Play → Click Unity → Play again.")
    client.close()

if __name__ == "__main__":
    main()
