# Process Annotations (Hostify database)

Process annotation is based on the database of known domain names used by known applications/processes. 
This database is created by observing the behavior of host running in a sandbox and then filtering and cleaning 
the data to provide accurate information.

The databse may contain not only regular applications but also malware records as obtained from Triage sandbox IoC.

## Applications

```sql
CREATE TABLE hostify_applications (
    id INT PRIMARY KEY,
    tag VARCHAR(100),
    short_name VARCHAR(50),
    full_name VARCHAR(100),
    description TEXT,
    url VARCHAR(255),
    category VARCHAR(50)
);
```

## Indicators of Actvities

Indicators of activities are data points that can be used to identify internet applications. Some common indicators of activities include:

* Domain names: A domain name is a string of characters that represents a unique identifier for a website or network. Domain names can provide information about the owner of a website or the location of a server, and can be used to track internet activity.
* Plain HTTP URL requests: A plain HTTP URL request is a request made by a user or application to access a resource on the internet. The URL can contain information about the resource being requested, as well as any parameters or data associated with the request.
* Contacted IP addresses: An IP address is a numerical identifier that is assigned to each device connected to the internet. When an application or user communicates with another device over the internet, they typically do so by sending data to the other device's IP address. Contacted IP addresses can be used to track internet activity and identify patterns of communication between devices.

By analyzing these indicators of activities, it is possible to identify internet applications and track their usage over time. This information can be used to inform decisions about network security, resource allocation, and application performance, among other things.

`hostify_ioa_domains` table stores information related to domain names that are observed during different activities such as plain HTTP URL requests, contacted IP addresses, and resolved domain names to IP addresses, SNI, or Subject from TLS handshake, or host name provided in HTTP header. The purpose of this table is to identify internet applications based on these observed domains.

```sql
CREATE TABLE hostify_ioa_domains (
    id INT PRIMARY KEY,
    value VARCHAR(100),
    shared INT,
    app_id INT,
    dns_domain BOOLEAN,
    tls_sni BOOLEAN,
    tls_subject BOOLEAN,
);
```

Columns:

* id: An integer column that represents the unique identifier of each domain name record in the table. This column is the primary key of the table.
* value: A string column that represents the domain name.
* shared: An integer column that indicates whether the domain name is shared among multiple applications.
* app_id: An integer column that represents the unique identifier of the application associated with the domain name.
* dns_domain: A boolean column that indicates whether the domain name is valid for matching names in DNS resolutions.
* tls_sni: A boolean column that indicates whether the domain name is valid for matching the SNI information.
* tls_subject: A boolean column that indicates whether the domain name is valid for matching TLS Subject information.


Table `hostify_ioa_urls` stores infromation related to HTTP URL requests related to application communication activities.
The information can be obtained by monitoring HTTP communication of application processes.

```sql
CREATE TABLE hostify_ioa_urls (
    id INT PRIMARY KEY,
    value VARCHAR(200),
    shared INT,
    app_id INT,
);
```

Table `hostify_ioa_ips` stores infromation related to IP addresses related to application activities. The table lists IP connections
established by monitored applications.

```sql
CREATE TABLE hostify_ioa_ips (
    id INT PRIMARY KEY,
    value VARCHAR(50),
    shared INT,
    app_id INT,
);
```
