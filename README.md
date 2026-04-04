# DeviceTrackr

Web application for **tracking company-owned mobile devices**: technical details, user **role/location**, and **who is currently using** each device (assignment).

## Tech stack

| Layer | Technology |
|--------|------------|
| Database | Microsoft SQL Server |
| API | ASP.NET Core Web API (.NET 8), Entity Framework Core |
| Frontend | Angular 17 (standalone components, routing) |
| Version control | Git |

## Repository layout

```
DeviceTrackr/
├── database/          # Idempotent SQL scripts (schema + demo data)
├── backend/
│   └── DeviceTrackr.Api/   # REST API
└── frontend/
    └── devicetrackr-web/   # Angular app
```

---

## What the application does

### 1. Database

- **`database/01-create-database-and-tables.sql`** – Creates the **DeviceTrackr** database (if missing), **Users** and **Devices** tables, unique index on user email, and foreign key from device to assigned user (`AssignedUserId`).
- **`database/02-seed-data.sql`** – Inserts demo users and devices (MERGE; safe to run multiple times).

**Users:** name, email, role, location, password hash.  
**Devices:** name, manufacturer, type (phone/tablet), OS, OS version, processor, RAM, description, optional assigned user.

### 2. API (backend)

Runs by default on **`http://localhost:5035`** (see `Properties/launchSettings.json`). Swagger is enabled in Development.

**Authentication (no cookie/JWT in the browser—session is kept in the frontend after login):**

| Method | Route | Purpose |
|--------|-------|---------|
| POST | `/api/auth/register` | New account: email, password, role, location; optional display name |
| POST | `/api/auth/login` | Login with email and password |

Response shape: `userId`, `name`, `email`. Passwords are stored as **SHA256 + Base64** (hashed, not plain text).

**Devices:**

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/api/devices` | List (includes assigned user when present) |
| GET | `/api/devices/{id}` | Details |
| POST | `/api/devices` | Create |
| PUT | `/api/devices/{id}` | Update **only if the device is unassigned** |
| DELETE | `/api/devices/{id}` | Delete **only if the device is unassigned** |
| POST | `/api/devices/{id}/assign` | Assign to user (body: `{ "userId": n }`) |
| POST | `/api/devices/{id}/unassign` | Unassign (only by the user who holds the assignment) |

**PUT** or **DELETE** on an **assigned** device returns **400** with an explanatory message.

**Users:** Standard CRUD on `/api/users` (used by the UI for lists; password hash is not exposed in JSON).

**CORS:** Allowed for the Angular dev origin (`http://localhost:4200`).

### 3. Frontend (Angular)

Dark UI theme.

**Routes:**

| Path | Content |
|------|---------|
| `/login` | Sign in; link to registration |
| `/register` | Form: email, password, role, location, optional name |
| `/devices` | Device dashboard (requires in-memory “session” after login/register) |

**Guards:** If you are not “logged in” in memory, you are redirected to login; if you are logged in, login/register send you to devices.

**On the devices page you can:**

- View the list showing **who uses** each device (or “Unassigned”).
- Open **details** for a row.
- **Add** new devices (field validation; unique name).
- **Edit** and **delete** only **unassigned** devices (buttons shown accordingly; backend enforces the same rule).
- **Assign to me** when signed in and the device is free.
- **Unassign** only for devices assigned to you.

The API base URL is set in `device-api.service.ts` (`http://localhost:5035/api`). If you change the API port, update it there as well.

---

## Running locally

### 1. SQL Server

Create the schema, then seed:

1. Run `database/01-create-database-and-tables.sql`
2. Run `database/02-seed-data.sql`

### 2. Local secrets — **dotnet user-secrets** (recommended)

Keep **passwords and API keys out of Git**. The API project already has a `UserSecretsId` in `DeviceTrackr.Api.csproj`; in **Development**, ASP.NET Core loads user secrets **after** `appsettings.json` and **overrides** those values.

Open a terminal in the API folder (from the repo root):

```powershell
cd backend\DeviceTrackr.Api
```

The `.csproj` already defines a **UserSecretsId**, so you can skip `dotnet user-secrets init` unless the CLI tells you to initialize.

**Gemini API key** (optional, for AI-generated descriptions) — from [Google AI Studio](https://aistudio.google.com/apikey):

```powershell
dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY"
```

**Optional — Gemini model:**

```powershell
dotnet user-secrets set "Gemini:Model" "gemini-2.5-flash"
```

**Verify secrets are registered:**

```powershell
dotnet user-secrets list
```

`appsettings.json` can stay with placeholders (`CHANGE_ME`, empty `Gemini:ApiKey`); **user-secrets win at runtime** when `ASPNETCORE_ENVIRONMENT` is `Development` (default for `dotnet run` with the included launch profile).

**Alternative (no user-secrets):** environment variables use `__` instead of `:` — e.g. `ConnectionStrings__DefaultConnection`, `Gemini__ApiKey`. Useful for Docker or production hosts.

### 3. API

```powershell
cd backend\DeviceTrackr.Api
dotnet run
```

Open Swagger at the URL shown in the console (e.g. `http://localhost:5035/swagger`).

### 4. Angular

```bash
cd frontend/devicetrackr-web
npm install
ng serve
```

App URL: **`http://localhost:4200`**

### Demo accounts (after seed)

As documented in `02-seed-data.sql`:

| Email | Password |
|--------|----------|
| ana@example.com | ana123 |
| mihai@example.com | mihai123 |
| elena@example.com | elena123 |

The seed assigns **iPhone 15** to **Ana** for demo purposes.

---
