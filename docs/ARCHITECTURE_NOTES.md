# Architecture Notes

## Caching layer for report generation

Report generation endpoints (attendance, grade cards) were re-computing the
same aggregates on every request under load testing, causing p95 latency
spikes during exam-result publish windows.

Added an in-memory LRU cache in front of the report aggregation queries,
keyed by (class, term, report_type), with a 10-minute TTL. Chose in-memory
over Redis for now since the app runs as a single instance per school
tenant — revisit if we move to multi-instance deployment.
