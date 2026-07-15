"""
Open Unity project in batch mode to compile and check for errors.
"""
import paramiko
import time

SERVER = "100.89.39.103"
USER = "hung"
PASSWORD = "1234"
REMOTE_PROJECT = "/home/hung/Applications/project2"
UNITY = "/home/hung/Unity/Hub/Editor/6000.0.23f1/Editor/Unity"

def main():
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    print("Connecting...")
    client.connect(SERVER, port=22, username=USER, password=PASSWORD, timeout=15)
    print("Connected!")
    
    # Run Unity in batch mode to import and compile
    log_file = "/home/hung/Applications/project2/unity_compile.log"
    cmd = (
        f"{UNITY} "
        f"-projectPath {REMOTE_PROJECT} "
        f"-batchmode -nographics -quit "
        f"-logFile {log_file} "
        f"2>&1; echo EXIT_CODE=$?"
    )
    
    print(f"\nRunning Unity batch compile...")
    print(f"Command: {cmd}")
    print("This may take a few minutes for first import...\n")
    
    # Execute with long timeout
    stdin, stdout, stderr = client.exec_command(cmd, timeout=300)
    
    # Stream output
    for line in stdout:
        print(line.strip())
    
    err = stderr.read().decode('utf-8', errors='replace').strip()
    if err:
        print(f"\nStderr: {err}")
    
    # Read compile log for errors
    print("\n--- Checking compile log for errors ---")
    stdin2, stdout2, stderr2 = client.exec_command(
        f"grep -i 'error\\|failed\\|exception' {log_file} | tail -20"
    )
    errors = stdout2.read().decode('utf-8', errors='replace').strip()
    
    if errors:
        print(f"ERRORS FOUND:\n{errors}")
    else:
        print("No compile errors found!")
    
    # Also check if compilation succeeded
    stdin3, stdout3, stderr3 = client.exec_command(
        f"grep -i 'Refresh completed' {log_file} | tail -5"
    )
    success = stdout3.read().decode('utf-8', errors='replace').strip()
    if success:
        print(f"\nCompilation status: {success}")
    
    client.close()

if __name__ == "__main__":
    main()
