# Detectors

## Anonymization

### TOR Detector
Both TOR TLS and HTTP TLS connections use the Transport Layer Security (TLS) protocol to secure communications, but there are some differences that can help distinguish between them:

* Port number: TOR connections typically use port 9001, while HTTP connections typically use port 80 or 443. However, it's important to note that port numbers can be changed or disguised, so this is not a foolproof way to distinguish between the two.

* Server certificate: The server certificate presented during a TLS handshake can give clues about whether a connection is using TOR or not. For example, if the certificate is issued to a .onion domain, it is likely a TOR connection. Additionally, if the certificate is issued by a certificate authority (CA) that is not trusted by major web browsers (e.g. self-signed), it could also be a sign of a TOR connection.

* HTTP headers: TOR connections often include specific HTTP headers, such as "X-Tor-Exit-Node" or "X-Onion-Routing", which can indicate that the connection is using TOR.

* Network traffic analysis: TOR connections typically use more hops than HTTP connections, and the traffic may have different patterns or signatures that can be analyzed to determine whether it is using TOR or not.

We observed the following behavior:

* Tor client creates a connection to IP address without resolving its domain name.
* Tor attempts to connect either to 9001 or 443 port.
* The SNI options in TLS1.2 is randomly generated domain of the following form: www.RANDOM.[com|net] 
* Server (if provided) certificate contains randomly generated issuer name and subject name.

### DoH
DNS over HTTPS (DoH) is a protocol for performing Domain Name System (DNS) resolution over HTTPS. The goal of DoH is to enhance user privacy and security by encrypting DNS queries and responses using HTTPS, the same protocol used to secure web traffic. By encrypting DNS traffic, DoH helps to prevent eavesdropping and interception of DNS queries and responses by third parties, including internet service providers (ISPs) and other network administrators.

DoH works by encapsulating DNS requests and responses in HTTPS packets, which are then sent to and from a DoH server. The DoH server acts as a resolver, querying the DNS hierarchy to resolve domain names and returning the results to the client in an encrypted format. This helps to prevent third parties from intercepting and snooping on the DNS traffic, which can reveal information about the websites a user is visiting and the services they are using.

### Anonymous VPN
An anonymization VPN, also known as an anonymous VPN, is a type of virtual private network (VPN) that is designed to enhance user privacy and security by providing anonymous and encrypted Internet connections. When a user connects to an anonymization VPN, their traffic is routed through the VPN server, which encrypts the traffic and assigns the user a new IP address. This can help to prevent third parties from tracking the user's online activities, including their browsing history, online purchases, and communication data.

Anonymization VPNs typically use a combination of encryption protocols and technologies to secure user data and maintain privacy. For example, they may use OpenVPN, IKEv2, or L2TP/IPSec to encrypt the user's data, and may also include additional features such as a kill switch or DNS leak protection to ensure that the user's data remains secure.

## Remote Access and Management

### Remote Desktop Connection

### Google Remote Desktop

### TeamViewer

### SSH



## Malicious and unwanted

### Cryptominers

### Malware 


## Social Networks ?



