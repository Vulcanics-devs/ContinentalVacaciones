#!/bin/bash
# echo "Compilando el proyecto..."
# dotnet build /app/tiempo-libre.csproj --verbosity detailed
echo "Ejecutando migraciones..."
dotnet ef database update --project /app/tiempo-libre.csproj --verbose
echo "Iniciando aplicación..."
dotnet tiempo-libre.dll