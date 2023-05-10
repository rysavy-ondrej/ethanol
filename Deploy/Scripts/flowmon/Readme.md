# Flowmon Host Configuration

Running custom exported that produces JSON output based on the configuration file and
this output is then send via netcat to the remote host for further processing:

```bash
sudo flowmonexp5 probe-ethanol.json | while(true); do nc --send-only 192.168.111.21 5170; done
```

As can be seen the infinite loop is used to reconnect in the case of broken connection.
