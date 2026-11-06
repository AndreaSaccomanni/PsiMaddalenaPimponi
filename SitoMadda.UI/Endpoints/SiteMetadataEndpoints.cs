using System.Text;
using System.Xml.Linq;
using SitoMadda.Application.Services;

namespace SitoMadda.UI.Endpoints;

/// <summary>
/// Espone gli indirizzi che i motori di ricerca e i sistemi di monitoraggio si aspettano:
/// robots.txt, sitemap.xml e health check. Non sono pagine Blazor, quindi non passano dal
/// rendering dei componenti.
/// </summary>
public static class SiteMetadataEndpoints
{
    /// <summary>Registra gli endpoint di servizio del sito.</summary>
    /// <param name="rotte">Mappa degli endpoint dell'applicazione.</param>
    /// <returns>La stessa mappa, per concatenare altre registrazioni.</returns>
    public static IEndpointRouteBuilder MapSiteMetadataEndpoints(this IEndpointRouteBuilder rotte)
    {
        rotte.MapGet("/robots.txt", (HttpContext contesto, IConfiguration configurazione) =>
        {
            var baseSito = IndirizzoBase(contesto, configurazione);

            var testo = new StringBuilder()
                .AppendLine("User-agent: *")
                .AppendLine("Allow: /")
                .AppendLine("Disallow: /Error")
                .AppendLine("Disallow: /pagina-non-trovata")
                .AppendLine("Disallow: /health")
                .AppendLine()
                .AppendLine($"Sitemap: {baseSito}/sitemap.xml")
                .ToString();

            ImpostaCache(contesto, "public, max-age=86400");
            return Results.Text(testo, "text/plain; charset=utf-8");
        });

        rotte.MapGet("/sitemap.xml", (HttpContext contesto, IConfiguration configurazione) =>
        {
            var baseSito = IndirizzoBase(contesto, configurazione);
            XNamespace schema = "http://www.sitemaps.org/schemas/sitemap/0.9";

            var documento = new XDocument(
                new XElement(schema + "urlset",
                    SitePages.Pubbliche.Select(pagina =>
                        new XElement(schema + "url",
                            new XElement(schema + "loc", baseSito + pagina.Percorso)))));

            ImpostaCache(contesto, "public, max-age=86400");
            return Results.Text(documento.ToString(), "application/xml; charset=utf-8");
        });

        return rotte;
    }

    /// <summary>
    /// Indirizzo assoluto del sito: usa l'eventuale valore configurato e, in mancanza, quello
    /// con cui il visitatore ha raggiunto il sito. Così finché il dominio non esiste non serve
    /// nessun valore da aggiornare.
    /// </summary>
    private static string IndirizzoBase(HttpContext contesto, IConfiguration configurazione)
    {
        var configurato = configurazione["Site:PublicBaseUrl"];

        return string.IsNullOrWhiteSpace(configurato)
            ? $"{contesto.Request.Scheme}://{contesto.Request.Host}"
            : configurato.TrimEnd('/');
    }

    /// <summary>
    /// Consente ai proxy e ai motori di ricerca di conservare la risposta: cambia solo al
    /// variare delle pagine pubblicate.
    /// </summary>
    private static void ImpostaCache(HttpContext contesto, string valore) =>
        contesto.Response.Headers.CacheControl = valore;
}
