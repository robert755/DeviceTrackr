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

### 2. Connection string

In `backend/DeviceTrackr.Api/appsettings.json`, set **`ConnectionStrings:DefaultConnection`** for your SQL instance (server, database, authentication). **Do not commit real passwords** to Git; use User Secrets or environment variables in production.

### 3. API

```bash
cd backend/DeviceTrackr.Api
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

## Current limitations

- **Auth is in-memory in the browser only**—refreshing the page requires signing in again; the devices/users API **does not** require a token or session cookie (any client that knows the URL can call the endpoints).
- Assignment sends `userId` in the request body—in production you would typically use the authenticated user identity on the server (JWT, cookies, etc.).

---

## License / project type

Educational / demo project—add a license file as needed.
