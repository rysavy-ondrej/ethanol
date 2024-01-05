# Context Provider

The Context Provider is a robust service designed to offer an intuitive REST API interface for accessing enriched host context data. By leveraging this service, clients can retrieve detailed context information associated with various hosts, further enriched with tags for enhanced insights and categorization.

The API provides various endpoints catering to diverse needs, such as counting context objects within a specified time range, enumerating detailed host contexts, and fetching information about the most recent available window.

## API Endpoints

### 1. Retrieve the Count of Context Objects within a Specified Interval

**Endpoint**: `/api/v1/host-context/contexts/count`  
**HTTP Method**: `GET`

**Parameters**:
- `start` (Optional): Specifies the start of the desired interval.
- `end` (Optional): Specifies the end of the desired interval.
- `ip` (Optional): A specific IP address to filter by, fetching contexts associated with this particular host.

**Sample Request**:
```
GET /api/v1/host-context/contexts/count?start=2023-03-10T18:00:00&end=2023-03-10T19:35:00
```

**Sample Response**:
```json
19
```

### 2. Enumerate Detailed Host-contexts within a Specific Interval (Low Performance)

Gets the context for the specified window. This API is suitable for a low amount of data as it fetches and retrieves data in a single response as a single JSON.

**Endpoint**: `/api/v1/host-context/contexts`  
**HTTP Method**: `GET`

**Parameters**:
- `start` (Optional): Start of the desired interval.
- `end` (Optional): End of the desired interval.
- `ip` (Optional): An IP address to filter by, obtaining contexts specific to this host.

**Sample Request**:
```
GET /api/v1/host-context/contexts?start=2023-03-10T18:00:00&end=2023-03-10T19:35:00&hostkey=192.168.111.19
```

**Sample Response**:
```json
[
    {
        "id": 66,
        "key": "192.168.111.19",
        "start": "2023-03-10T18:35:00",
        "end": "2023-03-10T18:40:00",
        ...host context objects...
    }
]
```

### 3. Enumerate Detailed Host-contexts within a Specific Interval

Retrieves the context for the specified window. This API is suitable for larger amounts of data.
The response has the format [NDJSON](https://ndjson.org/) and is returned as a chunked HTTP response. 

**Endpoint**: `/api/v1/host-context/context-stream`  
**HTTP Method**: `GET`

**Parameters**:
- `start` (Optional): Start of the desired interval.
- `end` (Optional): End of the desired interval.
- `ip` (Optional): An IP address to filter by, obtaining contexts specific to this host.

**Sample Request**:
```
GET /api/v1/host-context/context-stream?start=2023-03-10T18:00:00&end=2023-03-10T19:35:00&hostkey=192.168.111.19
```


**Sample Response**:
```json
{ "id": 66, "key": "192.168.111.19", "start": "2023-03-10T18:35:00", "end": "2023-03-10T18:40:00", ...host context objects... }
{ "id": 86, "key": "192.168.111.19", "start": "2023-03-10T18:40:00", "end": "2023-03-10T18:45:00", ...host context objects... }
...
{ "id": 126, "key": "192.168.111.19", "start": "2023-03-10T19:15:00", "end": "2023-03-10T19:20:00", ...host context objects... }
...
```

### 4. Fetch Details of the Last Available Context Window

Gain insights into the last window interval available for context data.

**Endpoint**: `/api/v1/host-context/windows/last`  
**HTTP Method**: `GET`

**Sample Request**:
```
GET /api/v1/host-context/windows/last
```

**Sample Response**:
```json
{
    "start": "2023-03-10T18:45:00",
    "end": "2023-03-10T18:50:00"
}
```
