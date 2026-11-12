# Convenzioni del progetto

- Il sito è statico: il codice pubblicato sta in `static/` (HTML, CSS, JavaScript). Non ci sono progetti .NET, build o dipendenze.
- `README.md` è la guida unica del progetto. Va aggiornato **ogni volta** che una modifica cambia il funzionamento del sito: pagine, modulo contatti, mappe, WhatsApp, metadati, pubblicazione o dominio. Aggiungi anche una riga in "Cronologia delle funzionalità".
- Le modifiche solo estetiche (colori, spaziature, immagini decorative) non richiedono una riga di cronologia, ma se cambiano il modo in cui si modificano i contenuti il README va comunque allineato.
- Il README è scritto per una persona senza formazione tecnica: i termini tecnici vanno spiegati, non solo citati.
- Rami: `develop` per il lavoro, `main` per la versione pubblicata. Ogni push su `main` pubblica tramite `.github/workflows/pages.yml`.
- Commit e merge da `develop` a `main` li gestisce il proprietario del repository. Non aprire pull request senza una richiesta esplicita.
