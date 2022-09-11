The root point is for malware samples:

https://bazaar.abuse.ch/browse/

---

Other sandboxes:
https://www.capesandbox.com/analysis/

Information sources:

https://www.virustotal.com/

---

To find the related malware sample in APP.ANY.RUN sandbox, open:

https://app.any.run/tasks/<<<UID>>>

To produce flows from pcap:

sudo flowmonexp5 -I pcap-replay:file=66642dc0-b79e-469a-b0d2-41cc6457827a.pcap,speed=1 -P nbar2 -P tls:fields=MAIN#JA3#CLIENT#CERT -P dns -P http -E json > 66642dc0-b79e-469a-b0d2-41cc6457827a.flows.json

to generate context:

Ethanol.Console Build-HostContext -i 66642dc0-b79e-469a-b0d2-41cc6457827a.flows.json > 66642dc0-b79e-469a-b0d2-41cc6457827a.context.yaml
