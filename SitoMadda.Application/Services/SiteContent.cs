using SitoMadda.Domain.Models;

namespace SitoMadda.Application.Services;

/// <summary>Raccoglie i recapiti pubblici e le sedi mostrati nelle diverse pagine del sito.</summary>
public sealed class SiteContent
{
    /// <summary>Nome e qualifica professionale da mostrare nelle comunicazioni del sito.</summary>
    public string NomeCompleto => "Dott.ssa Maddalena Pimponi";

    /// <summary>Indirizzo email pubblico della psicologa.</summary>
    public string IndirizzoEmail => "psi.maddalenapimponi@gmail.com";

    /// <summary>Numero di telefono nel formato leggibile dalle persone che visitano il sito.</summary>
    public string TelefonoVisualizzato => "371 535 2853";

    /// <summary>Collegamento telefonico utilizzato dai dispositivi mobili.</summary>
    public string CollegamentoTelefono => "tel:+393715352853";

    /// <summary>Collegamento diretto alla chat WhatsApp della psicologa.</summary>
    public string CollegamentoWhatsApp => "https://wa.me/393715352853";

    /// <summary>Collegamento al profilo Instagram professionale.</summary>
    public string CollegamentoInstagram => "https://www.instagram.com/psi.maddalenapimponi/";

    /// <summary>Collegamento al profilo professionale su MioDottore indicato nella bio di Instagram.</summary>
    public string CollegamentoMioDottore => "https://www.miodottore.it/profilo/maddalena-pimponi";

    /// <summary>Elenco degli studi in cui la psicologa riceve su appuntamento.</summary>
    public IReadOnlyList<StudioLocation> Studi { get; } =
    [
        new("Orvieto", "Centro Diagnostico Eolo", "Piazza del Fanello, 27", "Orvieto (TR)"),
        new("Narni", "SALUS Poliambulatorio medico e fisioterapico", "Via Tuderte, 428/A", "Narni (TR)")
    ];
}
