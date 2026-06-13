#!/bin/sh
set -e
mkdir -p /app/uploads
chmod 777 /app/uploads
exec dotnet matdev.API.dll
