// Micro-interazioni della navigazione: header compatto allo scroll, indice attivo
// nella pagina dei temi e chiusura del menu mobile con Escape o clic esterno.
(() => {
    const intestazione = document.getElementById("site-header");
    const menu = document.querySelector(".mobile-nav");

    const chiudiMenu = () => menu?.removeAttribute("open");

    const aggiornaIntestazione = () => {
        intestazione?.classList.toggle("is-scrolled", window.scrollY > 8);
    };

    const navTemi = document.querySelector(".topic-nav");
    const sezioniTemi = navTemi
        ? [...document.querySelectorAll(".topic-row[id]")]
        : [];

    const aggiornaTemaAttivo = () => {
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

    const aggiorna = () => {
        aggiornaIntestazione();
        aggiornaTemaAttivo();
    };

    document.addEventListener("click", (evento) => {
        if (menu?.hasAttribute("open") && !menu.contains(evento.target)) {
            chiudiMenu();
        }
    });

    document.addEventListener("keydown", (evento) => {
        if (evento.key === "Escape") {
            chiudiMenu();
        }
    });

    window.addEventListener("scroll", aggiorna, { passive: true });
    window.addEventListener("resize", aggiorna, { passive: true });

    // Blazor riusa il documento: dopo ogni navigazione riallinea lo stato.
    Blazor.addEventListener("enhancedload", () => {
        chiudiMenu();
        aggiornaIntestazione();
        aggiornaTemaAttivo();
    });

    aggiorna();
})();
