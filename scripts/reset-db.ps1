#!/usr/bin/env pwsh
# Drops and recreates the postgres volume. All data is lost.

$ErrorActionPreference = 'Stop'
Set-Location -Path (Split-Path -Parent $PSScriptRoot)

Write-Host "This will DESTROY the postgres volume and all data in it." -ForegroundColor Yellow
$response = Read-Host "Type 'yes' to continue"
if ($response -ne 'yes') {
    Write-Host "Aborted." -ForegroundColor Yellow
    exit 1
}

docker compose stop postgres
docker compose rm -f postgres

$projectName = (Split-Path -Leaf (Get-Location)).ToLower() -replace '[^a-z0-9]', ''
$volumeName = "${projectName}_postgres_data"

if ((docker volume ls --quiet --filter "name=^${volumeName}$").Length -gt 0) {
    docker volume rm $volumeName | Out-Null
    Write-Host "Removed volume $volumeName." -ForegroundColor Green
} else {
    Write-Host "Volume $volumeName not found, skipping." -ForegroundColor DarkGray
}

docker compose up postgres -d
Write-Host "Postgres reset complete." -ForegroundColor Green
