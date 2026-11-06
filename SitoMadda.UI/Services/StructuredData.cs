using System.Text.Json;
using SitoMadda.Application.Services;
using SitoMadda.Domain.Models;

namespace SitoMadda.UI.Services;

/// <summary>
/// Costruisce i dati strutturati schema.org pubblicati nel documento: servono ai motori di
/// ricerca per collegare il sito alla professionista, ai recapiti e alle sedi reali.
/// </summary>
public static class StructuredData
{
    private static readonly JsonSerializerOptions OpzioniJson = new()
    {
        // L'encoder predefinito trasforma < > & in sequenze \uXXXX: il JSON resta valido
        // anche se un giorno un valore contenesse il carattere di chiusura di uno script.
        WriteIndented = false
    };

    /// <summary>
    /// Restituisce il blocco JSON-LD con i dati pubblici della professionista e degli studi.
    /// </summary>
    /// <param name="indirizzoBase">Indirizzo assoluto del sito, con la barra finale.</param>
    /// <param name="contenuti">Recapiti e sedi già usati dalle pagine.</param>
    /// <returns>JSON serializzato, pronto da inserire in un elemento script.</returns>
    public static string PerSito(string indirizzoBase, SiteContent contenuti)
    {
        var baseSito = indirizzoBase.TrimEnd('/');

        var dati = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Psychologist",
            ["name"] = contenuti.NomeCompleto,
            ["description"] = SitePages.Home.Descrizione,
            ["url"] = baseSito + "/",
            ["image"] = baseSito + "/images/maddalena-ritratto.jpg",
            ["telephone"] = contenuti.CollegamentoTelefono.Replace("tel:", string.Empty, StringComparison.Ordinal),
            ["email"] = contenuti.IndirizzoEmail,
            ["areaServed"] = new[] { "Orvieto", "Narni" },
            ["location"] = contenuti.Studi.Select(PlaceDaStudio).ToArray()
        };

        return JsonSerializer.Serialize(dati, OpzioniJson);
    }

    /// <summary>Converte uno studio del sito in un oggetto schema.org di tipo Place.</summary>
    private static Dictionary<string, object?> PlaceDaStudio(StudioLocation studio)
    {
        var (localita, provincia) = SeparaCitta(studio.Citta);

        return new Dictionary<string, object?>
        {
            ["@type"] = "Place",
            ["name"] = studio.Struttura,
            ["address"] = new Dictionary<string, object?>
            {
                ["@type"] = "PostalAddress",
                ["streetAddress"] = studio.Indirizzo,
                ["addressLocality"] = localita,
                ["addressRegion"] = provincia,
                ["addressCountry"] = "IT"
            }
        };
    }

    /// <summary>
    /// Divide una località scritta come "Orvieto (TR)" in nome della città e sigla della
    /// provincia. Se il formato non è riconosciuto restituisce il testo intero come città.
    /// </summary>
    private static (string Localita, string Provincia) SeparaCitta(string citta)
    {
        var indice = citta.LastIndexOf(" (", StringComparison.Ordinal);

        if (indice < 0 || !citta.EndsWith(')'))
        {
            return (citta.Trim(), string.Empty);
        }

        return (citta[..indice].Trim(), citta[(indice + 2)..^1].Trim());
    }
}
