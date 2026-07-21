import paramiko
import time
import sys

# Ensure UTF-8 output on Windows console
sys.stdout.reconfigure(encoding='utf-8')

def check_server():
    c = paramiko.SSHClient()
    c.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    c.connect('100.89.39.103', username='hung', password='1234')
    
    # Check processes
    stdin, stdout, stderr = c.exec_command('pgrep -a -f RangerCityServer')
    output = stdout.read().decode('utf-8', errors='ignore')
    print("=== RUNNING PROCESSES ===")
    print(output if output.strip() else "NO PROCESS FOUND!")
    
    # Check port 7777
    stdin, stdout, stderr = c.exec_command('ss -tuln | grep 7777 || netstat -tuln | grep 7777')
    port_output = stdout.read().decode('utf-8', errors='ignore')
    print("=== PORT 7777 BINDING ===")
    print(port_output if port_output.strip() else "PORT NOT BOUND!")
    
    # Check latest logs
    stdin, stdout, stderr = c.exec_command('tail -n 25 /home/hung/Applications/resolve_stress_project/project2/Builds/Linux/server_run.log')
    log_output = stdout.read().decode('utf-8', errors='ignore')
    print("=== LATEST LOGS ===")
    print(log_output)
    
    c.close()

if __name__ == '__main__':
    check_server()
