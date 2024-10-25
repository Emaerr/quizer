#!/bin/sh

dotnet tool restore
dotnet ef migrations add InitialCreate
dotnet ef database update --connection $ConnectionStrings__DefaultConnection

dotnet Quizer.dll