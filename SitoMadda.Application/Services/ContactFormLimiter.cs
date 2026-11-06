namespace SitoMadda.Application.Services;

/// <summary>
/// Limita quante richieste di contatto possono partire in un intervallo di tempo, per singolo
/// visitatore e per l'intero sito. È pensato per essere registrato come singleton: lo stato
/// vive in memoria e si azzera al riavvio dell'applicazione, sufficiente per fermare invii
/// ripetuti automatici.
/// </summary>
/// <remarks>
/// Con il rendering interattivo di Blazor il modulo viene inviato sul circuito SignalR e non
/// passa da un endpoint HTTP: il rate limiter di ASP.NET Core non lo vedrebbe. Per questo il
/// controllo è fatto qui, nel servizio richiamato dal componente.
/// </remarks>
public sealed class ContactFormLimiter
{
    private readonly int _massimoPerFinestra;
    private readonly int _massimoGlobale;
    private readonly TimeSpan _finestra;
    private readonly TimeSpan _intervalloMinimo;
    private readonly object _sincronizzazione = new();
    private readonly Dictionary<string, Queue<DateTimeOffset>> _inviiPerChiave = new(StringComparer.Ordinal);
    private readonly Queue<DateTimeOffset> _inviiGlobali = new();
    private DateTimeOffset _prossimaPulizia;

    /// <summary>Configura le soglie del limite di invio.</summary>
    /// <param name="massimoPerFinestra">Invii consentiti per la stessa chiave nella finestra.</param>
    /// <param name="finestraMinuti">Durata della finestra, in minuti.</param>
    /// <param name="intervalloMinimoSecondi">Attesa minima fra due invii della stessa chiave.</param>
    /// <param name="massimoGlobale">Invii consentiti a tutto il sito in un'ora.</param>
    public ContactFormLimiter(
        int massimoPerFinestra = 3,
        int finestraMinuti = 10,
        int intervalloMinimoSecondi = 20,
        int massimoGlobale = 30)
    {
        _massimoPerFinestra = Math.Max(1, massimoPerFinestra);
        _massimoGlobale = Math.Max(1, massimoGlobale);
        _finestra = TimeSpan.FromMinutes(Math.Max(1, finestraMinuti));
        _intervalloMinimo = TimeSpan.FromSeconds(Math.Max(0, intervalloMinimoSecondi));
        _prossimaPulizia = DateTimeOffset.UtcNow.Add(_finestra);
    }

    /// <summary>
    /// Verifica se una richiesta può partire e, in caso affermativo, la registra.
    /// </summary>
    /// <param name="chiave">Identificativo del visitatore, ad esempio il suo indirizzo IP.</param>
    /// <param name="adesso">Istante corrente, passato dall'esterno per rendere il test deterministico.</param>
    /// <param name="attesa">Tempo minimo prima di poter riprovare; zero se l'invio è consentito.</param>
    /// <returns><see langword="true"/> se il messaggio può essere inviato.</returns>
    public bool Consenti(string chiave, DateTimeOffset adesso, out TimeSpan attesa)
    {
        lock (_sincronizzazione)
        {
            Ripulisci(adesso);

            // Limite globale: protegge la casella email anche se arrivano molte connessioni diverse.
            if (_inviiGlobali.Count >= _massimoGlobale)
            {
                attesa = _inviiGlobali.Peek() + TimeSpan.FromHours(1) - adesso;
                return false;
            }

            if (!_inviiPerChiave.TryGetValue(chiave, out var invii))
            {
                invii = new Queue<DateTimeOffset>();
                _inviiPerChiave[chiave] = invii;
            }

            // Intervallo minimo: ferma il doppio clic e gli invii in raffica dalla stessa sessione.
            if (invii.Count > 0 && adesso - invii.Last() < _intervalloMinimo)
            {
                attesa = _intervalloMinimo - (adesso - invii.Last());
                return false;
            }

            // Finestra: massimo numero di invii per la stessa chiave.
            if (invii.Count >= _massimoPerFinestra)
            {
                attesa = invii.Peek() + _finestra - adesso;
                return false;
            }

            invii.Enqueue(adesso);
            _inviiGlobali.Enqueue(adesso);
            attesa = TimeSpan.Zero;
            return true;
        }
    }

    /// <summary>
    /// Elimina le registrazioni più vecchie della finestra e le chiavi ormai inattive.
    /// Viene eseguita al massimo una volta per finestra per non appesantire ogni invio.
    /// </summary>
    private void Ripulisci(DateTimeOffset adesso)
    {
        if (adesso < _prossimaPulizia)
        {
            return;
        }

        _prossimaPulizia = adesso + _finestra;

        while (_inviiGlobali.Count > 0 && adesso - _inviiGlobali.Peek() > TimeSpan.FromHours(1))
        {
            _inviiGlobali.Dequeue();
        }

        foreach (var chiave in _inviiPerChiave.Keys.ToArray())
        {
            var invii = _inviiPerChiave[chiave];

            while (invii.Count > 0 && adesso - invii.Peek() > _finestra)
            {
                invii.Dequeue();
            }

            if (invii.Count == 0)
            {
                _inviiPerChiave.Remove(chiave);
            }
        }
    }
}
