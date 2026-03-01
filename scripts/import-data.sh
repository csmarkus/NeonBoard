#!/usr/bin/env bash
set -euo pipefail

# Import NeonBoard data via API calls
#
# Usage:
#   ./scripts/import-data.sh <data.json> <api-base-url> <bearer-token>
#
# Example:
#   ./scripts/import-data.sh data.json http://localhost:5000 "eyJhbGci..."
#
# The data.json file should be produced by scripts/export-data.sql

DATA_FILE="${1:?Usage: $0 <data.json> <api-base-url> <bearer-token>}"
API_URL="${2:?Usage: $0 <data.json> <api-base-url> <bearer-token>}"
TOKEN="${3:?Usage: $0 <data.json> <api-base-url> <bearer-token>}"

if ! command -v jq &> /dev/null; then
  echo "Error: jq is required. Install it with: apt install jq / brew install jq"
  exit 1
fi

if [ ! -f "$DATA_FILE" ]; then
  echo "Error: File not found: $DATA_FILE"
  exit 1
fi

AUTH="Authorization: Bearer $TOKEN"
CT="Content-Type: application/json"

# Track old ID -> new ID mappings (API generates new IDs)
declare -A PROJECT_MAP
declare -A BOARD_MAP
declare -A COLUMN_MAP
declare -A LABEL_MAP
declare -A CARD_MAP

echo "=== Importing NeonBoard data ==="
echo ""

# --- Projects ---
echo "--- Projects ---"
PROJECT_COUNT=$(jq '.projects | length' "$DATA_FILE")
for i in $(seq 0 $((PROJECT_COUNT - 1))); do
  NAME=$(jq -r ".projects[$i].name" "$DATA_FILE")
  DESC=$(jq -r ".projects[$i].description" "$DATA_FILE")
  OLD_ID=$(jq -r ".projects[$i].id" "$DATA_FILE")

  RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_URL/api/projects" \
    -H "$AUTH" -H "$CT" \
    -d "$(jq -n --arg name "$NAME" --arg desc "$DESC" '{name: $name, description: $desc}')")

  HTTP_CODE=$(echo "$RESPONSE" | tail -1)
  BODY=$(echo "$RESPONSE" | head -n -1)

  if [ "$HTTP_CODE" -ge 200 ] && [ "$HTTP_CODE" -lt 300 ]; then
    NEW_ID=$(echo "$BODY" | jq -r '.id')
    PROJECT_MAP[$OLD_ID]=$NEW_ID
    echo "  ✓ Project: $NAME ($OLD_ID -> $NEW_ID)"
  else
    echo "  ✗ Project: $NAME — HTTP $HTTP_CODE: $BODY"
  fi
done

# --- Boards ---
echo ""
echo "--- Boards ---"
BOARD_COUNT=$(jq '.boards | length' "$DATA_FILE")
for i in $(seq 0 $((BOARD_COUNT - 1))); do
  NAME=$(jq -r ".boards[$i].name" "$DATA_FILE")
  OLD_ID=$(jq -r ".boards[$i].id" "$DATA_FILE")
  OLD_PROJECT_ID=$(jq -r ".boards[$i].projectId" "$DATA_FILE")
  NEW_PROJECT_ID="${PROJECT_MAP[$OLD_PROJECT_ID]}"

  RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_URL/api/projects/$NEW_PROJECT_ID/boards" \
    -H "$AUTH" -H "$CT" \
    -d "$(jq -n --arg name "$NAME" '{name: $name}')")

  HTTP_CODE=$(echo "$RESPONSE" | tail -1)
  BODY=$(echo "$RESPONSE" | head -n -1)

  if [ "$HTTP_CODE" -ge 200 ] && [ "$HTTP_CODE" -lt 300 ]; then
    NEW_ID=$(echo "$BODY" | jq -r '.id')
    BOARD_MAP[$OLD_ID]=$NEW_ID
    echo "  ✓ Board: $NAME ($OLD_ID -> $NEW_ID)"
  else
    echo "  ✗ Board: $NAME — HTTP $HTTP_CODE: $BODY"
  fi
done

