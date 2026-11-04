using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SitoMadda.Application.Services;
using SitoMadda.Infrastructure.Services;

namespace SitoMadda.Infrastructure;

/// <summary>Registra nel contenitore i servizi di infrastruttura e la configurazione SMTP.</summary>
public static class DependencyInjection
{
    /// <summary>Configura il mittente email e associa l'interfaccia applicativa all'implementazione SMTP.</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection servizi,
        IConfiguration configurazione)
    {
        servizi.Configure<EmailSettings>(configurazione.GetSection(EmailSettings.SectionName));
        servizi.AddScoped<IEmailSender, SmtpEmailSender>();
        return servizi;
    }
}
