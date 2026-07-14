"""Import TMP Essentials and verify compilation on server."""
import paramiko

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    print("Connecting...")
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)

    # Check if TMP Essential Resources already exist
    stdin, stdout, stderr = client.exec_command(
        'ls -la /home/hung/project2/Assets/TextMesh\\ Pro/ 2>/dev/null || echo "NOT_FOUND"'
    )
    result = stdout.read().decode()
    print(f"TMP folder: {result.strip()}")

    # Check compilation errors
    stdin, stdout, stderr = client.exec_command(
        'ls -la /home/hung/project2/Assets/Scripts/Lobby/*.cs | wc -l'
    )
    count = stdout.read().decode().strip()
    print(f"Script files in Lobby: {count}")

    # Check if there are any .cs files referencing old methods
    stdin, stdout, stderr = client.exec_command(
        'grep -rn "CreateTrees()" /home/hung/project2/Assets/Scripts/Lobby/ 2>/dev/null || echo "No old refs"'
    )
    old_refs = stdout.read().decode().strip()
    print(f"Old method refs: {old_refs}")

    # Check NPCController for _moveSpeed field
    stdin, stdout, stderr = client.exec_command(
        'grep -n "_moveSpeed\|_wanderPause\|_wanderRadius" /home/hung/project2/Assets/Scripts/Lobby/NPCController.cs 2>/dev/null'
    )
    npc_fields = stdout.read().decode().strip()
    print(f"NPC fields: {npc_fields}")

    client.close()

if __name__ == "__main__":
    main()
