using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SitoMadda.Application.Models;
using SitoMadda.Application.Services;

namespace SitoMadda.Infrastructure.Services;

/// <summary>
/// Invia le richieste di contatto usando il server SMTP configurato. È il canale usato in
/// sviluppo e su host che consentono le connessioni SMTP; in produzione su Render free il
/// traffico SMTP è bloccato e interviene <see cref="ResendEmailSender"/>.
/// </summary>
public sealed class SmtpEmailSender(
    IOptions<EmailSettings> opzioniEmail,
    SiteContent contenutiSito) : IEmailSender
{
    private readonly EmailSettings _impostazioniEmail = opzioniEmail.Value;

    /// <summary>
    /// Invia alla titolare un'email di testo con i dati inseriti nel modulo di contatto.
    /// </summary>
    /// <remarks>
    /// Il flusso è: verifica che la configurazione SMTP sia completa, ripulisce i valori che
    /// finiscono nelle intestazioni (oggetto e Reply-To), compone il messaggio in testo semplice
    /// e lo consegna al server configurato. Il mittente resta l'account SMTP, mentre la persona
    /// che ha scritto viene impostata come Reply-To così la risposta arriva a lei.
    /// </remarks>
    /// <param name="richiesta">Dati già validati dal modulo di contatto.</param>
    /// <param name="tokenAnnullamento">Token per interrompere l'invio se la richiesta viene annullata.</param>
    /// <exception cref="InvalidOperationException">
    /// La configurazione SMTP è incompleta: il chiamante deve mostrare un messaggio generico e
    /// registrare il dettaglio nei log, senza esporlo al visitatore.
    /// </exception>
    public async Task InviaRichiestaContattoAsync(ContactRequest richiesta, CancellationToken tokenAnnullamento = default)
    {
        // Blocco preventivo: senza host, porta valida, mittente, password e destinatario non ha
        // senso tentare la connessione, e l'errore deve essere riconoscibile dal chiamante.
        if (string.IsNullOrWhiteSpace(_impostazioniEmail.SmtpHost) ||
            _impostazioniEmail.SmtpPort is < 1 or > 65535 ||
            string.IsNullOrWhiteSpace(_impostazioniEmail.SenderAddress) ||
            string.IsNullOrWhiteSpace(_impostazioniEmail.SenderPassword) ||
            string.IsNullOrWhiteSpace(_impostazioniEmail.RecipientAddress))
        {
            throw new InvalidOperationException(
                "L'invio email non è ancora configurato. Imposta i dati SMTP locali seguendo le istruzioni nel README.");
        }

        using var messaggioEmail = new MailMessage
        {
            From = new MailAddress(_impostazioniEmail.SenderAddress, contenutiSito.NomeCompleto),
            Subject = MessaggioContatto.Oggetto(richiesta),
            Body = MessaggioContatto.Corpo(richiesta, contenutiSito),
            IsBodyHtml = false
        };
        messaggioEmail.To.Add(new MailAddress(_impostazioniEmail.RecipientAddress));
        messaggioEmail.ReplyToList.Add(MessaggioContatto.RispostaA(richiesta));

        // Il client è creato per singolo invio: SmtpClient non è thread-safe e va rilasciato.
        // Il timeout evita che un server SMTP che non risponde tenga occupato il circuito.
        using var clientSmtp = new SmtpClient(_impostazioniEmail.SmtpHost, _impostazioniEmail.SmtpPort)
        {
            EnableSsl = _impostazioniEmail.UseSsl,
            Credentials = new NetworkCredential(_impostazioniEmail.SenderAddress, _impostazioniEmail.SenderPassword),
            Timeout = 15000
        };

        await clientSmtp.SendMailAsync(messaggioEmail, tokenAnnullamento);
    }
}
