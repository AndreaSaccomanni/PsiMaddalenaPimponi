// Riporta la pagina in cima dopo la navigazione Blazor, mantenendo i collegamenti alle sezioni.
Blazor.addEventListener("enhancedload", () => {
    if (!window.location.hash) {
        window.scrollTo({ top: 0, left: 0, behavior: "instant" });
    }

    document.querySelectorAll(".mobile-nav[open]").forEach((menu) => {
        menu.removeAttribute("open");
    });
});
