# Netify Annotations

Netify is a tool that enables users to track web application IP addresses, domains, and other relevant information. It can be accessed at https://www.netify.ai/products/netify-informatics/application-detection. Netify's offline version, which is stored in Postgres, annotates flows using the IP address of the remote host as the key. Similar services that provide comparable information are available, such as https://ipinfo.io/.

The following tables are used to store the Netify provided data:

## Web Applications

```sql
CREATE TABLE netify_applications (
    id INT PRIMARY KEY,
    tag VARCHAR(100),
    short_name VARCHAR(50),
    full_name VARCHAR(100),
    description TEXT,
    url VARCHAR(255),
    category VARCHAR(50)
);
```
The `netify_applications` table includes the following columns:

* id: an integer column used as the primary key for each row in the table.
* tag: a string column with a maximum length of 50 characters, used to categorize or label the web application.
* short_name: a string column with a maximum length of 50 characters, used to provide a short name or abbreviation for the web application.
* full_name: a string column with a maximum length of 100 characters, used to provide the full name or title of the web application.
* description: a text column used to provide a longer description or summary of the web application.
* url: a string column with a maximum length of 255 characters, used to store the URL or web address for the application.
* category: a string column with a maximum length of 50 characters, used to categorize the web application based on its purpose or function.

## Known IP addresses mapped to applications

```sql
CREATE TABLE netify_addresses (
    id INT PRIMARY KEY,
    value VARCHAR(50),
    ip_version INT,
    shared INT,
    app_id INT,
    platform_id INT,
    asn_tag VARCHAR(20),
    asn_label VARCHAR(128),
    asn_route VARCHAR(50),
    asn_entity_id INT
);
```

The `netify_addresses` table includes the following columns:

* id: an integer column used as the primary key for each row in the table.
* value: a string column with a maximum length of 50 characters, used to store the IPv4 or IPv6 address.
* ip_version: an integer column used to indicate the IP version of the address (e.g. 4 for IPv4, 6 for IPv6).
* shared: an integer column used to indicate whether the IP address is shared (value > 1) or dedicated (value = 1).
* app_id: an integer column used to reference the ID of the web application in netify_applications table associated with the IP address.
* platform_id: an integer column used to reference the ID of the platform or hosting provider associated with the IP address or network.
* asn_tag: a string column with a maximum length of 20 characters, used to store the ASN tag associated with the IP address or network.
* asn_label: a string column with a maximum length of 128 characters, used to provide a label or description for the ASN associated with the IP address.
* asn_route: a string column with a maximum length of 50 characters, used to store the ASN route or prefix associated with the IP address.
* asn_entity_id: an integer column used to reference the ID of the entity or organization associated with the ASN.