# --- Columns ---
echo ""
echo "--- Columns ---"
COLUMN_COUNT=$(jq '.columns | length' "$DATA_FILE")
for i in $(seq 0 $((COLUMN_COUNT - 1))); do
  NAME=$(jq -r ".columns[$i].name" "$DATA_FILE")
  OLD_ID=$(jq -r ".columns[$i].id" "$DATA_FILE")
  OLD_BOARD_ID=$(jq -r ".columns[$i].boardId" "$DATA_FILE")
  NEW_PROJECT_ID=""
  NEW_BOARD_ID="${BOARD_MAP[$OLD_BOARD_ID]}"

  # Find the project for this board
  for j in $(seq 0 $((BOARD_COUNT - 1))); do
    BID=$(jq -r ".boards[$j].id" "$DATA_FILE")
    if [ "$BID" = "$OLD_BOARD_ID" ]; then
      OLD_PID=$(jq -r ".boards[$j].projectId" "$DATA_FILE")
      NEW_PROJECT_ID="${PROJECT_MAP[$OLD_PID]}"
      break
    fi
  done

  RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_URL/api/projects/$NEW_PROJECT_ID/boards/$NEW_BOARD_ID/columns" \
    -H "$AUTH" -H "$CT" \
    -d "$(jq -n --arg name "$NAME" '{name: $name}')")

  HTTP_CODE=$(echo "$RESPONSE" | tail -1)
  BODY=$(echo "$RESPONSE" | head -n -1)

  if [ "$HTTP_CODE" -ge 200 ] && [ "$HTTP_CODE" -lt 300 ]; then
    NEW_ID=$(echo "$BODY" | jq -r '.id')
    COLUMN_MAP[$OLD_ID]=$NEW_ID
    echo "  ✓ Column: $NAME ($OLD_ID -> $NEW_ID)"
  else
    echo "  ✗ Column: $NAME — HTTP $HTTP_CODE: $BODY"
  fi
done

# --- Labels ---
echo ""
echo "--- Labels ---"
LABEL_COUNT=$(jq '.labels | length' "$DATA_FILE")
for i in $(seq 0 $((LABEL_COUNT - 1))); do
  NAME=$(jq -r ".labels[$i].name" "$DATA_FILE")
  COLOR=$(jq -r ".labels[$i].color" "$DATA_FILE")
  OLD_ID=$(jq -r ".labels[$i].id" "$DATA_FILE")
  OLD_BOARD_ID=$(jq -r ".labels[$i].boardId" "$DATA_FILE")
  NEW_BOARD_ID="${BOARD_MAP[$OLD_BOARD_ID]}"

  # Find the project for this board
  NEW_PROJECT_ID=""
  for j in $(seq 0 $((BOARD_COUNT - 1))); do
    BID=$(jq -r ".boards[$j].id" "$DATA_FILE")
    if [ "$BID" = "$OLD_BOARD_ID" ]; then
      OLD_PID=$(jq -r ".boards[$j].projectId" "$DATA_FILE")
      NEW_PROJECT_ID="${PROJECT_MAP[$OLD_PID]}"
      break
    fi
  done

  RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_URL/api/projects/$NEW_PROJECT_ID/boards/$NEW_BOARD_ID/labels" \
    -H "$AUTH" -H "$CT" \
    -d "$(jq -n --arg name "$NAME" --arg color "$COLOR" '{name: $name, color: $color}')")

  HTTP_CODE=$(echo "$RESPONSE" | tail -1)
  BODY=$(echo "$RESPONSE" | head -n -1)

  if [ "$HTTP_CODE" -ge 200 ] && [ "$HTTP_CODE" -lt 300 ]; then
    NEW_ID=$(echo "$BODY" | jq -r '.id')
    LABEL_MAP[$OLD_ID]=$NEW_ID
    echo "  ✓ Label: $NAME ($OLD_ID -> $NEW_ID)"
  else
    echo "  ✗ Label: $NAME — HTTP $HTTP_CODE: $BODY"
  fi
done

