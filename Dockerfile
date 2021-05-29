FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS publish
WORKDIR /src
COPY . .
RUN dotnet publish src/CreditCard.Api/ -c Release --self-contained true -r linux-x64 /p:PublishSingleFile=true -o deploy

FROM base AS final
WORKDIR /app
COPY --from=publish /src/deploy .
CMD ASPNETCORE_URLS=http://*:$PORT /app/CreditCard.Api