# recurring-defects

Detecting recurring aircraft defects from free-text maintenance reports, using public FAA Service Difficulty Reports (SDR).

> Status: Phase 1 (data foundation) in progress. Design: [docs/superpowers/specs/2026-11-26-tekrarlayan-ariza-design.md](docs/superpowers/specs/2026-11-26-tekrarlayan-ariza-design.md)

## What it does (target)

1. Ingests FAA SDR records into a standard `defect_event` table (shape inspired by ATA Spec 2000 logbook records).
2. Cleans technician free text (abbreviations, reference boilerplate).
3. Groups defect reports that describe the same problem, within the same aircraft model and JASC/ATA chapter.
4. Flags repeats on the same aircraft and fleet-level rates above an alert level (FAA AC 120-17B style), with measured accuracy.

## Run locally

Requirements: Docker Desktop.

```bash
docker compose up -d
docker compose exec db psql -U reliability -d reliability -c "\d defect_event"
```

PostgreSQL 17 + pgvector listens on `127.0.0.1:5434` only. Credentials in `docker-compose.yml` are for local development.

Migrations in `db/migrations/` run automatically **only on first start** (empty volume). After changing them: `docker compose down -v && docker compose up -d` (this deletes local data).

## Layout

```
db/migrations/   SQL schema
docs/notes/      domain notes (JASC, alert levels, Spec 2000, competitors)
docs/decisions.md  one line per design decision and why
```

## Data

FAA SDR data is public. Raw files go in `data/raw/` and are not committed.
