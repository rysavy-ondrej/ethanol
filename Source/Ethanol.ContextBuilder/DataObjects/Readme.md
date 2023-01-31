# DataObjects

In this folder, the POCO representations for different input data is implemented:

* FlowmonExportEntry -- object representing JSON record created by flowmonexp5 tool. The export is an independned colleciton of JSON objects.
* IpfixcolEntry -- object representing JSON record created by ipfixcol2 tool. The export is NDJSON file.
* NfdumpEntry -- represents a single flow record as exported from nfdump. The export is CSV file.