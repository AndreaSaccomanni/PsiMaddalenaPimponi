using System.Net.Mail;
using SitoMadda.Application.Models;
using SitoMadda.Application.Services;

namespace SitoMadda.Infrastructure.Services;

/// <summary>
/// Compone oggetto, corpo e indirizzo di risposta della richiesta di contatto. È condiviso fra
/// i canali di invio (SMTP in locale, API HTTP in produzione) così il messaggio ricevuto è
/// identico qualunque sia il provider.
/// </summary>
internal static class MessaggioContatto
{
    /// <summary>Oggetto dell'email, con l'argomento scelto ripulito da caratteri non validi.</summary>
    internal static string Oggetto(ContactRequest richiesta)
    {
        var argomento = PulisciIntestazione(
            string.IsNullOrWhiteSpace(richiesta.Argomento) ? "primo colloquio" : richiesta.Argomento);

        return $"Richiesta dal sito: {(argomento.Length == 0 ? "primo colloquio" : argomento)}";
    }

    /// <summary>Corpo in testo semplice con tutti i dati inseriti nel modulo.</summary>
    internal static string Corpo(ContactRequest richiesta, SiteContent contenuti) =>
        $"""
        Nuovo messaggio dal sito di {contenuti.NomeCompleto}

        Nome: {richiesta.Nome.Trim()}
        Email: {richiesta.Email.Trim()}
        Telefono: {richiesta.Telefono?.Trim() ?? "Non indicato"}
        Argomento: {richiesta.Argomento?.Trim() ?? "Non indicato"}

        Messaggio:
        {richiesta.Messaggio.Trim()}
        """;

    /// <summary>
    /// Indirizzo del visitatore con il suo nome come nome visualizzato, da usare come Reply-To:
    /// premendo "Rispondi" la psicologa scrive direttamente a chi ha compilato il modulo.
    /// </summary>
    internal static MailAddress RispostaA(ContactRequest richiesta)
    {
        var indirizzo = richiesta.Email.Trim();
        var nome = PulisciNomeVisualizzato(richiesta.Nome);

        return nome.Length == 0 ? new MailAddress(indirizzo) : new MailAddress(indirizzo, nome);
    }

    /// <summary>
    /// Normalizza un valore destinato a un'intestazione: sostituisce i caratteri di controllo
    /// con spazi, compatta gli spazi e limita la lunghezza a 100 caratteri, per impedire
    /// iniezioni di intestazioni e valori malformati.
    /// </summary>
    internal static string PulisciIntestazione(string valore)
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
    /// Prepara il nome da mostrare: toglie virgolette, barre rovesciate e caratteri di controllo,
    /// che romperebbero l'intestazione Reply-To.
    /// </summary>
    private static string PulisciNomeVisualizzato(string nome)
    {
        var caratteri = nome
            .Select(carattere => char.IsControl(carattere) || carattere is '"' or '\\' ? ' ' : carattere)
            .ToArray();
        var pulito = new string(caratteri).Trim();

        while (pulito.Contains("  ", StringComparison.Ordinal))
        {
            pulito = pulito.Replace("  ", " ", StringComparison.Ordinal);
        }

        if (pulito.Length > 100)
        {
            pulito = pulito[..100].TrimEnd();
        }

        return pulito;
    }
}
