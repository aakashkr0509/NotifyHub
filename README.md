# NotifyHub

> Real-time multi-tenant notification system built with .NET 8, Angular 15+, SignalR, Redis, Dapper, and PostgreSQL.

---

## Table of contents

- [What is NotifyHub?](#what-is-notifyhub)
- [Why this architecture?](#why-this-architecture)
- [Feature overview](#feature-overview)
- [System architecture](#system-architecture)
- [Tech stack](#tech-stack)
- [Project structure](#project-structure)
- [Domain model](#domain-model)
- [Database schema](#database-schema)
- [Authentication and multi-tenancy](#authentication-and-multi-tenancy)
- [Real-time flow](#real-time-flow)
- [API endpoints](#api-endpoints)
- [SignalR hub](#signalr-hub)
- [Redis usage](#redis-usage)
- [Angular frontend](#angular-frontend)
- [Background service](#background-service)
- [Getting started](#getting-started)
- [Environment variables](#environment-variables)
- [Interview talking points](#interview-talking-points)

---

## What is NotifyHub?

NotifyHub is a production-grade, multi-tenant notification platform that delivers real-time alerts to users across isolated tenant organisations. Each tenant — think of a company or enterprise client — operates in complete data isolation. A notification sent to Tenant A is never visible to Tenant B, even though they share the same infrastructure.

Users within a tenant receive in-app notifications instantly via a persistent WebSocket connection managed by SignalR. Notifications are stored in PostgreSQL via Dapper, tenant configs are cached in Redis to avoid repeated database hits, and a background worker processes outbound notification jobs from an in-memory queue.

This project demonstrates the kind of architecture commonly found in enterprise SaaS products: multi-tenancy, real-time event delivery, token-based auth with role-based access, and a clean separation of concerns across Domain, Application, Infrastructure, and API layers.

---

## Why this architecture?

| Decision | Reason |
|---|---|
| Dapper over EF Core | Raw SQL gives full control over query shape and performance. Dapper is widely used in high-throughput SaaS backends where EF Core's abstraction becomes a liability. |
| SignalR over polling | Polling wastes server resources and adds latency. SignalR maintains a persistent WebSocket, falling back to Server-Sent Events or long polling automatically. |
| Redis for caching | Tenant config (connection strings, feature flags) is read on every request. Redis keeps this sub-millisecond without hammering PostgreSQL. |
| Redis as SignalR backplane | When the API scales to multiple instances, SignalR needs a shared message bus so any instance can push to any connected client. Redis provides this out of the box. |
| Clean Architecture layers | Domain and Application have zero framework dependencies. This keeps business logic testable in isolation and makes the Infrastructure layer swappable. |
| Tenant isolation via JWT claims | The tenant ID comes from the server-issued JWT, never from a request header the client controls. This is the only secure model. |

---

## Feature overview

### Core features

- **Real-time notifications** — users receive in-app alerts instantly without refreshing the page
- **Multi-tenant isolation** — each tenant's notifications are scoped by `tenant_id`; no cross-tenant data leakage is possible
- **JWT authentication** — stateless token-based auth with access token and refresh token flow
- **Role-based access control (RBAC)** — `Admin`, `Manager`, and `Viewer` roles per tenant with policy-based enforcement
- **Notification CRUD** — create, list, mark as read, mark all as read, delete
- **Unread badge count** — Angular component shows live unread count updated in real time
- **Background queue worker** — `IHostedService` processes notification dispatch jobs asynchronously
- **Tenant config caching** — Redis caches tenant settings with a configurable TTL to reduce database load
- **Automatic reconnection** — Angular SignalR client reconnects with exponential backoff on disconnect
- **Tenant group re-join** — on reconnect, the client automatically re-joins the correct tenant SignalR group

### Stretch features (planned)

- Email digest via background service
- Notification categories and filtering
- Read receipts with timestamps
- Admin dashboard showing per-tenant notification volume

---

## System architecture

```
┌─────────────────────────────────────────────────────┐
│                   Angular 15+ SPA                    │
│  NotificationBellComponent  │  AuthInterceptor       │
│  SignalR Client (WS)        │  HTTP Client           │
└────────────┬────────────────────────────┬────────────┘
             │ WebSocket                  │ HTTP/REST
             ▼                            ▼
┌─────────────────────────────────────────────────────┐
│              ASP.NET Core Web API (.NET 8)           │
│  JWT Middleware  │  RBAC Filters  │  Controllers     │
│  NotificationHub (SignalR)                           │
│  IHostedService (Background Worker)                  │
└──────┬─────────────────────────────┬────────────────┘
       │                             │
       ▼                             ▼
┌─────────────┐             ┌────────────────┐
│    Redis    │             │   PostgreSQL   │
│  Tenant     │             │  Tenants       │
│  config     │             │  Users         │
│  cache      │             │  Notifications │
│  SignalR    │             │  RefreshTokens │
│  backplane  │             └────────────────┘
└─────────────┘
```

---

## Tech stack

| Layer | Technology | Version |
|---|---|---|
| Backend framework | ASP.NET Core Web API | .NET 8 |
| ORM / data access | Dapper | 2.x |
| Database | PostgreSQL | 15 |
| Cache + backplane | Redis (StackExchange.Redis) | 7 |
| Real-time | SignalR (ASP.NET Core) | built-in |
| Auth | JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer) | .NET 8 |
| Frontend | Angular | 15+ |
| SignalR client | @microsoft/signalr | 7.x |
| Containerisation | Docker + docker-compose | — |
| Architecture pattern | Clean Architecture | — |

---

## Project structure

```
NotifyHub/
│
├── NotifyHub.Domain/                  # Pure C# entities, enums, no dependencies
│   ├── Entities/
│   │   ├── Tenant.cs
│   │   ├── AppUser.cs
│   │   └── Notification.cs
│   └── Enums/
│       ├── NotificationStatus.cs
│       └── UserRole.cs
│
├── NotifyHub.Application/             # Business logic, interfaces, DTOs
│   ├── Interfaces/
│   │   ├── INotificationRepository.cs
│   │   ├── IUserRepository.cs
│   │   ├── ITenantRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── DTOs/
│   │   ├── NotificationDto.cs
│   │   ├── CreateNotificationRequest.cs
│   │   └── LoginRequest.cs
│   └── Services/
│       └── NotificationService.cs
│
├── NotifyHub.Infrastructure/          # Dapper repos, Redis, PostgreSQL
│   ├── Persistence/
│   │   ├── DapperContext.cs
│   │   ├── UnitOfWork.cs
│   │   └── Repositories/
│   │       ├── NotificationRepository.cs
│   │       ├── UserRepository.cs
│   │       └── TenantRepository.cs
│   ├── Cache/
│   │   └── RedisTenantCache.cs
│   └── DependencyInjection.cs
│
├── NotifyHub.API/                     # Controllers, hub, middleware, program
│   ├── Controllers/
│   │   ├── NotificationsController.cs
│   │   └── AuthController.cs
│   ├── Hubs/
│   │   └── NotificationHub.cs
│   ├── Middleware/
│   │   └── TenantMiddleware.cs
│   ├── BackgroundServices/
│   │   └── NotificationWorker.cs
│   ├── Program.cs
│   └── appsettings.json
│
└── NotifyHub.Web/                     # Angular frontend
    └── src/
        ├── app/
        │   ├── core/
        │   │   ├── services/
        │   │   │   ├── auth.service.ts
        │   │   │   └── notification-signalr.service.ts
        │   │   └── interceptors/
        │   │       └── auth.interceptor.ts
        │   └── features/
        │       └── notifications/
        │           └── notification-bell.component.ts
        └── environments/
            └── environment.ts
```

---

## Domain model

### Tenant

Represents an isolated organisation on the platform.

```
Tenant
├── Id           : Guid
├── Name         : string
├── Subdomain    : string   (unique — used for routing)
├── IsActive     : bool
└── CreatedAt    : DateTime
```

### AppUser

A user belonging to exactly one tenant.

```
AppUser
├── Id           : Guid
├── TenantId     : Guid     (FK → Tenant)
├── Email        : string
├── PasswordHash : string
├── Role         : UserRole  (Admin | Manager | Viewer)
└── CreatedAt    : DateTime
```

### Notification

A notification scoped to a tenant, optionally targeted at a specific user.

```
Notification
├── Id           : Guid
├── TenantId     : Guid     (FK → Tenant)
├── UserId       : Guid?    (null = broadcast to all tenant users)
├── Title        : string
├── Body         : string
├── Status       : NotificationStatus  (Unread | Read)
└── CreatedAt    : DateTime
```

---

## Database schema

```sql
CREATE TABLE tenants (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(200) NOT NULL,
    subdomain   VARCHAR(100) NOT NULL UNIQUE,
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE app_users (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id     UUID NOT NULL REFERENCES tenants(id),
    email         VARCHAR(255) NOT NULL,
    password_hash TEXT NOT NULL,
    role          VARCHAR(50) NOT NULL DEFAULT 'Viewer',
    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(tenant_id, email)
);

CREATE TABLE notifications (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   UUID NOT NULL REFERENCES tenants(id),
    user_id     UUID REFERENCES app_users(id),
    title       VARCHAR(300) NOT NULL,
    body        TEXT NOT NULL,
    status      VARCHAR(20) NOT NULL DEFAULT 'Unread',
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE refresh_tokens (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID NOT NULL REFERENCES app_users(id),
    token       TEXT NOT NULL UNIQUE,
    expires_at  TIMESTAMPTZ NOT NULL,
    is_revoked  BOOLEAN NOT NULL DEFAULT FALSE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Indexes for common query patterns
CREATE INDEX idx_notifications_tenant_id ON notifications(tenant_id);
CREATE INDEX idx_notifications_user_id   ON notifications(user_id);
CREATE INDEX idx_notifications_status    ON notifications(status);
CREATE INDEX idx_app_users_tenant_id     ON app_users(tenant_id);
```

---

## Authentication and multi-tenancy

### How JWT carries tenant identity

When a user logs in, the server issues a JWT containing these claims:

```json
{
  "sub": "user-guid",
  "email": "user@acme.com",
  "tenant_id": "tenant-guid",
  "role": "Admin",
  "exp": 1234567890
}
```

The `tenant_id` claim is extracted in `TenantMiddleware` and stored in `HttpContext.Items["TenantId"]`. Every downstream service reads the tenant ID from there — never from a request header or query string the client can forge.

### Refresh token flow

1. Client logs in → receives `access_token` (15 min TTL) + `refresh_token` (7 day TTL)
2. Access token expires → client calls `POST /api/auth/refresh` with the refresh token
3. Server validates, issues a new access token, rotates the refresh token
4. Old refresh token is marked as revoked in the database

### RBAC policy example

```csharp
// Only Admins can create notifications for all users in a tenant
[Authorize(Policy = "AdminOnly")]
[HttpPost]
public async Task<IActionResult> Create(CreateNotificationRequest request) { ... }
```

---

## Real-time flow

This is the end-to-end sequence from a notification being created to it appearing in a user's browser:

```
1.  Admin calls POST /api/notifications
2.  Controller validates JWT + tenant claim
3.  NotificationService saves to PostgreSQL via Dapper
4.  NotificationService enqueues a dispatch job to the background worker
5.  Background worker dequeues the job
6.  Worker calls IHubContext.Clients.Group("tenant_{tenantId}")
              .SendAsync("ReceiveNotification", notificationDto)
7.  SignalR pushes the message over WebSocket to all connected clients in that group
8.  Angular SignalR client receives the message via connection.on("ReceiveNotification")
9.  BehaviorSubject emits the new notification
10. NotificationBellComponent updates the unread badge count
```

### Tenant group isolation

Every connected client joins a group named `tenant_{tenantId}` after authentication. The group name is derived from the JWT claim on the server, not from anything the client sends:

```csharp
public async Task JoinTenantGroup()
{
    var tenantId = Context.User?.FindFirst("tenant_id")?.Value;
    await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
}
```

This means even if a malicious client tries to join a different tenant's group, the server ignores it — the group assignment comes exclusively from the verified JWT.

---

## API endpoints

### Auth

| Method | Endpoint | Description | Auth required |
|---|---|---|---|
| POST | `/api/auth/login` | Login, returns JWT + refresh token | No |
| POST | `/api/auth/refresh` | Rotate refresh token | No |
| POST | `/api/auth/logout` | Revoke refresh token | Yes |

### Notifications

| Method | Endpoint | Description | Role required |
|---|---|---|---|
| GET | `/api/notifications` | List notifications for current user | Viewer+ |
| GET | `/api/notifications/unread-count` | Get unread count | Viewer+ |
| POST | `/api/notifications` | Create a notification | Admin |
| PATCH | `/api/notifications/{id}/read` | Mark one as read | Viewer+ |
| PATCH | `/api/notifications/read-all` | Mark all as read | Viewer+ |
| DELETE | `/api/notifications/{id}` | Delete a notification | Admin |

All notification endpoints are automatically scoped to the tenant extracted from the JWT. There is no way to access another tenant's notifications.

---

## SignalR hub

**Hub URL:** `wss://localhost:5001/hubs/notifications`

### Client-callable methods (server receives)

| Method | Description |
|---|---|
| `JoinTenantGroup` | Adds the connection to the caller's tenant group |
| `LeaveGroup` | Removes the connection from the group (called on logout) |

### Server-to-client events (client listens for)

| Event | Payload | Description |
|---|---|---|
| `ReceiveNotification` | `NotificationDto` | A new notification was created for this tenant |
| `NotificationRead` | `{ id: string }` | A notification was marked read (useful for multi-tab sync) |

### Reconnection behaviour

The Angular client is configured with:

```typescript
.withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
```

After reconnection, `JoinTenantGroup` is called again because the server assigns a new `ConnectionId` on every connection — the previous group membership is gone.

---

## Redis usage

### 1. Tenant config cache

Tenant settings are read on every authenticated request. Instead of hitting PostgreSQL each time:

```csharp
// Cache key: "tenant:{tenantId}:config"
// TTL: 10 minutes
var tenant = await _cache.GetOrSetAsync(
    $"tenant:{tenantId}:config",
    () => _tenantRepository.GetByIdAsync(tenantId),
    TimeSpan.FromMinutes(10)
);
```

### 2. SignalR backplane

When the API runs as multiple instances (horizontal scaling), each instance manages its own set of WebSocket connections. Without a backplane, Instance A cannot send a message to a client connected to Instance B.

Redis solves this: all instances subscribe to the same Redis pub/sub channel. When any instance calls `SendAsync`, Redis fans it out to all other instances which then push to their local connections.

```csharp
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnectionString);
```

This one line is what makes SignalR work correctly in a load-balanced environment.

### 3. Refresh token store

Refresh tokens are stored in both PostgreSQL (for persistence and auditing) and Redis (for fast revocation lookups with TTL-based expiry).

---

## Angular frontend

### Key services

| Service | Responsibility |
|---|---|
| `AuthService` | Stores and retrieves JWT and tenant ID from localStorage |
| `NotificationSignalrService` | Manages hub connection lifecycle, exposes `notifications$` observable |
| `AuthInterceptor` | Attaches `Authorization` and `X-Tenant-Id` headers to all HTTP requests |

### State management

Notifications are held in a `BehaviorSubject<NotificationDto[]>` inside `NotificationSignalrService`. Components subscribe to the `notifications$` observable. No NgRx or external state library is needed for this scale.

### Connection lifecycle

```
App init
  → AuthInterceptor registered
  → NotificationBellComponent.ngOnInit()
  → signalrService.startConnection()
  → HubConnectionBuilder configured with accessTokenFactory
  → connection.start() → WebSocket negotiation
  → connection.invoke("JoinTenantGroup")
  → connection.on("ReceiveNotification") active
  → [disconnect event]
  → withAutomaticReconnect kicks in (0s, 2s, 5s, 10s, 30s)
  → onreconnected → JoinTenantGroup called again
```

---

## Background service

`NotificationWorker` is an `IHostedService` that runs for the lifetime of the application. It processes notification dispatch jobs from a `Channel<NotificationJob>` (an in-memory async queue).

```
POST /api/notifications
  → Controller saves to DB
  → Enqueues job: Channel<NotificationJob>.Writer.WriteAsync(job)

NotificationWorker (running in background)
  → Channel<NotificationJob>.Reader.ReadAsync()
  → Calls IHubContext to push via SignalR
  → Logs dispatch result
```

Using `Channel<T>` instead of a third-party queue (like Kafka or RabbitMQ) keeps the implementation self-contained. In a production system you would replace this with Kafka or Azure Service Bus for durability and horizontal scaling.

---

## Getting started

### Prerequisites

- .NET 8 SDK
- Node.js 18+
- Docker Desktop

### 1. Clone and run infrastructure

```bash
git clone https://github.com/yourusername/NotifyHub.git
cd NotifyHub
docker-compose up -d
```

This starts PostgreSQL on port 5432 and Redis on port 6379.

### 2. Run database migrations

```bash
cd NotifyHub.API
dotnet run --migrate
```

Or apply the SQL scripts in `/scripts/migrations/` manually via Azure Data Studio or psql.

### 3. Start the API

```bash
cd NotifyHub.API
dotnet run
```

API runs at `https://localhost:5001`.

### 4. Start the Angular app

```bash
cd NotifyHub.Web
npm install
ng serve
```

App runs at `http://localhost:4200`.

---

## Environment variables

### `appsettings.json` (API)

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=notifyhub_db;Username=notifyhub;Password=notifyhub123",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-minimum-32-chars",
    "Issuer": "NotifyHub",
    "Audience": "NotifyHubUsers",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

### `environment.ts` (Angular)

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api',
  hubUrl: 'https://localhost:5001/hubs/notifications'
};
```

---

## Interview talking points

These are the architectural decisions you should be able to speak to confidently in any technical interview:

**On multi-tenancy:**
> "Tenant isolation is enforced at the JWT claim level on the server. The `tenant_id` is embedded in the token at login and extracted by middleware on every request. SignalR group names are derived from this claim, so a client cannot join another tenant's group regardless of what it sends."

**On Dapper vs EF Core:**
> "I chose Dapper because notification queries have specific shapes — filtered by tenant, ordered by date, with unread counts. Dapper gives me full control over the SQL. I use the Unit of Work pattern to wrap multiple Dapper operations in a single transaction."

**On SignalR scaling:**
> "Adding the Redis backplane with one line means the system can scale horizontally. Any API instance can push to any connected client because Redis acts as the shared message bus between instances."

**On the background worker:**
> "The background service uses `System.Threading.Channels` as an in-memory queue. The controller writes jobs to the channel asynchronously without blocking the HTTP response. The worker reads and dispatches in the background. For production I'd replace the channel with Kafka for durability."

**On Redis caching:**
> "Tenant config is read on every authenticated request. Caching it in Redis with a 10-minute TTL eliminates repeated database round-trips. The cache is invalidated explicitly when tenant settings change."

---

*Built as a portfolio project to demonstrate multi-tenant SaaS architecture, real-time systems, and clean layered design in .NET 8 and Angular.*
