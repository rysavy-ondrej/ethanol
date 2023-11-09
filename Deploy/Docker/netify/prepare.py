import csv
import os
import json

# Define file paths
applications_file = 'applications.csv'
domains_file = 'domains.csv'
ips_file = 'ips.csv'
output_file = 'netify.csv'

print(f"This tool will proces Netify CSV files:")
print(f"  {applications_file} -- list of known internet applications")
print(f"  {domains_file} -- list of recognized domain names")
print(f"  {ips_file} -- list of IP addresses")

# Function to read applications and return a dictionary {app_id: tag}
def read_applications(filename):
    applications = {}
    with open(filename, mode='r', encoding='utf-8') as f:
        reader = csv.reader(f, delimiter=';')
        next(reader)  # Skip the header
        for row in reader:
            app_id, tag = row[0], row[1]
            details = {'Tag': row[1], 'ShortName': row[2],'FullName':row[3],'Category':row[5]}
            applications[app_id] = [tag,details]
    return applications

# Read applications to dictionary
applications = read_applications(applications_file)

print(f"Applications loaded. Number of applications={len(applications)}.")

domains = 0
ips = 0
# Read domains and ips, and write the netify.csv
with open(domains_file, mode='r', encoding='utf-8') as domains_f, \
     open(ips_file, mode='r', encoding='utf-8') as ips_f, \
     open(output_file, mode='w', encoding='utf-8', newline='\n') as out_f:

    domains_reader = csv.reader(domains_f, delimiter=';')
    ips_reader = csv.reader(ips_f, delimiter=';')
    output_writer = csv.writer(out_f, delimiter=';',quotechar='"',quoting=csv.QUOTE_MINIMAL)

    # Write header to the output file
    output_writer.writerow(['type', 'key', 'value', 'reliability', 'validity', 'details'])

    # Skip headers in source files
    next(domains_reader)
    next(ips_reader)

    print(f"Indexing domains...")
    # Process domains and write to the output
    for domain_row in domains_reader:
        app_id = domain_row[2]
        if app_id in applications:
            output_writer.writerow(['NetifyDomain', domain_row[1][:64], applications[app_id][0][:128],1.0,'[-infinity,infinity]',json.dumps(applications[app_id][1]) ])
            domains+=1
    print(f"Indexing IPs...")
    # Process IPs and write to the output
    for ip_row in ips_reader:
        app_id = ip_row[4]
        if app_id in applications:
            output_writer.writerow(['NetifyIp', ip_row[1][:64], applications[app_id][0][:128],1.0,'[-infinity,infinity]',json.dumps(applications[app_id][1]) ])
            ips+=1

print(f"Netify CSV file 'netify.csv' has been created. Number of domain records={domains}, ip records={ips}.")
print()
print("To deleted obsolete data execute delete command in the psql:")
print()
print("DELETE FROM netify_data")
print()
print("To insert the data in the database execute the following command in the connected psql:")
print()
print("\COPY netify_data (type,key,value,reliability,validity,details) FROM netify.csv WITH DELIMITER ',' CSV HEADER;")
print()
print()
print("... or simply use update-netify.sql script:")
print()
print("psql -U postgres -d ethanol -f insert-data.sql")
