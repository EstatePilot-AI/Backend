# Estate Pilot (Backend API Engine) 🏡🤖

[![Backend Framework](https://img.shields.io/badge/.NET_Core-10.0-512BD4?logo=.net&logoColor=white)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/PostgreSQL_/_Neon-Serverless-00E599?logo=neon&logoColor=black)](https://neon.tech/)
[![Hosting Provider](https://img.shields.io/badge/Hosting-SmarterASP.NET-orange?logo=serverfault&logoColor=white)](https://www.smarterasp.net/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean_Architecture-brightgreen)](#-architecture-patterns)

**Estate Pilot** is an enterprise-grade backend ecosystem engineered to drive an AI-powered autonomous voice agent specialized for the Egyptian real estate market. This system handles real-time natural language pipelines, property matching logic, and comprehensive customer relations workflows with minimal processing latency.

---

## 🌐 Live Production Deployment

The backend application and its supporting database infrastructure are fully deployed and accessible in the cloud.

* **API Dashboard (Swagger):** `https://estatepilot.runasp.net/index.html`
* **Database Engine:** Managed Serverless **Neon PostgreSQL** cluster.

### 📸 Production API Dashboard Preview
![Estate Pilot Swagger Dashboard](https://github.com/EstatePilot-AI/Backend/issues/66)

> **Note for Reviewers:** You can use the live Swagger link above to directly test the authentication pipelines, real estate database queries, and AI agent log endpoints live without needing to set up a local database environment.

---

## 🚀 Key Features

* **AI Voice Handler Gateway:** High-performance REST endpoints linking distributed voice network interactions directly into system processing layers.
* **Property Management Matrix:** Built-in advanced pagination, intricate search algorithms, and location filters optimized for rapid querying.
* **Lead Queuing Orchestration:** Automated server-side sorting queues that intelligently rank and route hot leads to assigned property consultants.
* **Robust Multi-Layer Security:** Secure data boundaries built around strict authorization protocols protecting system interactions.

---

## 🛠️ Technology Stack & Tooling

* **Framework:** ASP.NET Core (.NET 10 Web API)
* **Data Layer:** Entity Framework Core (EF Core) via Code-First Approach
* **Production Database:** Neon (Serverless PostgreSQL Cloud Database)
* **Real-time Pipeline:** SignalR Hub integration for async notification dispatching
* **DevOps:** Dockerized runtime environments (`Dockerfile` and compose configurations for local development)

---

## 🏗️ Architecture Patterns

The system implements strict **Clean Architecture / Onion Architecture** rules to guarantee complete decoupling of business rules from database systems or user-facing protocols:

```text
├── EstatePilot.Core / Domain          # Entities, Value objects, repository abstractions
├── EstatePilot.Application            # Business workflows, DTO structures, CQRS handlers
├── EstatePilot.Infrastructure         # DBContext, Identity Setup, External integrations (Neon Cloud)
└── EstatePilot.WebAPI                 # Controllers, Middleware filters, Swagger configurations

graph TD
    API[WebAPI Layer / SmarterASP.NET] --> Application[Application Layer]
    API --> Infrastructure[Infrastructure Layer]
    Application --> Core[Core Domain Entities]
    Infrastructure --> Core
    Infrastructure --> Neon[Neon Serverless PostgreSQL]