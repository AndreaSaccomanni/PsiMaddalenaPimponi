using SitoMadda.UI.Components;
using SitoMadda.Application.Services;
using SitoMadda.Infrastructure;

// Configura l'applicazione, i servizi Blazor e il servizio di invio email.
var creatoreApp = WebApplication.CreateBuilder(args);

creatoreApp.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
creatoreApp.Services.AddSingleton<SiteContent>();
creatoreApp.Services.AddInfrastructure(creatoreApp.Configuration);

var applicazioneWeb = creatoreApp.Build();

// Gestisce gli errori, serve i file statici e registra le rotte dei componenti Razor.
if (!applicazioneWeb.Environment.IsDevelopment())
{
    applicazioneWeb.UseExceptionHandler("/Error", createScopeForErrors: true);
}

applicazioneWeb.UseStaticFiles();
applicazioneWeb.UseAntiforgery();

applicazioneWeb.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

applicazioneWeb.Run();
