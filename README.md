# Sito Maddalena Pimponi

Sito vetrina della Dott.ssa Maddalena Pimponi, psicologa clinica dello sviluppo a Orvieto, Narni e online.

Il sito è fatto solo di file HTML, CSS e JavaScript nella cartella `static/`. Non serve installare nulla: non ci sono .NET, server, database o build. Il sito pubblicato è quello che GitHub Pages serve da `static/`.

> **Regola:** quando una modifica cambia il *funzionamento* del sito (non solo l'aspetto), aggiorna la sezione interessata e aggiungi una riga nella [cronologia](#cronologia-delle-funzionalità).

---

## Rami e pubblicazione

| Ramo | Uso |
|---|---|
| `develop` | Lavoro quotidiano e prove in locale. |
| `main` | Versione pubblicata. |

- Ogni push su `main` pubblica automaticamente il contenuto di `static/` tramite il workflow `.github/workflows/pages.yml`.
- I merge da `develop` a `main` li fa il proprietario del repository. Quindi la modifica va su `develop`, e diventa pubblica solo quando viene unita a `main`.
- Il sito è raggiungibile su `https://psicologamaddalenapimponi.it/`.

---

## Come si modifica

1. Lavora sul ramo `develop`.
2. Modifica i file in `static/`:

| Cosa | Dove |
|---|---|
| Pagine | `static/index.html`, `static/chi-sono/`, `static/di-cosa-mi-occupo/`, `static/domande-frequenti/`, `static/contatti/`, `static/privacy/`, `static/404.html` |
| Stili | `static/site.css` |
| Script | `static/js/navigation.js` (menu e comportamento), `static/js/form.js` (modulo contatti), `static/js/maps.js` (mappe) |
| Immagini e font | `static/images/`, `static/fonts/`, `static/favicon.*`, `static/apple-touch-icon.png` |
| SEO | `static/sitemap.xml`, `static/robots.txt`, dati strutturati JSON-LD in ogni pagina |
| Dominio | `static/CNAME` (`psicologamaddalenapimponi.it`) |

3. Controlla le modifiche aprendo le pagine in un browser.
4. Fai commit e push su `develop`. Il sito pubblico non cambia finché il proprietario non unisce `develop` in `main`.

---

## Cosa fa il sito

### Le pagine

| Indirizzo | Contenuto |
|---|---|
| `/` | Home: presentazione, ritratto, collegamenti rapidi |
| `/chi-sono/` | Percorso professionale, formazione e approccio |
| `/di-cosa-mi-occupo/` | Aree di intervento |
| `/domande-frequenti/` | Domande frequenti sul primo colloquio e sul percorso |
| `/contatti/` | Modulo, telefono, WhatsApp, email, sedi e mappe |
| `/privacy/` | Informativa sul trattamento dei dati |
| `/404.html` | Pagina per indirizzi inesistenti |

### Modulo contatti

- Campi: nome, email, telefono facoltativo, argomento, messaggio. Il consenso privacy è obbligatorio.
- Il campo nascosto `website` è una trappola per i robot: se viene compilato, il modulo non invia nulla.
- Se la costante `ENDPOINT` in `static/js/form.js` è vuota, il modulo apre il programma di posta con il messaggio già compilato verso la casella della professionista.
- Se `ENDPOINT` contiene l'indirizzo di un servizio di inoltro (per esempio Formspree), il messaggio parte direttamente dal browser.

### Mappe e WhatsApp

- Le mappe Google non vengono caricate finché il visitatore non preme "Mostra la mappa": prima di quel clic non parte nessuna richiesta a Google.
- Il pulsante WhatsApp flottante compare in basso a destra su tutte le pagine che non sono marcate con `senza-float` nel tag `<body>`. La pagina `/contatti/` lo nasconde perché ha già il pulsante WhatsApp nella propria sezione.

### Dominio e DNS

Il dominio personalizzato è impostato in **Settings → Pages → Custom domain** del repository GitHub. Con il workflow GitHub il file `static/CNAME` non basta da solo.

| Nome | Tipo | Valore |
|---|---|---|
| `@` | A | `185.199.108.153` |
| `@` | A | `185.199.109.153` |
| `@` | A | `185.199.110.153` |
| `@` | A | `185.199.111.153` |
| `www` | CNAME | `andreassaccomanni.github.io` |

Un dominio può stare su un solo host: non lasciare record `AAAA` o record verso altri servizi.

---

## Cronologia delle funzionalità

- **Versione statica su GitHub Pages**: il sito è riscritto come file statici; il ramo `main` pubblica con il workflow `pages.yml`. La versione Blazor/Render resta nella storia Git (commit `e159a9c`).
- **Mappe su clic**: le mappe Google si caricano solo quando il visitatore le attiva.
- **Pulsante WhatsApp**: flottante in basso a destra su tutte le pagine tranne `/contatti/`.

---

## Per chi lavora sul codice

- Nessuna dipendenza: il sito è HTML, CSS e JavaScript puri.
- Il JavaScript è in `static/js/`, caricato a fine pagina in ogni HTML.
- Per l'anteprima locale basta aprire i file in `static/` con un server statico qualsiasi.
