# Immagine di build: SDK .NET 8 con tutto il codice, poi pubblicazione in Release.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# La copia completa precede il restore: con i soli .csproj gli static web assets di Blazor
# (_framework/blazor.web.js) non vengono generati e il sito pubblicato non funziona.
COPY . ./
RUN dotnet restore SitoMadda.UI/SitoMadda.UI.csproj
RUN dotnet publish SitoMadda.UI/SitoMadda.UI.csproj -c Release -o /app --no-restore

# Immagine finale: solo il runtime ASP.NET, senza SDK né codice sorgente.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Render espone il servizio sulla porta 8080 e termina lui il TLS: l'applicazione ascolta in HTTP.
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SitoMadda.UI.dll"]
