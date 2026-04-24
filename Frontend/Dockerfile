FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Frontend.csproj ./
RUN dotnet restore Frontend.csproj

COPY . ./
RUN dotnet publish Frontend.csproj -c Release -o /app/publish

FROM nginx:1.27-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

EXPOSE 80
