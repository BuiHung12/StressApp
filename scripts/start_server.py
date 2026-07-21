import paramiko
import time

def start_server():
    c = paramiko.SSHClient()
    c.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    c.connect('100.89.39.103', username='hung', password='1234')
    
    # Kill any existing server
    c.exec_command('pkill -9 -f RangerCityServer')
    time.sleep(1)
    
    # Start server daemon using nohup inside a subshell with redirect
    cmd = "nohup /home/hung/Applications/resolve_stress_project/project2/Builds/Linux/RangerCityServer -batchmode -logFile /home/hung/Applications/resolve_stress_project/project2/Builds/Linux/server_run.log </dev/null >/dev/null 2>&1 &"
    
    # Execute command
    c.exec_command(f"export DISPLAY=:0 && {cmd}")
    time.sleep(2)
    
    # Verify running
    stdin, stdout, stderr = c.exec_command('pgrep -a -f RangerCityServer')
    output = stdout.read().decode('utf-8', errors='ignore')
    print("Running processes:\n" + output)
    
    # Check log
    stdin, stdout, stderr = c.exec_command('tail -n 20 /home/hung/Applications/resolve_stress_project/project2/Builds/Linux/server_run.log')
    log_output = stdout.read().decode('utf-8', errors='ignore')
    print("Latest log:\n" + log_output)
    
    c.close()

if __name__ == '__main__':
    start_server()
