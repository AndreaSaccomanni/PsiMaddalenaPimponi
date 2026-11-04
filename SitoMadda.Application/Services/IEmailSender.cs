using SitoMadda.Application.Models;

namespace SitoMadda.Application.Services;

/// <summary>Definisce il servizio applicativo che inoltra le richieste di contatto via email.</summary>
public interface IEmailSender
{
    /// <summary>Invia alla psicologa i dati del modulo di contatto tramite email.</summary>
    Task InviaRichiestaContattoAsync(ContactRequest richiesta, CancellationToken tokenAnnullamento = default);
}
