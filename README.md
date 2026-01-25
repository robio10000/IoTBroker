# IoTBroker 🚀
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/en-us/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

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

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* A running instance of PostgreSQL or MySQL (optional, SQLite works out-of-the-box)

#### 2. Configuration

IoTBroker supports multiple database providers. Configure your choice in `appsettings.json`:

```json
{
  "DatabaseProvider": "Postgres", // Options: "Postgres", "SQLite", "MySQL", "InMemory"
  "ConnectionStrings": {
    "SQLiteConnection": "Data Source=iotbroker.db",
    "PostgresConnection": "Host=localhost;Database=iot_broker_metadata;Username=broker_admin;Password=adminpassword",
    "MySqlConnection": "Server=localhost;Database=iot_broker_db;User=broker_user;Password=userpassword"
  }
}

```

#### 3. Database Migrations

Apply the migrations for your configured provider:

```bash
dotnet ef database update --context IoTContext --project IoTBroker

```

#### 4. Run the Application

```bash
dotnet run --project IoTBroker --launch-profile https

```

Once started, the interactive API documentation is available at: `https://localhost:7045/doc` (or your configured port).

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

This project is licensed under the MIT License.

---