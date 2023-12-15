# Sandbox Host Configuration

This document outlines the configuration of a Windows-based sandbox host. The sandbox host is used to run various network applications and collect process names related to TCP connections.

## Preparation

Start with a fresh installation of Windows 10. To equip the sandbox host with full capabilities, install the following components:

### Required Software

1. **PowerShell 7**: Used for running monitoring and activity scripts.
   - Installation command:
     ```cmd
     winget install powershell
     ```

2. **Chrome Browser**: Essential for web browsing simulations.
   - Installation command:
     ```cmd
     winget install Google.Chrome
     ```

3. **Mozilla Firefox**: Another browser for varied web simulations.
   - Installation command:
     ```cmd
     winget install Mozilla.Firefox
     ```

4. **Tor Browser**: For secure and anonymous web browsing.
   - Installation command:
     ```cmd
     winget install TorProject.TorBrowser
     ```
   - **Note**: After installing Tor, move the installation to the `Program Files` directory and rename the Firefox executable to `torfox.exe`.

### Additional Setup

Ensure that the machine is configured with the necessary network settings and permissions to run these applications effectively.

## Deployment

Deploy the contents of this repository to the sandbox host for monitoring and simulation purposes.

### Running the Sandbox Monitoring

To initiate monitoring, execute the following PowerShell script:

```pwsh
.\collect-tcp.ps1 -SendTo 192.168.111.21:5175 -OutFormat ndjson -ProbeInterval 00:00:10 -Duration 00:01:00
```

This script collects TCP connection data and sends it to the specified IP and port in `ndjson` format. The `-ProbeInterval` and `-Duration` parameters can be adjusted as needed.

### Simulating Web Browsing

To start simulated web browsing, use this command:

```pwsh
.\open-websites.ps1 top-1m.txt
```

This script opens websites listed in the `top-1m.txt` file, simulating real-world web browsing behavior.

---

Remember, the deployment and operation of a sandbox host should be conducted in compliance with your organization's IT policies and guidelines.