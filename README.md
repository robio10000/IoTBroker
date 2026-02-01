# IoTBroker 🚀
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/en-us/)
[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-yellow)](https://www.apache.org/licenses/LICENSE-2.0)

![Static Badge](https://img.shields.io/badge/Build-passing-brightgreen)
![Static Badge](https://img.shields.io/badge/Testing-Unit%20Tests-red)

![Static Badge](https://img.shields.io/badge/Database-PostgreSQL%20%7C%20MySQL%20%7C%20SQLite-blue)
![Static Badge](https://img.shields.io/badge/RuleEngine-AdvancedMultiStrategy-blue)
![Static Badge](https://img.shields.io/badge/Actions-WebHooks%20%7C%20DeviceState-blue)
![Static Badge](https://img.shields.io/badge/Security-APIKeyAuthentication-blue)

![Static Badge](https://img.shields.io/badge/Docs-Swagger%2FOpenAPI-blue)
![Static Badge](https://img.shields.io/badge/Support-OpenSource-blue)

---

**A high-performance, rule-based IoT Data Broker built with .NET 9.**

IoTBroker serves as a central intelligence layer for smart environments. It processes incoming sensor payloads, evaluates them against a flexible rule engine, and triggers automated actions.

---

## 🛠 Tech Stack

* **Framework:** .NET 9 (Web API)
* **Persistence:** Entity Framework Core 9.0 (supporting **PostgreSQL**, **MySQL**, **SQLite**)
* **Security:** Custom `X-API-KEY` Middleware & Client Isolation
* **Logic:** Polymorphic Rule Engine with Parallel Action Execution (`Task.WhenAll`)
* **Documentation:** Swagger / OpenAPI 3.0 (available at `/doc`)

---

### 🚀 Getting Started

#### 1. Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) for DB migrations and local builds.
* [Docker & Docker Compose (for containerized DB setup)](https://docs.docker.com/compose/install/)

#### 2. Configuration

IoTBroker supports multiple database providers. Copy the `.env.example` to `.env` and adjust the settings as needed.:

```json
# --- API CONFIGURATION ---
EXTERNAL_PORT=5500
INTERNAL_PORT=8080
DOTNET_ENV=Development

# --- DATABASE CONFIGURATION ---
DB_USEDOCKER=true
DB_PROVIDER=Postgres
DB_HOST=db
DB_NAME=iot_broker_metadata
DB_USER=broker_admin
DB_PASSWORD=adminpassword
DB_EXTERNAL_PORT=5432
DB_INTERNAL_PORT=5432

```

#### 3. Run the Application

```bash
# Run first to look for failures:
docker compose up

# After successful build, run in detached mode (In the background):
docker compose up -d

```

#### 4. Database Migrations

Apply the migrations for your configured provider:

```bash
dotnet ef database update --context IoTContext --project IoTBroker
# For PostgreSQL, use:
dotnet ef database update --project IoTBroker/IoTBroker.csproj --startup-project IoTBroker/IoTBroker.csproj --context IoTBroker.Infrastructure.Data.IoTContext --configuration Debug 20260118211227_Initial_Postgres --connection Host=YOUR_DB_HOST;Database=YOUR_DB_NAME;Username=YOUR_DB_USERNAME;Password=YOUR_DB_PASSWORD

# For MySQL, use:
dotnet ef database update --project IoTBroker/IoTBroker.csproj --startup-project IoTBroker/IoTBroker.csproj --context IoTBroker.Infrastructure.Data.IoTContext --configuration Debug 20260118212128_Initial_MySQL --connection Server=YOUR_DB_HOST;Database=YOUR_DB_NAME;User=YOUR_DB_USERNAME;Password=YOUR_DB_PASSWORD

# For SQLite, use:
dotnet ef database update --project IoTBroker/IoTBroker.csproj --startup-project IoTBroker/IoTBroker.csproj --context IoTBroker.Infrastructure.Data.IoTContext --configuration Debug 20260118212208_Initial_SQLite --connection "Data Source=/.../ProjectRoot/IoTBroker/iotbroker.db"
```

*If Ef Core tools are not installed, run:*

```bash
dotnet tool install --global dotnet-ef
```

Once started, the interactive API documentation is available at: `http://localhost:5500/doc` (or your configured port).

---

#### 5. Authentication

Include the following header in your requests:

* **Header Key:** `X-API-KEY`
* **Default Keys for Development:**
* **Admin Key:** `admin-key-123`
* **Client Key:** `client-key-123`

---

## ⚙️ Rule Engine & Automation

The Rule Engine allows you to define automated workflows. A rule consists of **Conditions** (logic) and **Actions** (output).

### 📋 Rule Definition Reference

| Field             | Type      | Description                                                                                           |
|-------------------|-----------|-------------------------------------------------------------------------------------------------------|
| `name`            | `String`  | Unique name of the rule.                                                                              |
| `logicalOperator` | `Enum`    | **`All`** (AND): All conditions must be met. <br> **`Any`** (OR): At least one condition must be met. |
| `conditions` *1   | `Array`   | List of conditions to be evaluated.                                                                   |
| `actions` *2      | `Array`   | List of actions to execute when triggered.                                                            |
| `isActive`        | `Boolean` | Enables or disables the rule.                                                                         |

#### Condition Fields *1

| Field            | Description                                       |
|------------------|---------------------------------------------------|
| `deviceId`       | The ID of the device being monitored.             |
| `operator`       | `Equals`, `NotEquals`, `GreaterThan`, `LessThan`. |
| `thresholdValue` | The reference value (as a string).                |

#### Action Fields by `$type` (Discriminator) *2

| Action Type       | `$type`     | Required Fields                               |
|-------------------|-------------|-----------------------------------------------|
| **WebHook**       | `webhook`   | `url`, `method`, `headers`, `payloadTemplate` |
| **Device Update** | `set_value` | `targetDeviceId`, `valueType`, `newValue`     |

---

### 🔍 Supported Replacer Tokens

Inject dynamic data into WebHook URLs, Payloads, or Headers:

| Token          | Description                    | Example Output         |
|----------------|--------------------------------|------------------------|
| `{device}`     | ID of the triggering device    | `temp-sensor-01`       |
| `{value}`      | Current sensor value           | `22.5`                 |
| `{value.type}` | Data type of the value         | `Numeric`              |
| `{rule.name}`  | Name of the triggered rule     | `High Temp Alert`      |
| `{timestamp}`  | Execution Timestamp (ISO 8601) | `2026-01-25T18:00:00Z` |

---

### 🚀 Usage Examples (POST `/api/Rules`)

#### Example: Dynamic WebHook Alert

Triggers when "TempSensor" is greater than 25.

```json
{
  "name": "High Temp WebHook",
  "logicalOperator": "All",
  "conditions": [
    { "deviceId": "TempSensor", "operator": "GreaterThan", "thresholdValue": "25" }
  ],
  "actions": [
    {
      "$type": "webhook",
      "url": "https://api.service.com/alert?id={device}",
      "method": "POST",
      "headers": { "X-Sensor": "{value.type}" },
      "payloadTemplate": "{ \"msg\": \"Critical: {value} at {timestamp}\" }"
    },
    {
      "$type": "set_value",
      "targetDeviceId": "CoolingSystem",
      "valueType": "boolean",
      "newValue": "true"
    }
  ],
  "isActive": true
}

```

---

## 🗺 Roadmap

This project is under active development. Our upcoming milestones include:

| Phase | Title                                 | Key Milestone                                                       |
|-------|---------------------------------------|---------------------------------------------------------------------|
| 🚧    | **v1.2.0 - Resilience & DevOps**      | Implement global error handling, logging, Dockerization, and CI/CD. |
| 📅️    | **v1.3.0 - Testing Suite**            | Develop comprehensive unit and integration tests.                   |
| 📅    | **v1.4.0 - Frontend Dashboard**       | Build a React-based real-time dashboard for data visualization.     |

---

## 📝 License

Distributed under the **Apache 2.0** License. Maintained by [GOERISSEN.DEV](https://goerissen.dev/).

---