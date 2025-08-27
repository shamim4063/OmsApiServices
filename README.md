
<div align="center">
   <h1>OmsApiServices Monorepo</h1>
   <p><b>ERP Microservices Backend</b> — <code>.NET 9</code> · <code>PostgreSQL</code> · <code>Docker</code> · <code>YARP</code> · <code>MassTransit</code> · <code>K8s</code></p>
   <p>
      <img src="https://img.shields.io/badge/.NET-9.0-blueviolet?logo=dotnet" />
      <img src="https://img.shields.io/badge/PostgreSQL-16-blue?logo=postgresql" />
      <img src="https://img.shields.io/badge/Docker-Compose-blue?logo=docker" />
      <img src="https://img.shields.io/badge/YARP-ReverseProxy-green" />
      <img src="https://img.shields.io/badge/MassTransit-RabbitMQ-orange" />
      <img src="https://img.shields.io/badge/Orchestration-K8s-green" />
   </p>
</div>

---



## 📝 Overview
This repository contains a modular, production-grade ERP backend built with .NET 9, following Clean Architecture and microservices best practices. Each bounded context (Catalog, Procurement, etc.) is implemented as an independent service with its own API, Application, Domain, and Infrastructure projects.

- **Tech Stack:** .NET 9, ASP.NET Core, PostgreSQL, EF Core 9, MassTransit (RabbitMQ), YARP (API Gateway), Docker
- **Architecture:** Clean Architecture, CQRS, DDD, Outbox Pattern, Database-per-Service


---


## 🗂️ Solution Structure

```
OmsApiServices.sln
├─ Src/
│  ├─ BuildingBlocks/
│  │  ├─ Persistence/
│  │  ├─ Messaging/
│  │  └─ Web/
│  ├─ Services/
│  │  ├─ Catalog/
│  │  │  ├─ Catalog.Api/
│  │  │  ├─ Catalog.Application/
│  │  │  ├─ Catalog.Domain/
│  │  │  └─ Catalog.Infrastructure/
│  │  └─ ... (other services: procurement, inventory, etc.)
│  └─ contracts/ (OpenAPI specs, DTOs)
├─ Deploy/
│  └─ compose/
│     ├─ infra.yml
│     └─ dev.yml
└─ Tests/
```


---


## ⚙️ Key Principles
- **Microservice per bounded context**: Each service is fully isolated.
- **Database-per-service**: One Postgres instance, each service owns its schema and migrations.
- **No shared domain code**: Only contracts/utilities in `BuildingBlocks`.
- **Independently deployable**: Each service can be built and run on its own.


---


## 🏗️ Project Responsibilities
- **Api**: HTTP endpoints, DI, health checks, Swagger, logging, YARP gateway.
- **Application**: CQRS handlers, validation, orchestration, MediatR.
- **Domain**: Entities, value objects, domain services, business rules.
- **Infrastructure**: EF Core DbContext, migrations, repositories, outbox, messaging.
- **BuildingBlocks**: Shared technical utilities (never domain logic).


---


## 🚀 Local Development

