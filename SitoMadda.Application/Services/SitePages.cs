namespace SitoMadda.Application.Services;

/// <summary>Descrizione di una pagina del sito usata per titolo, meta description e sitemap.</summary>
/// <param name="Percorso">Percorso relativo della pagina, con la barra iniziale.</param>
/// <param name="Titolo">Titolo mostrato nella scheda del browser e nei risultati di ricerca.</param>
/// <param name="Descrizione">Riassunto per i motori di ricerca e le anteprime sui social.</param>
/// <param name="Indicizzabile">Indica se la pagina va inclusa in sitemap e con canonical.</param>
public sealed record SitePage(
    string Percorso,
    string Titolo,
    string Descrizione,
    bool Indicizzabile = true);

/// <summary>
/// Raccoglie in un unico punto percorso, titolo e descrizione di ogni pagina: le pagine li
/// usano per i metadati e la sitemap li usa per elencare gli indirizzi pubblici, così non
/// possono divergere.
/// </summary>
public static class SitePages
{
    /// <summary>Pagina iniziale del sito.</summary>
    public static SitePage Home { get; } = new(
        "/",
        "Home · Maddalena Pimponi, psicologa",
        "Psicologa clinica dello sviluppo a Orvieto, Narni e online: consulenza e sostegno psicologico per pre-adolescenti, adolescenti e giovani adulti.");

    /// <summary>Presentazione e formazione della professionista.</summary>
    public static SitePage ChiSono { get; } = new(
        "/chi-sono",
        "Chi sono · Maddalena Pimponi",
        "Dott.ssa Maddalena Pimponi, psicologa clinica dello sviluppo e tutor dell'apprendimento: formazione, approccio e percorso professionale.");

    /// <summary>Aree di intervento e temi trattati.</summary>
    public static SitePage DiCosaMiOccupo { get; } = new(
        "/di-cosa-mi-occupo",
        "Di cosa mi occupo · Maddalena Pimponi",
        "Aree di intervento: risorse individuali, qualità della vita, ansia e stress, alimentazione, disagio emotivo, DSA e BES, infanzia e digitale.");

    /// <summary>Risposte alle domande più comuni sul percorso.</summary>
    public static SitePage DomandeFrequenti { get; } = new(
        "/domande-frequenti",
        "Domande frequenti · Maddalena Pimponi",
        "Risposte alle domande più comuni su primo colloquio, durata del percorso, riservatezza, appuntamenti e modalità online.");

    /// <summary>Modulo di contatto, recapiti diretti e sedi degli studi.</summary>
    public static SitePage Contatti { get; } = new(
        "/contatti",
        "Contatti e appuntamenti · Maddalena Pimponi",
        "Telefono, WhatsApp, email e modulo per un primo colloquio. Ricevo su appuntamento a Orvieto, a Narni e online.");

    /// <summary>Informativa sul trattamento dei dati personali.</summary>
    public static SitePage Privacy { get; } = new(
        "/privacy",
        "Informativa privacy · Maddalena Pimponi",
        "Informazioni sul trattamento dei dati inviati tramite il modulo di contatto.");

    /// <summary>Pagina mostrata per gli errori dell'applicazione; non va indicizzata.</summary>
    public static SitePage Errore { get; } = new(
        "/Error",
        "Errore · Maddalena Pimponi",
        "La richiesta non è stata completata.",
        Indicizzabile: false);

    /// <summary>Pagina mostrata per gli indirizzi inesistenti; non va indicizzata.</summary>
    public static SitePage PaginaNonTrovata { get; } = new(
        "/pagina-non-trovata",
        "Pagina non trovata · Maddalena Pimponi",
        "L'indirizzo richiesto non esiste oppure è stato spostato.",
        Indicizzabile: false);

    /// <summary>Pagine pubbliche da elencare nella sitemap, nell'ordine di navigazione.</summary>
    public static IReadOnlyList<SitePage> Pubbliche { get; } =
    [
        Home, ChiSono, DiCosaMiOccupo, DomandeFrequenti, Contatti, Privacy
    ];
}
