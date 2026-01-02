

# IoTBroker 🚀
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/en-us/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A modern, lightweight, multi-tenant IoT Broker built with **.NET 9**. This project provides a secure infrastructure to receive, manage, and isolate sensor data from various devices for different clients and users.

## ✨ Current Features

* **Multi-Tenancy:** Data is strictly isolated using a combined `clientId_deviceId` key strategy. Clients can only access and manage devices they are authorized for.
* **API Key Authentication:** Secure access control via custom `ApiKeyMiddleware`. Every request (excluding Health/Doc) requires a valid `X-API-KEY` header.
* **Role-Based Access Control (RBAC):** Granular permissions for `Admin` (full access, client/device management) and `SensorNode/User` roles.
* **Client Management:** Dedicated `ClientsController` to manage API identities, roles, and device ownership.
* **Interactive API Documentation:** Full Swagger/OpenAPI integration including security definitions for easy testing of API keys. Once running, you can explore the API at ``http://localhost:5048/doc``.
* **Thread-Safe Processing:** High-performance data handling using `ConcurrentDictionary` and granular locking for In-Memory consistency.
* **Automatic Device Onboarding:** Devices are dynamically assigned to the authenticated client's profile during data submission.

---

## 🛠 Tech Stack

* **Backend:** ASP.NET Core 9.0 (Web API)
* **Security:** Custom Middleware & API Key Authentication
* **Documentation:** Swagger / Swashbuckle
* **Storage:** In-Memory Storage (ConcurrentDictionary)

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
* **Default Admin Key:** `admin-key-123` (Pre-configured for development)
* **Default Client Key:** `client-key-123`

### 3. Example: Submit Sensor Data

**POST** `/api/Sensor`

```json
{
  "deviceId": "temp-sensor-01",
  "type": "Numeric", // Numeric, Boolean, String
  "value": "22.5", // Raw value as string
  "timestamp": "2024-03-21T10:00:00Z" // ISO 8601 format (optional -> it will generated if missing)
}

```

---

## 🗺 Roadmap

This project is under active development. Our upcoming milestones include:

1. **Rule Engine:** Implementing a logic layer to trigger automated actions based on sensor thresholds.
2. **Unit & Integration Testing:** Comprehensive test suite to ensure business logic and client isolation stability.
3. **Data Persistence:** Migrating In-Memory storage to a persistent database using **EF Core** (supporting SQLite, MySQL, and PostgreSQL).
4. **Frontend:** Building a modern real-time dashboard with **React** for data visualization.

---

## 📝 License

This project is licensed under the MIT License.

---