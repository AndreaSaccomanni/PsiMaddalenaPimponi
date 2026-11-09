using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SitoMadda.Application.Models;
using SitoMadda.Application.Services;

namespace SitoMadda.Infrastructure.Services;

/// <summary>Configurazione dell'invio tramite l'API HTTP di Resend.</summary>
public sealed class ResendSettings
{
    /// <summary>Nome della sezione di configurazione usata per Resend.</summary>
    public const string SectionName = "Resend";

    /// <summary>Chiave API dell'account Resend; arriva da User Secrets o dalla variabile RESEND_API_KEY.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Mittente dichiarato a Resend. Con il dominio non ancora verificato si può usare
    /// onboarding@resend.dev, che consegna solo all'indirizzo dell'account; dopo la verifica
    /// del dominio si usa un indirizzo del dominio stesso.
    /// </summary>
    public string Mittente { get; set; } = "onboarding@resend.dev";

    /// <summary>Indirizzo che riceve le richieste inviate dal modulo.</summary>
    public string Destinatario { get; set; } = string.Empty;
}

/// <summary>
/// Invia le richieste di contatto tramite l'API HTTP di Resend. Serve dove il traffico SMTP in
/// uscita è bloccato (piano free di Render) e non richiede di aprire porte o gestire server.
/// </summary>
public sealed class ResendEmailSender(
    HttpClient client,
    IOptions<ResendSettings> opzioniResend,
    SiteContent contenutiSito,
    ILogger<ResendEmailSender> logger) : IEmailSender
{
    private readonly ResendSettings _impostazioni = opzioniResend.Value;

    /// <summary>
    /// Invia alla titolare lo stesso testo prodotto per SMTP, con il visitatore come Reply-To.
    /// </summary>
    /// <param name="richiesta">Dati già validati dal modulo di contatto.</param>
    /// <param name="tokenAnnullamento">Token per interrompere l'invio se la richiesta viene annullata.</param>
    /// <exception cref="InvalidOperationException">
    /// Manca la chiave API o il destinatario: il chiamante mostra l'alternativa concreta al visitatore.
    /// </exception>
    /// <exception cref="HttpRequestException">Resend ha rifiutato l'invio o non è raggiungibile.</exception>
    public async Task InviaRichiestaContattoAsync(ContactRequest richiesta, CancellationToken tokenAnnullamento = default)
    {
        if (string.IsNullOrWhiteSpace(_impostazioni.ApiKey) || string.IsNullOrWhiteSpace(_impostazioni.Destinatario))
        {
            throw new InvalidOperationException(
                "L'invio email non è ancora configurato. Imposta Resend:ApiKey e Resend:Destinatario seguendo le istruzioni nel README.");
        }

        var corpo = new Dictionary<string, object>
        {
            ["from"] = _impostazioni.Mittente,
            ["to"] = new[] { _impostazioni.Destinatario },
            ["subject"] = MessaggioContatto.Oggetto(richiesta),
            ["text"] = MessaggioContatto.Corpo(richiesta, contenutiSito),
            ["reply_to"] = MessaggioContatto.RispostaA(richiesta).ToString()
        };

        using var richiestaHttp = new HttpRequestMessage(HttpMethod.Post, "emails")
        {
            Content = JsonContent.Create(corpo)
        };
        richiestaHttp.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _impostazioni.ApiKey);

        using var risposta = await client.SendAsync(richiestaHttp, tokenAnnullamento);

        if (!risposta.IsSuccessStatusCode)
        {
            var dettaglio = await risposta.Content.ReadAsStringAsync(tokenAnnullamento);
            logger.LogError(
                "Resend ha rifiutato l'email a {Destinatario}: {Stato} {Dettaglio}",
                _impostazioni.Destinatario,
                (int)risposta.StatusCode,
                dettaglio);

            throw new HttpRequestException(
                $"Invio email rifiutato da Resend con stato {(int)risposta.StatusCode}.");
        }

        logger.LogInformation("Richiesta di contatto inviata tramite Resend a {Destinatario}.", _impostazioni.Destinatario);
    }
}
