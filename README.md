

# IoTBroker 🚀
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/en-us/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A modern, lightweight, multi-tenant IoT Broker built with **.NET 9**. This project provides a secure infrastructure to receive, manage, and isolate sensor data from various devices for different clients and users.

## ✨ Current Features

* **Multi-Tenancy:** Data is strictly isolated using a combined `clientId_deviceId` key strategy. Clients can only access and manage devices they are authorized for.
* **API Key Authentication:** Secure access control via custom `ApiKeyMiddleware`. Every request (excluding Health/Doc) requires a valid `X-API-KEY` header.
* **Role-Based Access Control (RBAC):** Granular permissions for `Admin` (full access, client/device management) and `SensorNode/User` roles.
* **Client Management:** Dedicated `ClientsController` to manage API identities, roles, and device ownership.
* **Powerful Rule Engine:** Execute complex logic based on sensor data with support for multiple conditions (**AND/OR**) and automated actions.
* **WebHooks & Automation:** Send real-time HTTP requests to external APIs or update other devices automatically.
* **Dynamic Token Replacement:** Use placeholders like `{value}` or `{device}` in WebHook URLs, Headers, and Payloads.
* **Interactive API Documentation:** Full Swagger/OpenAPI integration including security definitions for easy testing of API keys. Once running, you can explore the API at ``http://localhost:5048/doc``.
* **Thread-Safe Processing:** High-performance data handling using `ConcurrentDictionary` and granular locking for In-Memory consistency.
* **Automatic Device Onboarding:** Devices are dynamically assigned to the authenticated client's profile during data submission.

---

## 🛠 Tech Stack

* **Backend:** ASP.NET Core 9.0 (Web API)
* **Security:** Custom Middleware & API Key Authentication (X-API-KEY)
* **Communication:** HttpClientFactory for robust WebHook execution
* **Serialization:** System.Text.Json with Polymorphic Support
* **Documentation:** Swagger / Swashbuckle
* **Storage:** In-Memory Storage (ConcurrentDictionary) with Thread-Safe Processing

---

## 🚀 Getting Started

### 1. Run the Project

```bash
dotnet run --project IoTBroker

```

The API will be available at `http://localhost:5048` (or the port specified in your `launchSettings.json`).

### 2. Authentication

Include the following header in your requests:

* **Header Key:** `X-API-KEY`

(Pre-configured for development)

| Key Type | Default Value (Dev) | Header Key |
| --- | --- | --- |
| **Admin Key** | `admin-key-123` | `X-API-KEY` |
| **Client Key** | `client-key-123` | `X-API-KEY` |

### 3. Example: Submit Sensor Data

| Field| Type | Description |
| --- | --- |----|
| `deviceId` | `String` | Unique ID of the sensor |
| `type` | `Enum` | `Numeric`, `Boolean` or `String` |
| `value` | `String` | The raw value of the sensor |
| `timestamp` | `DateTime` | Optional. It wil be generated if missing (ISO 8601) |

**POST** `/api/Sensor`

```json
{
  "deviceId": "temp-sensor-01",
  "type": "Numeric",
  "value": "22.5",
  "timestamp": "2024-03-21T10:00:00Z"
}

```

---

## ⚙️ Rule Engine & Automation

The Rule Engine allows you to define automated workflows. A rule consists of **Conditions** (when to trigger) and **Actions** (what to do).

### 📋 Rule Definition Reference

| Field | Type | Description |
| --- | --- | --- |
| `name` | `String` | Unique name of the rule for identification. |
| `logicalOperator` | `Enum` | **`All`** (AND): All conditions must be met. <br><br> **`Any`** (OR): At least one condition must be met. |
| `conditions` | `Array` | List of conditions to be evaluated. |
| `actions` | `Array` | List of actions to execute when the rule triggers. |
| `isActive` | `Boolean` | Enables or disables the rule. |

#### Condition Fields

| Field | Description |
| --- | --- |
| `deviceId` | The ID of the device whose value is being monitored. |
| `operator` | Comparison operator: `Equals`, `NotEquals`, `GreaterThan`, `LessThan`. |
| `thresholdValue` | The reference value for the comparison (as a string). |
| `ignoreCase` | (Optional) Boolean to ignore casing for string comparisons. |

#### Action Fields by `$type`

| Action Type | Required Fields | Description |
| --- | --- | --- |
| **`webhook`** | `url`, `method` | Sends an HTTP request. Supports optional `headers` and `payloadTemplate`. |
| **`set_value`** | `targetDeviceId`, `valueType`, `newValue` | Updates another device's state internally. |

---

### 🔍 Supported Replacer Tokens

Use these tokens in WebHook URLs, Payloads, Headers, or `SetDeviceValue` targets to inject dynamic data:

| Token | Description | Example Output |
| --- | --- | --- |
| `{device}` | ID of the triggering device | `temp-sensor-01` |
| `{value}` | Current sensor value | `22.5` |
| `{value.type}` | Data type of the value | `Numeric` |
| `{rule.name}` | Name of the triggered rule | `Overheat Alert` |
| `{rule.conditions}` | Summary of the logic setup | `TempSensor GreaterThan 20` |
| `{timestamp}` | ISO 8601 Execution Timestamp | `2026-01-04T22:15:00Z` |

---
### 🚀 Example: Create a WebHook Rule (POST `/api/Rules`)

This rule triggers a WebHook when a "TempSensor" reports a value greater than 20. It includes dynamic tokens in the URL, Headers, and Payload.

```json
{
  "name": "High Temp WebHook",
  "logicalOperator": "All",
  "conditions": [
    {
      "deviceId": "TempSensor",
      "operator": "GreaterThan",
      "thresholdValue": "20"
    }
  ],
  "actions": [
    {
      "$type": "webhook",
      "url": "https://webhook.site/your-id?src={device}",
      "method": "POST",
      "headers": {
        "Authorization": "Bearer MySecretToken",
        "X-Sensor-Type": "{value.type}"
      },
      "payloadTemplate": "{ \"msg\": \"Alert for {device}! Current value: {value}\" }"
    }
  ],
  "isActive": true
}

```

### 🔄 Example: Internal Device Update (POST `/api/Rules`)

This rule automatically turns on a "Heater" device when a "TempSensor" reports a value below 18. It uses the token replacer to include the original value in the status string.

```json
{
  "name": "Auto-Heater-On",
  "logicalOperator": "All",
  "conditions": [
    {
      "deviceId": "TempSensor",
      "operator": "LessThan",
      "thresholdValue": "18"
    }
  ],
  "actions": [
    {
      "$type": "set_value",
      "targetDeviceId": "LivingRoomHeater",
      "valueType": "String",
      "newValue": "ON (Triggered by {device} at {value}°C)"
    }
  ],
  "isActive": true
}

```

---

## 🗺 Roadmap

This project is under active development. Our upcoming milestones include:

1. 🚧**Data Persistence:** Migrating In-Memory storage to a persistent database using **EF Core** (supporting SQLite, MySQL, and PostgreSQL).
2. 📅**Unit & Integration Testing:** Comprehensive test suite to ensure business logic and client isolation stability.
3. ⚙️**DevOps Enhancements:** Dockerization and CI/CD pipeline setup.
4. 📅**Frontend:** Building a modern real-time dashboard with **React** for data visualization.

---

## 📝 License

This project is licensed under the MIT License.

---