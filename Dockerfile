FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source
COPY . .
RUN dotnet restore
RUN dotnet publish ./src/Mimir.Api -c release -o /app --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:6.0.15-alpine3.17-arm64v8
WORKDIR /app
COPY --from=build /app ./
EXPOSE 5000
RUN apk --no-cache add curl
HEALTHCHECK CMD curl --fail http://localhost:5000/healthz || exit 1
ENTRYPOINT ["dotnet", "Mimir.Api.dll"]

# docker build -t mimir-api .
# docker run -it --rm -p 5000:8300 --name mimir-api-1 mimir-api