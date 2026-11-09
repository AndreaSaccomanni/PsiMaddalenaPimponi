using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SitoMadda.Application.Services;
using SitoMadda.Infrastructure.Services;

namespace SitoMadda.Infrastructure;

/// <summary>Registra nel contenitore i servizi di infrastruttura e la configurazione dell'invio email.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Configura l'invio delle richieste di contatto scegliendo il canale disponibile: l'API HTTP
    /// di Resend se c'è una chiave, altrimenti il server SMTP.
    /// </summary>
    /// <remarks>
    /// Il piano free di Render blocca il traffico SMTP in uscita: in produzione si usa quindi
    /// Resend, che funziona su HTTPS. In locale, senza chiave Resend, resta attivo SMTP con le
    /// credenziali negli User Secrets. Le due configurazioni non si sovrappongono: la presenza
    /// della chiave decide il canale e il messaggio inviato è identico.
    /// </remarks>
    /// <param name="servizi">Contenitore delle dipendenze dell'applicazione.</param>
    /// <param name="configurazione">Configurazione da cui leggere host, chiavi e destinatari.</param>
    /// <returns>Lo stesso contenitore, per concatenare altre registrazioni.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection servizi,
        IConfiguration configurazione)
    {
        // Destinatario, host e porta SMTP restano nella sezione Email.
        servizi.Configure<EmailSettings>(configurazione.GetSection(EmailSettings.SectionName));

        // Resend legge la propria sezione, con fallback sulle variabili usate su Render.
        servizi.AddOptions<ResendSettings>()
            .Configure(opzioni => configurazione.GetSection(ResendSettings.SectionName).Bind(opzioni))
            .PostConfigure(opzioni =>
            {
                opzioni.ApiKey = string.IsNullOrWhiteSpace(opzioni.ApiKey)
                    ? Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? string.Empty
                    : opzioni.ApiKey;

                var mittenteDaAmbiente = Environment.GetEnvironmentVariable("RESEND_FROM");
                if (!string.IsNullOrWhiteSpace(mittenteDaAmbiente))
                {
                    opzioni.Mittente = mittenteDaAmbiente;
                }

                if (string.IsNullOrWhiteSpace(opzioni.Destinatario))
                {
                    opzioni.Destinatario = configurazione["Email:RecipientAddress"] ?? string.Empty;
                }
            });

        var chiaveResend = configurazione["Resend:ApiKey"];
        if (string.IsNullOrWhiteSpace(chiaveResend))
        {
            chiaveResend = Environment.GetEnvironmentVariable("RESEND_API_KEY");
        }

        if (!string.IsNullOrWhiteSpace(chiaveResend))
        {
            servizi.AddHttpClient<IEmailSender, ResendEmailSender>(client =>
            {
                client.BaseAddress = new Uri("https://api.resend.com/");
                client.Timeout = TimeSpan.FromSeconds(20);
            });
        }
        else
        {
            servizi.AddScoped<IEmailSender, SmtpEmailSender>();
        }

        return servizi;
    }
}