# --- Cards (ordered by createdAt to preserve card numbers) ---
echo ""
echo "--- Cards ---"
CARD_COUNT=$(jq '.cards | length' "$DATA_FILE")
for i in $(seq 0 $((CARD_COUNT - 1))); do
  TITLE=$(jq -r ".cards[$i].title" "$DATA_FILE")
  DESC=$(jq -r ".cards[$i].description" "$DATA_FILE")
  OLD_ID=$(jq -r ".cards[$i].id" "$DATA_FILE")
  OLD_BOARD_ID=$(jq -r ".cards[$i].boardId" "$DATA_FILE")
  OLD_COLUMN_ID=$(jq -r ".cards[$i].columnId" "$DATA_FILE")
  NEW_BOARD_ID="${BOARD_MAP[$OLD_BOARD_ID]}"
  NEW_COLUMN_ID="${COLUMN_MAP[$OLD_COLUMN_ID]}"

  # Find the project for this board
  NEW_PROJECT_ID=""
  for j in $(seq 0 $((BOARD_COUNT - 1))); do
    BID=$(jq -r ".boards[$j].id" "$DATA_FILE")
    if [ "$BID" = "$OLD_BOARD_ID" ]; then
      OLD_PID=$(jq -r ".boards[$j].projectId" "$DATA_FILE")
      NEW_PROJECT_ID="${PROJECT_MAP[$OLD_PID]}"
      break
    fi
  done

  RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_URL/api/projects/$NEW_PROJECT_ID/boards/$NEW_BOARD_ID/cards" \
    -H "$AUTH" -H "$CT" \
    -d "$(jq -n --arg colId "$NEW_COLUMN_ID" --arg title "$TITLE" --arg desc "$DESC" \
      '{columnId: $colId, title: $title, description: $desc}')")

  HTTP_CODE=$(echo "$RESPONSE" | tail -1)
  BODY=$(echo "$RESPONSE" | head -n -1)

  if [ "$HTTP_CODE" -ge 200 ] && [ "$HTTP_CODE" -lt 300 ]; then
    NEW_ID=$(echo "$BODY" | jq -r '.id')
    CARD_MAP[$OLD_ID]=$NEW_ID
    echo "  ✓ Card: $TITLE ($OLD_ID -> $NEW_ID)"
  else
    echo "  ✗ Card: $TITLE — HTTP $HTTP_CODE: $BODY"
  fi
done

# --- Card Labels (stored as jsonb array on each card) ---
echo ""
echo "--- Card Labels ---"
CARD_LABEL_COUNT=0
for i in $(seq 0 $((CARD_COUNT - 1))); do
  OLD_CARD_ID=$(jq -r ".cards[$i].id" "$DATA_FILE")
  OLD_BOARD_ID=$(jq -r ".cards[$i].boardId" "$DATA_FILE")
  LABEL_IDS=$(jq -r ".cards[$i].labelIds // [] | .[]" "$DATA_FILE" 2>/dev/null)

  if [ -z "$LABEL_IDS" ]; then
    continue
  fi

  NEW_CARD_ID="${CARD_MAP[$OLD_CARD_ID]}"
  NEW_BOARD_ID="${BOARD_MAP[$OLD_BOARD_ID]}"

  # Find the project for this board
  NEW_PROJECT_ID=""
  for j in $(seq 0 $((BOARD_COUNT - 1))); do
    BID=$(jq -r ".boards[$j].id" "$DATA_FILE")
    if [ "$BID" = "$OLD_BOARD_ID" ]; then
      OLD_PID=$(jq -r ".boards[$j].projectId" "$DATA_FILE")
      NEW_PROJECT_ID="${PROJECT_MAP[$OLD_PID]}"
      break
    fi
  done

  for OLD_LABEL_ID in $LABEL_IDS; do
    NEW_LABEL_ID="${LABEL_MAP[$OLD_LABEL_ID]:-}"
    if [ -z "$NEW_LABEL_ID" ]; then
      echo "  ✗ Card label: unknown label $OLD_LABEL_ID on card $OLD_CARD_ID"
      continue
    fi

    RESPONSE=$(curl -s -w "\n%{http_code}" -X PUT \
      "$API_URL/api/projects/$NEW_PROJECT_ID/boards/$NEW_BOARD_ID/cards/$NEW_CARD_ID/labels/$NEW_LABEL_ID" \
      -H "$AUTH" -H "$CT")

    HTTP_CODE=$(echo "$RESPONSE" | tail -1)

    if [ "$HTTP_CODE" -ge 200 ] && [ "$HTTP_CODE" -lt 300 ]; then
      echo "  ✓ Card label: $NEW_CARD_ID <- $NEW_LABEL_ID"
      CARD_LABEL_COUNT=$((CARD_LABEL_COUNT + 1))
    else
      BODY=$(echo "$RESPONSE" | head -n -1)
      echo "  ✗ Card label: $OLD_CARD_ID <- $OLD_LABEL_ID — HTTP $HTTP_CODE: $BODY"
    fi
  done
done

echo ""
echo "=== Import complete ==="
echo "  Projects: ${#PROJECT_MAP[@]}"
echo "  Boards:   ${#BOARD_MAP[@]}"
echo "  Columns:  ${#COLUMN_MAP[@]}"
echo "  Labels:   ${#LABEL_MAP[@]}"
echo "  Cards:    ${#CARD_MAP[@]}"
echo "  Card labels: $CARD_LABEL_COUNT"
