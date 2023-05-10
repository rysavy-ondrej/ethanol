# Sandbox Host Configuration

The sandbox host is Windows host used to run various network applications and collect process names related to TCP connections.

## Preparation

Consider the fresh Windows 10 installation. The following needs to be installed on the machine to provide a full sandbox capabilities:

* PowerShell 7 to run various monitoring and activity scripts:

```cmd
winget install powershell
```

* Chrome browser:

```cmd
winget install Google.Chrome
````

* Mozilla Firefox

```cmd
winget install Mozilla.Firefox
```

* Tor Browser

```cmd
winget install TorProject.TorBrowser
```

In case of Tor, the installation shuould be moved to Program Files and the firefox executable renamed to `torfox.exe`.

## Deployment

The content of this folder should be deployed on the sandbox host.
To run the sandbox monitoring execute:

```pwsh
.\collect-tcp.ps1 -SendTo 192.168.111.21:5175 -OutFormat ndjson -ProbeInterval 00:00:10 -Duration 00:01:00
```

To start simulated web browsing, execute the follwing command:

```pwsh
.\open-websites.ps1 top-1m.txt
```
