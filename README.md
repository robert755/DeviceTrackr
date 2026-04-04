# DeviceTrackr

Web app for **company-owned devices** (phones, tablets, laptops): specs, who they are assigned to, search, and optional **Gemini**-generated descriptions.

**Stack:** SQL Server · ASP.NET Core 8 Web API (EF Core) · Angular 17 (standalone).

**Repo layout**

| Path | Role |
|------|------|
| `database/` | SQL scripts (schema + seed) |
| `backend/DeviceTrackr.Api/` | REST API |
| `frontend/devicetrackr-web/` | Angular UI |

---

## Backend API (`/api/...`)

Base URL in dev is usually `http://localhost:5035`. Swagger is on in **Development**.

### Auth — `api/auth`

| Method | Route | Description |
|--------|--------|-------------|
| POST | `/register` | Create account (email, password, role, location; optional display name) |
| POST | `/login` | Login; returns user id, name, email (session is kept in the Angular app) |

### Devices — `api/devices`

| Method | Route | Description |
|--------|--------|-------------|
| GET | `/` | List all devices (includes assigned user when set) |
| GET | `/search?q=` | Free-text search (name, manufacturer, OS fields, description); empty `q` = all |
| GET | `/{id}` | Get one device |
| POST | `/` | Create device |
| PUT | `/{id}` | Update **only if unassigned** |
| DELETE | `/{id}` | Delete **only if unassigned** |
| POST | `/{id}/assign` | Body `{ "userId": n }` — assign to user |
| POST | `/{id}/unassign` | Body `{ "userId": n }` — unassign (only the assigned user) |
| POST | `/{id}/generate-description` | Gemini writes `Description` (needs `Gemini:ApiKey`) |

### Users — `api/users`

| Method | Route | Description |
|--------|--------|-------------|
| GET | `/` | List users |
| GET | `/{id}` | Get user |
| POST | `/` | Create user |
| PUT | `/{id}` | Update user |
| DELETE | `/{id}` | Delete user |

**CORS** allows `http://localhost:4200` for the Angular dev server.

---

## Run locally

1. **Database** — run `database/01-create-database-and-tables.sql`, then `02-seed-data.sql`.

2. **User secrets** (from `backend\DeviceTrackr.Api`; never commit real secrets):

   ```powershell
   cd backend\DeviceTrackr.Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=DeviceTrackr;...;TrustServerCertificate=True;"
   dotnet user-secrets set "Gemini:ApiKey" "YOUR_KEY"
   ```

   Optional: `dotnet user-secrets set "Gemini:Model" "gemini-2.5-flash"`. List with `dotnet user-secrets list`.  
   Alternative: env vars `ConnectionStrings__DefaultConnection`, `Gemini__ApiKey`.

3. **API** — `dotnet run` in `backend\DeviceTrackr.Api` (see console for URL + Swagger).

4. **Frontend** — `cd frontend/devicetrackr-web` → `npm install` → `ng serve` → `http://localhost:4200`. Update `device-api.service.ts` if the API port changes.

**Demo accounts** (after seed, see `02-seed-data.sql`): `ana@example.com` / `ana123`, `mihai@example.com` / `mihai123`, `elena@example.com` / `elena123`.
