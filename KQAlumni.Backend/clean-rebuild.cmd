@echo off
dotnet clean
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s /q "%%d" 2>nul
dotnet build --no-incremental
