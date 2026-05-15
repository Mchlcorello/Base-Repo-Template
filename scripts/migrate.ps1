#!/usr/bin/env pwsh
# Applies EF Core migrations to the database referenced by ConnectionStrings__AppDb.

$ErrorActionPreference = 'Stop'
Set-Location -Path (Split-Path -Parent $PSScriptRoot)

dotnet ef database update --project src/api/Base.Api
