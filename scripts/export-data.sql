-- Export NeonBoard data as JSON for re-import via API
-- Run: docker exec <postgres-container> psql -U postgres -d neonboarddb -f /dev/stdin < scripts/export-data.sql > data.json

SELECT json_build_object(
  'projects', (
    SELECT COALESCE(json_agg(json_build_object(
      'id', p."Id",
      'name', p."Name",
      'description', p."Description",
      'createdAt', p."CreatedAt"
    ) ORDER BY p."CreatedAt"), '[]'::json)
    FROM "Projects" p
  ),
  'boards', (
    SELECT COALESCE(json_agg(json_build_object(
      'id', b."Id",
      'projectId', b."ProjectId",
      'name', b."Name",
      'createdAt', b."CreatedAt"
    ) ORDER BY b."CreatedAt"), '[]'::json)
    FROM "Boards" b
  ),
  'columns', (
    SELECT COALESCE(json_agg(json_build_object(
      'id', col."Id",
      'boardId', col."BoardId",
      'name', col."Name",
      'position', col."Position"
    ) ORDER BY col."BoardId", col."Position"), '[]'::json)
    FROM "Columns" col
  ),
  'labels', (
    SELECT COALESCE(json_agg(json_build_object(
      'id', l."Id",
      'boardId', l."BoardId",
      'name', l."Name",
      'color', l."Color"
    ) ORDER BY l."BoardId", l."Name"), '[]'::json)
    FROM "Labels" l
  ),
  'cards', (
    SELECT COALESCE(json_agg(json_build_object(
      'id', c."Id",
      'boardId', c."BoardId",
      'columnId', c."ColumnId",
      'title', c."Title",
      'description', c."Description",
      'position', c."Position",
      'labelIds', c."LabelIds",
      'createdAt', c."CreatedAt"
    ) ORDER BY c."BoardId", c."CreatedAt"), '[]'::json)
    FROM "Cards" c
  )
);
