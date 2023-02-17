# Creating CSV data with nfdump

## Reading flow within the given interval

Here's an example command to get an output from `nfdump` for the given interval:

```bash
 nfdump -R /data/nfsen/profiles-data/live/127-0-0-1_p3000/ -t "2021/11/09.06:00:00-2021/11/09.18:00:00" -o csv > OUTPUT.csv
```