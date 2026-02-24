# WorkFlow

## Run with Docker

Prerequisites:
- Docker Desktop (with Compose)

From the repository root:

```bash
docker compose up --build
```

Services:
- Frontend: `http://localhost:5173`
- Backend API: `http://localhost:7190`
- Swagger: `http://localhost:7190/swagger`
- SQL Server: `localhost:1433`
- MongoDB: `localhost:27017`

Notes:
- The backend auto-runs EF Core migrations at startup (with retry while SQL Server initializes).
- Uploaded/seed contract files are persisted in the `backend_uploads` Docker volume.
- SQL and Mongo data are persisted in `sqlserver_data` and `mongo_data` volumes.

Stop containers:

```bash
docker compose down
```

Stop and remove volumes (fresh reset):

```bash
docker compose down -v
```
