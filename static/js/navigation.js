// Micro-interazioni del sito: header compatto allo scroll, indice attivo nella pagina dei
// temi, chiusura del menu mobile, ripristino dello scroll a ogni cambio pagina e comparsa
// progressiva dei blocchi. Lo script è autosufficiente: se non viene caricato, il sito
// resta comunque navigabile perché nessun contenuto dipende da queste classi.
(() => {
    const intestazione = document.getElementById("site-header");
    const menu = document.querySelector(".mobile-nav");

    /**
     * Chiude il menu mobile, se aperto. Il componente usa <details>, quindi basta togliere
     * l'attributo "open": la stessa funzione serve a Escape, al clic esterno e alla navigazione.
     */
    const chiudiMenu = () => menu?.removeAttribute("open");

    /**
     * Allinea la testata allo stato della pagina: aggiunge l'ombra quando si è scorso oltre
     * l'inizio e nasconde la bolla WhatsApp fissa sulla pagina contatti, dove coprirebbe i
     * campi del modulo su schermi piccoli.
     */
    const aggiornaIntestazione = () => {
        intestazione?.classList.toggle("is-scrolled", window.scrollY > 8);

        const percorso = window.location.pathname.replace(/\/+$/, "") || "/";
        // Vale sia alla radice del dominio sia sotto un sottopercorso (per esempio
        // l'indirizzo di progetto di GitHub Pages): conta solo la fine del percorso.
        document.body.classList.toggle("senza-float", /(^|\/)contatti$/.test(percorso));
    };

    /**
     * Evidenzia la pillola dell'indice temi corrispondente alla sezione visibile.
     * La ricerca degli elementi avviene a ogni chiamata: dopo le navigazioni Blazor il
     * documento viene sostituito e i riferimenti raccolti all'avvio sarebbero obsoleti.
     */
    const aggiornaTemaAttivo = () => {
        const navTemi = document.querySelector(".topic-nav");
        const sezioniTemi = navTemi ? [...document.querySelectorAll(".topic-row[id]")] : [];

        if (!navTemi || sezioniTemi.length === 0) {
            return;
        }

        const limite = window.innerHeight * 0.35;
        let attiva = sezioniTemi[0];

        for (const sezione of sezioniTemi) {
            if (sezione.getBoundingClientRect().top <= limite) {
                attiva = sezione;
            }
        }

        for (const collegamento of navTemi.querySelectorAll("a")) {
            collegamento.classList.toggle(
                "is-active",
                collegamento.getAttribute("href") === `#${attiva.id}`);
        }
    };

    // Lo scroll genera molti eventi: si aggiorna una sola volta per frame per non
    // forzare letture di layout ripetute e mantenere lo scorrimento fluido su telefono.
    let inCoda = false;

    /**
     * Pianifica l'aggiornamento di header e indice temi nel prossimo frame.
     * Se un aggiornamento è già in coda, le chiamate successive vengono ignorate.
     */
    const aggiorna = () => {
        if (inCoda) {
            return;
        }

        inCoda = true;
        requestAnimationFrame(() => {
            inCoda = false;
            aggiornaIntestazione();
            aggiornaTemaAttivo();
        });
    };

    /**
     * Riporta la vista all'inizio della pagina dopo un cambio di pagina; se l'URL contiene
     * un'ancora, porta invece la sezione richiesta sotto la testata. Lo scroll è impostato
     * su "instant" perché il CSS usa lo scorrimento fluido e un rientro animato da fondo
     * pagina risulterebbe lento e disorientante.
     */
    const portaInCimaOPAllAncora = () => {
        requestAnimationFrame(() => {
            const ancora = window.location.hash;
            if (ancora) {
                const destinazione = document.getElementById(decodeURIComponent(ancora.slice(1)));
                destinazione?.scrollIntoView({ block: "start" });
                return;
            }

            const radice = document.documentElement;
            const comportamentoPrecedente = radice.style.scrollBehavior;
            radice.style.scrollBehavior = "auto";
            window.scrollTo({ top: 0, left: 0, behavior: "instant" });
            setTimeout(() => {
                radice.style.scrollBehavior = comportamentoPrecedente;
            }, 0);
        });
    };

    // Blocchi che compaiono progressivamente: card, righe dei temi, FAQ, contatti e ritratti.
    const selettoriRivelazione = [
        ".paper-card", ".location-card", ".topic-row", ".timeline li",
        ".faq-group", ".contact-method", ".map-card", ".closing-cta",
        ".story-copy", ".portrait-circle", ".intro-text", ".map-details"
    ].join(",");

    let osservatore = null;

    /**
     * Prepara la comparsa dei blocchi che sono ancora sotto la piega.
     * Gli elementi già visibili restano tali (nessun lampeggio all'apertura), quelli più in
     * basso ricevono la classe "rivela" e vengono osservati: alla prima intersezione la
     * classe "is-visibile" avvia l'animazione, poi le classi vengono rimosse così hover,
     * focus e stampa tornano al comportamento normale. Con "prefers-reduced-motion" o senza
     * IntersectionObserver non viene nascosto nulla.
     */
    const preparaRivelazioni = () => {
        const movimentoRidotto = matchMedia("(prefers-reduced-motion: reduce)").matches;
        if (movimentoRidotto || !("IntersectionObserver" in window)) {
            return;
        }

        const sogliaVisibile = window.innerHeight * 0.94;
        const elementi = [...document.querySelectorAll(`main ${selettoriRivelazione}`)]
            .filter(elemento => !elemento.classList.contains("rivela") && !elemento.closest(".topic-nav"));

        osservatore ??= new IntersectionObserver((voci) => {
            for (const voce of voci) {
                if (!voce.isIntersecting) {
                    continue;
                }

                voce.target.classList.add("is-visibile");
                osservatore.unobserve(voce.target);
                setTimeout(() => {
                    voce.target.classList.remove("rivela", "is-visibile");
                    voce.target.style.transitionDelay = "";
                }, 900);
            }
        }, { threshold: 0.12, rootMargin: "0px 0px -6% 0px" });

        elementi.forEach((elemento, indice) => {
            if (elemento.getBoundingClientRect().top <= sogliaVisibile) {
                return;
            }

            elemento.classList.add("rivela");
            elemento.style.transitionDelay = `${Math.min(indice % 4, 3) * 70}ms`;
            osservatore.observe(elemento);
        });

        // Rete di sicurezza: se l'osservatore non scatta, il contenuto non resta nascosto.
        setTimeout(() => {
            document.querySelectorAll(".rivela:not(.is-visibile)").forEach((elemento) => {
                const riquadro = elemento.getBoundingClientRect();
                if (riquadro.top < window.innerHeight * 1.2) {
                    elemento.classList.remove("rivela", "is-visibile");
                    elemento.style.transitionDelay = "";
                }
            });
        }, 4000);
    };

    // Un clic fuori dal menu mobile lo chiude, senza intercettare i clic interni ai link.
    document.addEventListener("click", (evento) => {
        if (menu?.hasAttribute("open") && !menu.contains(evento.target)) {
            chiudiMenu();
        }
    });

    // Escape chiude il menu: è il comportamento atteso da un pannello a comparsa.
    document.addEventListener("keydown", (evento) => {
        if (evento.key === "Escape") {
            chiudiMenu();
        }
    });

    window.addEventListener("scroll", aggiorna, { passive: true });
    window.addEventListener("resize", aggiorna, { passive: true });

    /**
     * Blazor sostituisce il contenuto della pagina senza ricaricare il documento:
     * a ogni navigazione si riallineano menu, header, indice temi, posizione di scroll e
     * osservatore delle animazioni (quello precedente osserverebbe nodi rimossi).
     */
    // Versione statica: non c'è Blazor ad aggiornare la pagina, quindi l'avvio avviene
    // una sola volta. Se lo script venisse caricato dentro un documento con Blazor
    // (per esempio durante una migrazione) il ramo resta comunque sicuro.
    if (window.Blazor?.addEventListener) {
        Blazor.addEventListener("enhancedload", () => {
            chiudiMenu();
            aggiornaIntestazione();
            aggiornaTemaAttivo();
            portaInCimaOPAllAncora();

            osservatore?.disconnect();
            osservatore = null;
            preparaRivelazioni();
        });
    }

    // Anno del copyright: evita di doverlo aggiornare nel markup ogni gennaio.
    const anno = document.getElementById("anno-corrente");
    if (anno) {
        anno.textContent = String(new Date().getFullYear());
    }

    aggiorna();
    preparaRivelazioni();
})();
