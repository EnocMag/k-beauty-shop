## Prerequisites
- .NET 10
- Docker
## Instructions to run

### SQL container
Run sql docker container to get database up

```
docker run -d --name sqlserver -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=TuPassword123!" -e "MSSQL_PID=developer" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

### Applying EF migrations
Migrations should be automatically executed when running the api, but if you need to manually apply the migrations for any reason you can run this command:
```
cd src
dotnet ef database update --project Products.Infrastructure --startup-project Products.Api
```