### 1. Start infrastructure
   ```powershell
   docker compose -f infra.yml up -d
   ```
   Open PgAdmin in the browser: [http://localhost:8081](http://localhost:8081)
   - Host: <code>postgres</code> | Username: <code>omsadmin</code> | Database: <code>postgres</code> | Password: <code>123qwe</code>
   - Create databases with owner <code>omsadmin</code>: <code>catalog_db</code>, <code>procurement_db</code>

   <br/>
   <b>Apply Migration:</b>
   <br/>
      
   ```powershell
   dotnet ef database update  list -p .\Catalog.Infrastructure -s .\Catalog.Api\
   dotnet ef database update  list -p .\Procurement.Infrastructure -s .\Procurement.Api\
   ```

   <sub>Tip: If migration fails initially, run the commands again after DB is ready.</sub>
   
### 2. Run Catalog service
   ```powershell
   dotnet watch --project Catalog.Api
   ```
### 3. Run Procurement service
   ```powershell
   dotnet watch --project Procurement.Api
   ```
### 4. Run API Gateway
   ```powershell
   dotnet run --project Gateway.Api
   ```

### 5. Add a migration when needed
   ```powershell
   dotnet ef migrations add  list -p .\Procurement.Infrastructure -s .\Procurement.Api\
   dotnet ef database update  list -p .\Procurement.Infrastructure -s .\Procurement.Api\
   ```
### 6. Access Swagger UI
   - [Catalog Swagger](https://localhost:5101/swagger/index.html)
   - [Procurement Swagger](http://localhost:5102/swagger/index.html)

## 🖥️ Running with Visual Studio (Multiple Startup Projects)

You can run the API Gateway and multiple microservices at once using Visual Studio:

1. **Right-click the solution** in Solution Explorer and select <b>Set Startup Projects…</b>
2. Choose <b>Multiple startup projects</b>.
3. For each project you want to run (e.g., <code>Gateway.Api</code>, <code>Catalog.Api</code>, <code>Procurement.Api</code>), set the <b>Action</b> to <b>Start</b>.
4. Click <b>OK</b>.
5. Press <kbd>F5</kbd> or click <b>Start</b> to launch all selected services together.

<details>
<summary><b>Screenshot Example</b> (click to expand)</summary>

<img src="https://learn.microsoft.com/en-us/visualstudio/ide/media/vs-2022/solution-properties-dialog.png?view=vs-2017&viewFallbackFrom=vs-2022" alt="Visual Studio Multiple Startup Projects" width="600"/>

</details>

---



## 📦 Catalog Service API

- **Direct service endpoints:**
   - List products: `GET http://localhost:5101/v1/products`
   - Product detail: `GET http://localhost:5101/v1/products/{id}`
   - List categories: `GET http://localhost:5101/v1/categories`
   - Category detail: `GET http://localhost:5101/v1/categories/{id}`

- **Via API Gateway:**
   - List products: `GET http://localhost:5000/api/catalog/products`
   - Product detail: `GET http://localhost:5000/api/catalog/products/{id}`
   - List categories: `GET http://localhost:5000/api/catalog/categories`
   - Category detail: `GET http://localhost:5000/api/catalog/categories/{id}`

> The API Gateway (<code>Gateway.Api</code>) uses YARP to route <code>/api/catalog/{**catch-all}</code> to <code>/v1/{**catch-all}</code> on the Catalog service. See <code>Gateway.Api/appsettings.json</code> for details.


---


## 🏢 Procurement Service API

- **Direct service endpoints:**
   - List suppliers: `GET http://localhost:5102/v1/suppliers`
   - Supplier detail: `GET http://localhost:5102/v1/suppliers/{id}`
   - List supplier products: `GET http://localhost:5102/v1/supplier-products`
   - Supplier product detail: `GET http://localhost:5102/v1/supplier-products/{supplierId}/{productId}`

- **Via API Gateway:**
   - List suppliers: `GET http://localhost:5000/api/procurement/suppliers`
   - Supplier detail: `GET http://localhost:5000/api/procurement/suppliers/{id}`
   - List supplier products: `GET http://localhost:5000/api/procurement/supplier-products`
   - Supplier product detail: `GET http://localhost:5000/api/procurement/supplier-products/{supplierId}/{productId}`

> The API Gateway (<code>Gateway.Api</code>) uses YARP to route <code>/api/procurement/{**catch-all}</code> to <code>/v1/{**catch-all}</code> on the Procurement service. See <code>Gateway.Api/appsettings.json</code> for details.


---


## 🌐 API Gateway (YARP)
- All external traffic goes through the Gateway (`Gateway.Api`).
- Routes like `/api/catalog/...` or `/api/procurement/...` are mapped to internal service endpoints.
- See `Gateway.Api/appsettings.json` for route config.


---


## 🤝 Contributing
- Follow Clean Architecture and DDD boundaries.
- Do not share domain code between services.
- Add new features in the appropriate layer and project.
- See `copilot-instructions.md` and `Architecture.md` for detailed guidelines.
