using System.Net;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using SitoMadda.Application.Services;
using SitoMadda.Infrastructure;
using SitoMadda.Infrastructure.Services;
using SitoMadda.UI.Components;
using SitoMadda.UI.Endpoints;

// Configura l'applicazione, i servizi Blazor, il limite di invio del modulo e il servizio email.
var creatoreApp = WebApplication.CreateBuilder(args);
var configurazione = creatoreApp.Configuration;

creatoreApp.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
creatoreApp.Services.AddSingleton<SiteContent>();
creatoreApp.Services.AddInfrastructure(configurazione);

// Soglie del modulo contatti, regolabili da configurazione senza ricompilare.
var sezioneContatti = configurazione.GetSection("ContactForm");
creatoreApp.Services.AddSingleton(new ContactFormLimiter(
    valoreContatti("MaxPerWindow", 3),
    valoreContatti("WindowMinutes", 10),
    valoreContatti("MinSecondsBetween", 20),
    valoreContatti("GlobalPerHour", 30)));

// Chiavi di Data Protection: senza una cartella stabile, al riavvio o su più istanze i token
// antiforgery e i circuiti Blazor non sono più validi.
var protezioneDati = creatoreApp.Services.AddDataProtection().SetApplicationName("SitoMadda");
var cartellaChiavi = configurazione["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(cartellaChiavi))
{
    protezioneDati.PersistKeysToFileSystem(new DirectoryInfo(cartellaChiavi));
}

// Dietro un proxy (IIS, nginx, Cloudflare) l'indirizzo del visitatore e lo schema HTTPS
// arrivano negli header X-Forwarded-*: vanno letti prima di HSTS e del redirect HTTPS.
creatoreApp.Services.Configure<ForwardedHeadersOptions>(opzioni =>
{
    opzioni.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    if (IPAddress.TryParse(configurazione["ReverseProxy:KnownProxy"], out var indirizzoProxy))
    {
        opzioni.KnownProxies.Add(indirizzoProxy);
    }
});

creatoreApp.Services.AddHsts(opzioni =>
{
    opzioni.MaxAge = TimeSpan.FromDays(365);
    opzioni.IncludeSubDomains = false;
    opzioni.Preload = false;
});

creatoreApp.Services.AddHealthChecks();
creatoreApp.Services.AddResponseCompression(opzioni =>
{
    opzioni.EnableForHttps = true;
});

var applicazioneWeb = creatoreApp.Build();

// Se il modulo non ha le credenziali SMTP il sito resta online, ma l'avviso finisce nei log
// invece di scoprirlo dal primo visitatore che prova a scrivere.
var impostazioniEmail = applicazioneWeb.Services.GetRequiredService<IOptions<EmailSettings>>().Value;
if (string.IsNullOrWhiteSpace(impostazioniEmail.SenderAddress) || string.IsNullOrWhiteSpace(impostazioniEmail.SenderPassword))
{
    applicazioneWeb.Logger.LogWarning(
        "Configurazione SMTP incompleta: imposta Email__SenderAddress ed Email__SenderPassword. Fino ad allora il modulo contatti mostrerà l'indirizzo email diretto.");
}

// Rende effettivi gli header del proxy prima di qualunque altro middleware che li usa.
applicazioneWeb.UseForwardedHeaders();

// Gestisce gli errori, applica HTTPS e imposta le intestazioni di sicurezza.
if (!applicazioneWeb.Environment.IsDevelopment())
{
    applicazioneWeb.UseExceptionHandler("/Error", createScopeForErrors: true);
    applicazioneWeb.UseHsts();
    applicazioneWeb.UseHttpsRedirection();
}

// Intestazioni di sicurezza applicate a ogni risposta. La CSP è ora attiva (prima era in sola
// segnalazione): font, script e immagini arrivano solo dal sito, le mappe solo da Google.
applicazioneWeb.Use(async (contesto, successivo) =>
{
    var intestazioni = contesto.Response.Headers;
    intestazioni["X-Content-Type-Options"] = "nosniff";
    intestazioni["X-Frame-Options"] = "DENY";
    intestazioni["Referrer-Policy"] = "strict-origin-when-cross-origin";
    intestazioni["Permissions-Policy"] = "camera=(), geolocation=(), microphone=()";
    intestazioni["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; " +
        "font-src 'self'; img-src 'self' data:; connect-src 'self'; " +
        "frame-src https://www.google.com https://maps.google.com; form-action 'self'; " +
        "base-uri 'self'; object-src 'none'; frame-ancestors 'none'";

    await successivo(contesto);
});

// Ordine voluto: prima la compressione (altrimenti comprimerebbe un corpo già scritto), poi
// i file statici con cache breve, infine la pagina 404 per le rotte sconosciute.
// CSS e JS sono rivalidati a ogni caricamento perché cambiano con i deploy; le immagini
// possono restare in cache del browser per un'ora.
applicazioneWeb.UseResponseCompression();
applicazioneWeb.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = contesto =>
    {
        // CSS e JS cambiano con i deploy: il browser rivalida, le immagini restano in cache.
        var percorso = contesto.Context.Request.Path.Value ?? string.Empty;
        contesto.Context.Response.Headers.CacheControl = percorso.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
            percorso.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
            ? "no-cache"
            : "public, max-age=3600";
    }
});

// Le rotte sconosciute mostrano la pagina 404 mantenendo lo stato HTTP 404.
applicazioneWeb.UseStatusCodePagesWithReExecute("/pagina-non-trovata");

applicazioneWeb.UseAntiforgery();

applicazioneWeb.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// robots.txt, sitemap.xml e health check non sono pagine: gli indirizzi restano configurabili
// e si adattano da soli al dominio con cui viene raggiunto il sito.
applicazioneWeb.MapSiteMetadataEndpoints();
applicazioneWeb.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = (contesto, risultato) => contesto.Response.WriteAsync(risultato.Status.ToString())
});

applicazioneWeb.Run();

// Legge un valore numerico dalla sezione ContactForm, con un default se manca o non è valido.
int valoreContatti(string nome, int predefinito) =>
    int.TryParse(configurazione[$"ContactForm:{nome}"], out var valore) ? valore : predefinito;
