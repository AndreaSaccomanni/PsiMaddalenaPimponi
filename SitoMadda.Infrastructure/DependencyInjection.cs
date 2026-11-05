using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SitoMadda.Application.Services;
using SitoMadda.Infrastructure.Services;

namespace SitoMadda.Infrastructure;

/// <summary>Registra nel contenitore i servizi di infrastruttura e la configurazione SMTP.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Legge la sezione "Email" della configurazione e registra l'invio delle richieste.
    /// </summary>
    /// <remarks>
    /// <see cref="EmailSettings"/> è esposto tramite IOptions, così i valori possono arrivare
    /// da appsettings o dagli User Secrets senza modificare il codice. Il servizio email è
    /// scoped, come il circuito Blazor che lo usa, mentre il servizio è senza stato e non
    /// conserva connessioni SMTP tra un invio e l'altro.
    /// </remarks>
    /// <param name="servizi">Contenitore delle dipendenze dell'applicazione.</param>
    /// <param name="configurazione">Configurazione da cui leggere host, porta e credenziali SMTP.</param>
    /// <returns>Lo stesso contenitore, per concatenare altre registrazioni.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection servizi,
        IConfiguration configurazione)
    {
        servizi.Configure<EmailSettings>(configurazione.GetSection(EmailSettings.SectionName));
        servizi.AddScoped<IEmailSender, SmtpEmailSender>();
        return servizi;
    }
}
