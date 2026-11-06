using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SitoMadda.Application.Models;
using SitoMadda.Application.Services;

namespace SitoMadda.Infrastructure.Services;

/// <summary>Invia le richieste di contatto usando il server SMTP configurato.</summary>
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

        // L'oggetto può contenere testo libero: va ripulito da ritorni a capo e caratteri di
        // controllo, altrimenti MailMessage rifiuta il messaggio con ArgumentException.
        var oggettoEmail = PulisciValoreIntestazione(
            string.IsNullOrWhiteSpace(richiesta.Argomento) ? "primo colloquio" : richiesta.Argomento);

        if (oggettoEmail.Length == 0)
        {
            oggettoEmail = "primo colloquio";
        }

        using var messaggioEmail = new MailMessage
        {
            From = new MailAddress(_impostazioniEmail.SenderAddress, contenutiSito.NomeCompleto),
            Subject = $"Richiesta dal sito: {oggettoEmail}",
            Body = $"""
                Nuovo messaggio dal sito di {contenutiSito.NomeCompleto}

                Nome: {richiesta.Nome.Trim()}
                Email: {richiesta.Email.Trim()}
                Telefono: {richiesta.Telefono?.Trim() ?? "Non indicato"}
                Argomento: {richiesta.Argomento?.Trim() ?? "Non indicato"}

                Messaggio:
                {richiesta.Messaggio.Trim()}
                """,
            IsBodyHtml = false
        };
        messaggioEmail.To.Add(new MailAddress(_impostazioniEmail.RecipientAddress));
        messaggioEmail.ReplyToList.Add(CreaIndirizzoRisposta(richiesta));

        // Il client è creato per singolo invio: SmtpClient non è thread-safe e va rilasciato.
        using var clientSmtp = new SmtpClient(_impostazioniEmail.SmtpHost, _impostazioniEmail.SmtpPort)
        {
            EnableSsl = _impostazioniEmail.UseSsl,
            Credentials = new NetworkCredential(_impostazioniEmail.SenderAddress, _impostazioniEmail.SenderPassword)
        };

        await clientSmtp.SendMailAsync(messaggioEmail, tokenAnnullamento);
    }

    /// <summary>
    /// Normalizza un valore destinato a un'intestazione email: sostituisce i caratteri di
    /// controllo con spazi, compatta le sequenze di spazi e limita la lunghezza a 100 caratteri.
    /// Serve a impedire iniezioni di intestazioni e valori malformati.
    /// </summary>
    private static string PulisciValoreIntestazione(string valore)
    {
        var caratteri = valore.Select(carattere => char.IsControl(carattere) ? ' ' : carattere).ToArray();
        var pulito = new string(caratteri).Trim();

        while (pulito.Contains("  ", StringComparison.Ordinal))
        {
            pulito = pulito.Replace("  ", " ", StringComparison.Ordinal);
        }

        return pulito.Length > 100 ? pulito[..100] : pulito;
    }

    /// <summary>
    /// Costruisce l'indirizzo Reply-To usando l'email del visitatore e il suo nome come nome
    /// visualizzato. Il nome viene privato di virgolette, barre rovesciate e caratteri di
    /// controllo: MailAddress non fa escaping del display name e un valore grezzo produrrebbe
    /// un'intestazione non valida.
    /// </summary>
    private static MailAddress CreaIndirizzoRisposta(ContactRequest richiesta)
    {
        var indirizzo = richiesta.Email.Trim();
        var nomeVisualizzato = new string(richiesta.Nome
            .Select(carattere => char.IsControl(carattere) || carattere == '"' || carattere == '\\' ? ' ' : carattere)
            .ToArray()).Trim();

        while (nomeVisualizzato.Contains("  ", StringComparison.Ordinal))
        {
            nomeVisualizzato = nomeVisualizzato.Replace("  ", " ", StringComparison.Ordinal);
        }

        if (nomeVisualizzato.Length > 100)
        {
            nomeVisualizzato = nomeVisualizzato[..100].TrimEnd();
        }

        return nomeVisualizzato.Length == 0
            ? new MailAddress(indirizzo)
            : new MailAddress(indirizzo, nomeVisualizzato);
    }
}
