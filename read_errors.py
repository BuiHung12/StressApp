import paramiko

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect("100.89.39.103", port=22, username="hung", password="1234")
    
    print("=== Unity Editor.log file details ===")
    stdin, stdout, stderr = client.exec_command('ls -l /home/hung/.config/unity3d/Editor.log')
    print(stdout.read().decode('utf-8', errors='replace'))
    
    client.close()

if __name__ == "__main__":
    main()
