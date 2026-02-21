#!/usr/bin/env bash
set -euo pipefail

CONFIGURATION="${1:-Release}"
RUNTIME="${2:-win-x64}"
PROJECT="src/Inventory.Api/Inventory.Api.csproj"

echo "==> Restoring packages"
dotnet restore "$PROJECT"

echo "==> Publishing $PROJECT for $RUNTIME"
dotnet publish "$PROJECT" \
  -c "$CONFIGURATION" \
  -r "$RUNTIME" \
  --self-contained true \
  /p:PublishSingleFile=true \
  /p:PublishTrimmed=false

echo "✅ Publish completed: src/Inventory.Api/bin/$CONFIGURATION/net8.0/$RUNTIME/publish"
