# Redact PCAP Files

This guide provides instructions for creating datasets by merging benign/normal traffic captures with captures obtained from the analysis performed by Tria.ge sandbox.

## Prerequisites

Ensure the following software is installed:

1. **Wireshark/Tshark**: For analyzing and processing PCAP files.
   
2. **dotnet-script**: For running C# scripts to manipulate PCAP files.

## Procedure

To create a dataset that includes malware, follow these steps:

### 1. Prepare Normal/Background PCAP

   Start with a normal traffic capture file named `background.pcap`.

### 2. Prepare Malware PCAP

   Obtain a malware capture file and name it `malware.pcap`.

### 3. Analyze Capture Files

   Analyze the `background.pcap` and `malware.pcap` files to gather necessary information, such as packet times and endpoints. This will guide adjustments in later steps.

   Example command to get information:

   ```bash
   capinfos.exe PCAPS/background.pcap
   ```

   Focus on the 'First packet time' and 'Last packet time' of `background.pcap`. This data will be important for time-shifting the malware capture in step 5.

### 4. Modify Malware Capture IP Addresses

   Change IP addresses in `malware.pcap` to match a selected machine in the background pcap:

   ```bash
   dotnet-script replace-address.csx -- -r 10.127.0.60:192.168.111.19 -i PCAPS/malware.pcap -o PCAPS/malware.1.pcap
   ```

### 5. Time-Shift Malware Activity

   Adjust the timing of the malware capture so that its activity falls within the interval of the background pcap, ideally in the middle.

   ```bash
   dotnet-script shift-time.csx -- -s 2023-03-10T11:11:00 -i PCAPS/malware.1.pcap -o PCAPS/malware.2.pcap
   ```

### 6. Merge PCAP Files

   Combine the modified malware pcap with the background pcap to produce the final dataset:

   ```bash
   mergecap -F pcap -w PCAPS/infected-agenttesla.pcap PCAPS/background.pcap PCAPS/malware.2.pcap
   ```

### 7. Final Dataset

   The resulting file `PCAPS/infected-agenttesla.pcap` will contain both the background traffic and the malware traffic, ready for analysis or training purposes.

