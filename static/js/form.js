// Modulo di contatto della versione statica del sito.
//
// GitHub Pages serve solo file: non esiste un server che invii l'email. Il modulo può
// appoggiarsi a un servizio di inoltro (Formspree, Getform, Basin...) impostando ENDPOINT
// qui sotto: basta l'indirizzo fornito dal servizio, nessuna chiave da nascondere perché è
// pensato per il browser. Se ENDPOINT resta vuoto il modulo apre il programma di posta con
// il messaggio già compilato, così il sito funziona comunque senza servizi esterni.
(() => {
    const ENDPOINT = "";
    const EMAIL = "psi.maddalenapimponi@gmail.com";

    const modulo = document.getElementById("modulo-contatti");
    if (!modulo) {
        return;
    }

    const stato = modulo.querySelector(".form-status");
    const bottone = modulo.querySelector('button[type="submit"]');
    const testoBottone = bottone.innerHTML;

    /** Mostra un esito sotto il pulsante e riporta il focus sul messaggio. */
    const mostraStato = (testo, esito) => {
        stato.hidden = false;
        stato.textContent = testo;
        stato.className = `form-status ${esito}`;
    };

    /** Scrive l'errore accanto al campo e lo segnala anche agli screen reader. */
    const segnala = (campo, testo) => {
        const errore = modulo.querySelector(`.form-error[data-per="${campo.name}"]`);
        if (errore) {
            errore.textContent = testo;
        }
        campo.setAttribute("aria-invalid", testo ? "true" : "false");
        return !testo;
    };

    const valido = () => {
        const nome = modulo.elements.nome;
        const email = modulo.elements.email;
        const messaggio = modulo.elements.messaggio;
        const consenso = modulo.elements.consenso;

        const ok = [
            segnala(nome, nome.value.trim().length >= 2 ? "" : "Scrivi nome e cognome."),
            segnala(email, /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(email.value.trim()) ? "" : "Controlla l'indirizzo email."),
            segnala(messaggio, messaggio.value.trim().length >= 10 ? "" : "Aggiungi qualche riga sul motivo del contatto."),
            segnala(consenso, consenso.checked ? "" : "Serve il consenso per essere ricontattato/a."),
        ];

        return ok.every(Boolean);
    };

    const corpo = () => {
        const righe = [
            `Nome: ${modulo.elements.nome.value.trim()}`,
            `Email: ${modulo.elements.email.value.trim()}`,
            `Telefono: ${modulo.elements.telefono.value.trim() || "non indicato"}`,
            `Argomento: ${modulo.elements.argomento.value || "non indicato"}`,
            "",
            modulo.elements.messaggio.value.trim(),
        ];
        return righe.join("\n");
    };

    modulo.addEventListener("submit", async (evento) => {
        evento.preventDefault();
        stato.hidden = true;

        // Campo esca: compilato significa bot. Si risponde come per un invio riuscito.
        if (modulo.elements.website.value) {
            modulo.reset();
            mostraStato("Messaggio inviato correttamente. Grazie per avermi scritto.", "success");
            return;
        }

        if (!valido()) {
            modulo.querySelector('[aria-invalid="true"]')?.focus();
            return;
        }

        if (!ENDPOINT) {
            const oggetto = "Richiesta dal sito";
            const indirizzo = `mailto:${EMAIL}?subject=${encodeURIComponent(oggetto)}&body=${encodeURIComponent(corpo())}`;
            window.location.href = indirizzo;
            mostraStato("Si sta aprendo il tuo programma di posta con il messaggio già compilato: controllalo e premi Invia.", "success");
            return;
        }

        bottone.disabled = true;
        bottone.textContent = "Invio in corso…";

        try {
            const risposta = await fetch(ENDPOINT, {
                method: "POST",
                headers: { "Content-Type": "application/json", Accept: "application/json" },
                body: JSON.stringify({
                    nome: modulo.elements.nome.value.trim(),
                    email: modulo.elements.email.value.trim(),
                    telefono: modulo.elements.telefono.value.trim(),
                    argomento: modulo.elements.argomento.value,
                    messaggio: modulo.elements.messaggio.value.trim(),
                }),
            });

            if (!risposta.ok) {
                throw new Error(`Risposta ${risposta.status}`);
            }

            modulo.reset();
            mostraStato("Messaggio inviato correttamente. Grazie per avermi scritto.", "success");
        } catch {
            mostraStato(`Non è stato possibile inviare il messaggio. Scrivi direttamente a ${EMAIL}.`, "error");
        } finally {
            bottone.disabled = false;
            bottone.innerHTML = testoBottone;
        }
    });
})();
