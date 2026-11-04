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

    /// <summary>Prepara e invia un'email di testo con i recapiti della persona che ha compilato il modulo.</summary>
    public async Task InviaRichiestaContattoAsync(ContactRequest richiesta, CancellationToken tokenAnnullamento = default)
    {
        if (string.IsNullOrWhiteSpace(_impostazioniEmail.SmtpHost) ||
            _impostazioniEmail.SmtpPort is < 1 or > 65535 ||
            string.IsNullOrWhiteSpace(_impostazioniEmail.SenderAddress) ||
            string.IsNullOrWhiteSpace(_impostazioniEmail.SenderPassword) ||
            string.IsNullOrWhiteSpace(_impostazioniEmail.RecipientAddress))
        {
            throw new InvalidOperationException(
                "L'invio email non è ancora configurato. Imposta i dati SMTP locali seguendo le istruzioni nel README.");
        }

        var oggettoEmail = string.IsNullOrWhiteSpace(richiesta.Argomento) ? "primo colloquio" : richiesta.Argomento.Trim();
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
        messaggioEmail.ReplyToList.Add(new MailAddress(richiesta.Email.Trim(), richiesta.Nome.Trim()));

        using var clientSmtp = new SmtpClient(_impostazioniEmail.SmtpHost, _impostazioniEmail.SmtpPort)
        {
            EnableSsl = _impostazioniEmail.UseSsl,
            Credentials = new NetworkCredential(_impostazioniEmail.SenderAddress, _impostazioniEmail.SenderPassword)
        };

        await clientSmtp.SendMailAsync(messaggioEmail, tokenAnnullamento);
    }
}
