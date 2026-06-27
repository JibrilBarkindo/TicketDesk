# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/TicketDesk.Api/TicketDesk.Api.csproj", "src/TicketDesk.Api/"]
RUN dotnet restore "src/TicketDesk.Api/TicketDesk.Api.csproj"

COPY . .
WORKDIR "/src/src/TicketDesk.Api"
RUN dotnet publish "TicketDesk.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TicketDesk.Api.dll"]
