namespace SitoMadda.Domain.Models;

/// <summary>Rappresenta uno studio e fornisce i collegamenti alla relativa mappa Google.</summary>
public sealed record StudioLocation(string Nome, string Struttura, string Indirizzo, string Citta)
{
    /// <summary>Restituisce l'indirizzo completo da usare nelle mappe.</summary>
    public string IndirizzoCompleto => $"{Indirizzo}, {Citta}";

    /// <summary>Genera il collegamento per aprire lo studio nella ricerca di Google Maps.</summary>
    public string UrlRicercaMappa =>
        $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(IndirizzoCompleto)}";

    /// <summary>Genera l'indirizzo della mappa incorporabile nella pagina contatti.</summary>
    public string UrlMappaIncorporata =>
        $"https://www.google.com/maps?q={Uri.EscapeDataString(IndirizzoCompleto)}&output=embed";
}
