using System.ComponentModel.DataAnnotations;

namespace SitoMadda.Application.Models;

/// <summary>Contiene i dati validati inviati dal modulo di contatto.</summary>
public sealed class ContactRequest
{
    /// <summary>Nome della persona che invia la richiesta.</summary>
    [Required(ErrorMessage = "Inserisci il tuo nome.")]
    [StringLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri.")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>Indirizzo email da usare per rispondere alla persona.</summary>
    [Required(ErrorMessage = "Inserisci il tuo indirizzo email.")]
    [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Numero di telefono facoltativo per un ricontatto alternativo.</summary>
    [Phone(ErrorMessage = "Inserisci un numero di telefono valido.")]
    [StringLength(30)]
    public string? Telefono { get; set; }

    /// <summary>Argomento scelto nel modulo, se specificato.</summary>
    [StringLength(100)]
    public string? Argomento { get; set; }

    /// <summary>Testo della richiesta inviato dalla persona.</summary>
    [Required(ErrorMessage = "Scrivi un breve messaggio.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Il messaggio deve contenere da 10 a 4000 caratteri.")]
    public string Messaggio { get; set; } = string.Empty;

    /// <summary>Indica se la persona ha confermato la lettura dell'informativa privacy.</summary>
    public bool PrivacyAccettata { get; set; }
}
