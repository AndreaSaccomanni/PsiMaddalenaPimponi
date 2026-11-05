using SitoMadda.UI.Components;
using SitoMadda.Application.Services;
using SitoMadda.Infrastructure;

// Configura l'applicazione, i servizi Blazor e il servizio di invio email.
var creatoreApp = WebApplication.CreateBuilder(args);

creatoreApp.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
creatoreApp.Services.AddSingleton<SiteContent>();
creatoreApp.Services.AddInfrastructure(creatoreApp.Configuration);
creatoreApp.Services.AddResponseCompression(opzioni =>
{
    opzioni.EnableForHttps = true;
});

var applicazioneWeb = creatoreApp.Build();

// Gestisce gli errori, applica HTTPS e imposta le intestazioni di sicurezza.
if (!applicazioneWeb.Environment.IsDevelopment())
{
    applicazioneWeb.UseExceptionHandler("/Error", createScopeForErrors: true);
    applicazioneWeb.UseHsts();
    applicazioneWeb.UseHttpsRedirection();
}

// La CSP è in modalità di sola segnalazione per non bloccare contenuti in caso di modifiche future.
applicazioneWeb.Use(async (contesto, successivo) =>
{
    var intestazioni = contesto.Response.Headers;
    intestazioni["X-Content-Type-Options"] = "nosniff";
    intestazioni["Referrer-Policy"] = "strict-origin-when-cross-origin";
    intestazioni["Permissions-Policy"] = "camera=(), geolocation=(), microphone=()";
    intestazioni["Content-Security-Policy-Report-Only"] =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com; img-src 'self' data:; " +
        "frame-src https://www.google.com https://maps.google.com; connect-src 'self' ws: wss:; " +
        "form-action 'self'; base-uri 'self'; frame-ancestors 'self'";

    await successivo(contesto);
});

// Comprime le risposte testuali e serve i file statici con cache breve lato browser.
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

applicazioneWeb.Run();
