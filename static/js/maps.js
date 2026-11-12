// Le mappe di Google restano spente finché il visitatore non le attiva: nessuna richiesta
// a Google prima di quel clic, come nella versione server del sito.
(() => {
    for (const segnaposto of document.querySelectorAll(".map-placeholder[data-mappa-src]")) {
        segnaposto.addEventListener("click", () => {
            const mappa = document.createElement("iframe");
            mappa.src = segnaposto.dataset.mappaSrc;
            mappa.title = segnaposto.dataset.mappaTitolo || "Mappa dello studio";
            mappa.loading = "lazy";
            mappa.referrerPolicy = "no-referrer-when-downgrade";
            mappa.allowFullscreen = true;
            segnaposto.replaceWith(mappa);
        });
    }
})();
