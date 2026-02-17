


#  TCP CRUD Real-Time System

A multi-client TCP server built in **C# (.NET)** that handles structured string-based commands to perform **CRUD operations** on Student and Course entities. Every database change is instantly broadcast to connected browser clients through a **WebSocket server**, while a **TypeScript/HTML dashboard** displays updates in real time.

---

## Tech Stack

- **Server** — C# (.NET), TCP Sockets, WebSocket Server
- **Frontend** — TypeScript, HTML, CSS (WebSocket dashboard)
- **Client** — C# TCP Client

---

## Project Structure

```
Tcp-CRUD-realtime-system/
├── TcpServer/          # C# server (TCP + WebSocket)
├── Frontend/           # TypeScript/HTML WebSocket dashboard
└── TcpClient.cs        # Sample TCP client
```

---

## Command Format

Every TCP request follows this structure:

```
<OPERATION>|<ENTITY>|<PAYLOAD>
```

| Part | Values |
|------|--------|
| OPERATION | `CREATE`, `READ`, `UPDATE`, `DELETE` |
| ENTITY | `Student`, `Course` |
| PAYLOAD | JSON string or `ALL` |

### Examples

```
CREATE|Student|{"name":"John","email":"john@test.com"}
READ|Student|ALL
UPDATE|Course|{"courseId":1,"title":"Math","credits":4}
DELETE|Student|{"studentId":1}
```

---

## Response Format

**Success:**
```
OK|<MESSAGE>|<DATA>
```

**Error:**
```
ERROR|<MESSAGE>
```

**Example:**
```
OK|Student Created|{"studentId":1,"name":"John","email":"john@test.com"}
ERROR|Email already exists
```

---

## WebSocket Broadcast

After every successful `CREATE`, `UPDATE`, or `DELETE`, connected WebSocket clients receive:

```json
{
  "event": "UPDATE",
  "entity": "Student",
  "data": { }
}
```

---

## Validation Rules

**Student**
- `name` must not be empty
- `email` must not be empty and must contain `@`
- `email` must be unique

**Course**
- `title` must not be empty and must be unique
- `credits` must be between 1 and 6

---

## Getting Started

### 1. Run the Server

```bash
cd TcpServer
dotnet run
```

The server starts a TCP listener and a WebSocket server simultaneously.

### 2. Run the Frontend Dashboard

```bash
cd Frontend
npm install
npm start
```

Open your browser and connect to the WebSocket to see live updates.

### 3. Run the TCP Client

```bash
dotnet-script TcpClient.cs
```

Or compile and run it as part of a .NET project. The client connects to the server and sends sample CRUD commands.

---

## Entities

**Student**

| Field | Type | Notes |
|-------|------|-------|
| studentId | int | Auto-generated |
| name | string | Required |
| email | string | Required, unique |

**Course**

| Field | Type | Notes |
|-------|------|-------|
| courseId | int | Auto-generated |
| title | string | Required, unique |
| credits | int | Required, 1–6 |

---

## Features

- Multiple TCP clients supported simultaneously (multi-threaded)
- Thread-safe database operations
- Input validation with descriptive error messages
- Real-time WebSocket broadcast to all connected dashboard clients
- Graceful handling of malformed commands and invalid JSON
