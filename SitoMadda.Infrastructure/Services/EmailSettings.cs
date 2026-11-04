namespace SitoMadda.Infrastructure.Services;

/// <summary>Contiene i parametri SMTP letti dalla sezione Email della configurazione.</summary>
public sealed class EmailSettings
{
    /// <summary>Nome della sezione di configurazione che contiene i parametri SMTP.</summary>
    public const string SectionName = "Email";

    /// <summary>Nome host del server SMTP usato per spedire le email.</summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>Porta di connessione del server SMTP.</summary>
    public int SmtpPort { get; set; }

    /// <summary>Indirizzo dell'account mittente SMTP.</summary>
    public string SenderAddress { get; set; } = string.Empty;

    /// <summary>Password o password per app dell'account mittente, fornita tramite User Secrets.</summary>
    public string SenderPassword { get; set; } = string.Empty;

    /// <summary>Indirizzo che riceve le richieste inviate dal modulo.</summary>
    public string RecipientAddress { get; set; } = string.Empty;

    /// <summary>Abilita la connessione SMTP protetta con TLS.</summary>
    public bool UseSsl { get; set; } = true;
}
