# Sito Maddalena Pimponi

Sito vetrina della Dott.ssa Maddalena Pimponi, psicologa clinica dello sviluppo a Orvieto, Narni e online. È una **Blazor Web App** in **C#/.NET 8**: le pagine sono scritte in Razor (HTML con piccoli blocchi di codice C#) e l'unica azione che modifica qualcosa è l'invio di un'email dal modulo contatti. Non ci sono database, account utente, area riservata o API.

> **Questo file è la guida del progetto.** È incluso nella soluzione Visual Studio (cartella *Solution Items*): doppio clic su `README.md` in Esplora soluzioni per aprirlo, tasto destro → *Anteprima* per leggerlo formattato.
>
> **Regola:** quando una modifica cambia il *funzionamento* del sito (non solo l'aspetto), aggiorna la sezione interessata e aggiungi una riga nella [cronologia](#cronologia-delle-funzionalità).

---

## In due parole

Una persona che cerca una psicologa arriva sul sito, legge chi è e come lavora, e può scrivere. Il messaggio parte come email verso la casella della professionista: il sito non conserva nulla di suo. Il resto è contenuto, immagini e attenzione alla privacy di chi visita.

---

## Come avviarlo in locale

1. Apri `SitoMadda.sln` in Visual Studio.
2. Imposta `SitoMadda.UI` come progetto di avvio e scegli il profilo **http**.
3. Avvia: si apre il browser su `http://localhost:5250/`.

Da terminale, in alternativa:

```powershell
dotnet run --project .\SitoMadda.UI
```

Perché il modulo invii davvero le email in locale servono le credenziali SMTP negli *User Secrets* (vedi [Email](#email-come-funziona-linvio)). Senza credenziali il sito funziona lo stesso e il modulo mostra l'indirizzo email diretto.

---

## Cosa sa fare il sito

### Le pagine

| Indirizzo | Contenuto |
|---|---|
| `/` | Home: presentazione, ritratto, collegamenti rapidi |
| `/chi-sono` | Percorso professionale, formazione e approccio |
| `/di-cosa-mi-occupo` | Aree di intervento con navigazione adesiva fra i temi |
| `/domande-frequenti` | FAQ a fisarmonica sul primo colloquio e sul percorso |
| `/contatti` | Modulo, telefono, WhatsApp, email, MioDottore, sedi e mappe |
| `/privacy` | Informativa sul trattamento dei dati (bozza da completare) |
| `/pagina-non-trovata` | Pagina 404 per indirizzi inesistenti |
| `/Error` | Pagina di errore dell'applicazione |

Titolo, descrizione e percorso di ogni pagina pubblica sono definiti in un unico posto (`SitePages.cs`): da lì nascono anche i metadati per i motori di ricerca e la sitemap, quindi non possono divergere.

### Navigazione e comportamento

- Header che si compatta allo scorrimento, voce attiva evidenziata, menu per telefono con chiusura con tasto Esc o clic fuori.
- Animazioni leggere all'ingresso dei blocchi, disattivate per chi ha impostato "riduci movimento" nel sistema operativo.
- Link "Vai al contenuto" per chi naviga da tastiera.
- Pulsante WhatsApp sempre visibile in basso a destra.

### Modulo contatti

- Campi: nome, email, telefono facoltativo, argomento, messaggio. Validazione lato server con messaggi in italiano.
- Consenso privacy obbligatorio prima dell'invio.
- **Campo esca** invisibile: se un programma automatico lo compila, il messaggio viene scartato senza inviare email e senza dirlo al bot.
- **Limite di invii**: 3 messaggi ogni 10 minuti per visitatore, almeno 20 secondi fra due invii, massimo 30 all'ora per tutto il sito. Oltre il limite compare un messaggio con i minuti di attesa e l'indirizzo email diretto.
- Messaggi chiari in caso di problemi: se l'invio non è configurato o fallisce, il visitatore legge l'alternativa concreta (scrivere all'indirizzo email) invece di un errore tecnico.

### Email: come funziona l'invio

Il modulo consegna il messaggio a un servizio di posta. L'email arriva a `psi.maddalenapimponi@gmail.com`, con l'indirizzo di chi ha scritto impostato come **Rispondi a**: premendo "Rispondi" nella casella si risponde direttamente al visitatore.

Ci sono due canali, scelti automaticamente all'avvio:

| Canale | Quando si usa | Come si configura |
|---|---|---|
| **Resend** (API HTTPS) | In produzione, perché il piano free di Render blocca le connessioni SMTP in uscita | `RESEND_API_KEY` (e `RESEND_FROM`) |
| **SMTP Gmail** | In sviluppo, sul computer | User Secrets `Email:SenderAddress` e `Email:SenderPassword` |

Basta che sia presente una chiave Resend perché il sito usi quel canale; senza chiave resta SMTP. Il messaggio inviato è identico, quindi non cambia nulla per chi lo riceve.

Per attivare l'invio in locale:

1. Nell'account Google che spedisce, attiva la verifica in due passaggi e genera una [password per le app](https://myaccount.google.com/apppasswords) (non usare la password normale).
2. Salva i valori negli User Secrets, che restano sul tuo computer e non finiscono nella repository:

```powershell
dotnet user-secrets set "Email:SenderAddress" "ACCOUNT-GMAIL-MITTENTE@gmail.com" --project "C:\Users\andre\source\repos\PsiMaddalenaPimponi.git\SitoMadda.UI"
dotnet user-secrets set "Email:SenderPassword" "PASSWORD-PER-LE-APP" --project "C:\Users\andre\source\repos\PsiMaddalenaPimponi.git\SitoMadda.UI"
```

3. Riavvia l'applicazione e prova il modulo.

Per la produzione con Resend:

1. Crea un account su [resend.com](https://resend.com) (piano free: 100 email al giorno) e genera una chiave API.
2. Senza un dominio verificato, Resend accetta solo l'indirizzo dell'account come destinatario: va bene per provare, non per il sito pubblico.
3. Dopo aver registrato `psicologamaddalenapimponi.it`, aggiungi il dominio in Resend e copia i record DNS che mostra (SPF e DKIM) nel pannello del registrar. Quando risulta verificato, imposta `RESEND_FROM` a un indirizzo del dominio, per esempio `modulo@psicologamaddalenapimponi.it`.

Gmail gratuito ha un limite di invii giornalieri e non può essere usato su Render free perché la porta SMTP è chiusa: in produzione il canale di riferimento è Resend.

### Mappe e servizi esterni (privacy)

- Le mappe di Google nella pagina Contatti **non si caricano all'apertura**: restano un pulsante "Mostra la mappa". Solo dopo il clic il browser contatta Google, quindi il visitatore non trasmette il proprio indirizzo IP senza un'azione esplicita.
- I font (DM Sans, Playfair Display) sono serviti dal sito stesso, non da `fonts.googleapis.com`, per lo stesso motivo. La licenza è in `wwwroot/fonts/OFL.txt`.
- Non ci sono cookie di tracciamento, analytics o pubblicità: per questo non serve il banner dei cookie. Resta solo il cookie tecnico indispensabile alla sicurezza dei moduli.

### Sicurezza

- Intestazioni su ogni risposta: `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy` e **CSP** (Content Security Policy: l'elenco di cosa la pagina può caricare ed eseguire).
- HTTPS obbligatorio e **HSTS** in produzione (istruisce il browser a usare sempre la connessione sicura).
- Protezione anti-falsificazione dei moduli (antiforgery) di ASP.NET Core.
- Chiavi crittografiche di Data Protection salvabili su una cartella stabile: senza, dopo un riavvio o su più istanze i moduli possono rifiutare gli invii.
- Header del proxy (`X-Forwarded-*`) letti correttamente quando il sito sta dietro IIS, nginx o Cloudflare: così nei log compare l'indirizzo reale del visitatore e il redirect HTTPS funziona.

### Ricerca su Google e anteprime social

- Ogni pagina ha titolo e descrizione propri, indirizzo canonico, anteprime per social (`og:`) e scheda Twitter.
- Dati strutturati schema.org `Psychologist` con recapiti e le due sedi: aiutano Google a mostrare orari, indirizzi e telefono.
- `/sitemap.xml` e `/robots.txt` sono generati dall'applicazione, con indirizzi assoluti ricavati dalla richiesta: quando arriverà il dominio non servirà modificare nulla.
- Errore e pagina non trovata sono marcati `noindex` per non finire nei risultati di ricerca.

### Accessibilità e prestazioni

- Contrasti del testo almeno 4.5:1, etichette sui campi, stati dei messaggi annunciati agli screen reader, focus visibile.
- Immagini con dimensioni dichiarate e caricamento differito; compressione delle risposte attiva; CSS e JS rivalidati a ogni caricamento (così un aggiornamento si vede subito), immagini in cache per un'ora.

### Pagine tecniche di servizio

| Indirizzo | A cosa serve |
|---|---|
| `/health` | Risponde `Healthy` se l'applicazione è viva: utile al monitoraggio dell'hosting |
| `/robots.txt` | Regole per i motori di ricerca, con il rimando alla sitemap |
| `/sitemap.xml` | Elenco delle pagine pubbliche |

---

## Glossario

| Parola | Significato semplice |
|---|---|
| **Blazor** | Tecnologia .NET che costruisce le pagine web con componenti C# invece che solo JavaScript |
| **Razor** | Il linguaggio dei file `.razor`: HTML con dentro piccoli blocchi di C# |
| **SMTP** | Il "servizio postale" usato per spedire email |
| **User Secrets** | Cassetto di credenziali locale di Visual Studio/.NET, fuori dal repository |
| **Variabile d'ambiente** | Impostazione passata al programma dal sistema o dall'hosting, senza scriverla nei file |
| **CSP** | Elenco che dice al browser quali script, stili, immagini e frame sono ammessi |
| **HSTS** | Istruzione al browser: usa sempre HTTPS per questo sito |
| **Canonical** | Indirizzo "ufficiale" di una pagina, per evitare duplicati nei motori di ricerca |
| **JSON-LD / schema.org** | Dati leggibili dalle macchine che descrivono l'attività (nome, indirizzo, telefono) |
| **Sitemap** | Elenco delle pagine che il sito consegna a Google |
| **Honeypot** | Campo finto e nascosto: solo i programmi automatici lo compilano |
| **Rate limit** | Limite al numero di azioni in un intervallo di tempo |
| **Antiforgery** | Difesa contro l'invio di moduli da siti estranei |
| **Data Protection** | Sistema .NET che cifra e firma dati sensibili come i token dei moduli |
| **Reverse proxy** | Server (IIS, nginx, Cloudflare) che sta davanti all'applicazione e inoltra le richieste |
| **Publish** | Preparazione della versione compilata da copiare sul server |

---

## Configurazione

| Dove | Cosa contiene | Finisce su GitHub? |
|---|---|---|
| `appsettings.json` | Impostazioni non segrete: destinatario email, limiti del modulo, percorsi | Sì |
| `appsettings.Development.json` | Override per lo sviluppo | Sì |
| User Secrets | Credenziali SMTP in locale | No |
| Variabili d'ambiente | Credenziali e dominio in produzione | No |

### Impostazioni del modulo (`ContactForm`)

| Chiave | Valore attuale | Significato |
|---|---|---|
| `MaxPerWindow` | 3 | Messaggi consentiti per visitatore nella finestra |
| `WindowMinutes` | 10 | Durata della finestra, in minuti |
| `MinSecondsBetween` | 20 | Attesa minima fra due invii dello stesso visitatore |
| `GlobalPerHour` | 30 | Tetto di messaggi all'ora per tutto il sito |

Il conteggio vive in memoria: si azzera al riavvio. È sufficiente a fermare gli invii automatici, non è un registro permanente.

---

## Pubblicare online

Il sito è predisposto per **Render** con piano **free**: costo zero per hosting, certificato TLS e dominio personalizzato. Si paga solo la registrazione del dominio (circa 10-15 € all'anno). Il servizio si sospende dopo 15 minuti senza visite e la prima richiesta successiva lo risveglia in circa un minuto: per un sito vetrina è accettabile.

### Come funziona il deploy automatico

Il repository contiene già `Dockerfile` e `render.yaml`. Su Render:

1. **New → Blueprint** (oppure **New → Web Service**) e seleziona il repository `PsiMaddalenaPimponi`, ramo `main`. Se il repository è privato, Render deve avere accesso tramite la sua app GitHub: nella pagina di autorizzazione spunta il repository.
2. Render legge `render.yaml` e crea il servizio `psicologamaddalenapimponi` con piano free, regione Francoforte, controllo di salute su `/health`.
3. Aggiungi `RESEND_API_KEY` fra le variabili d'ambiente (è segreta, non sta nel repository).
4. Da quel momento **ogni push su `main` ricostruisce e pubblica il sito** (`autoDeploy: true`). Il deploy si può seguire dalla scheda *Events* del servizio.

Il sito resta raggiungibile anche su `https://psicologamaddalenapimponi.onrender.com` prima che il dominio sia pronto.

### Collegare il dominio

Dopo aver registrato `psicologamaddalenapimponi.it` presso un registrar italiano (Aruba, Register.it, OVH e simili; serve un codice fiscale o una partita IVA):

1. Su Render: servizio → **Settings → Custom Domains** → *Add Custom Domain* → `psicologamaddalenapimponi.it`. Render mostra i record DNS da creare.
2. Nel pannello DNS del registrar:
   - per il dominio senza `www`, un record **A** verso l'indirizzo indicato da Render (per l'apice non si può usare un CNAME);
   - per `www`, un **CNAME** verso `psicologamaddalenapimponi.onrender.com`, oppure aggiungi `www.psicologamaddalenapimponi.it` su Render come dominio che reindirizza all'apice.
3. Attendi la verifica (da pochi minuti a qualche ora): Render emette il certificato HTTPS da solo, senza costi.
4. Per l'invio email, in Resend aggiungi lo stesso dominio e copia i record SPF/DKIM nel pannello DNS, poi imposta `RESEND_FROM` a `modulo@psicologamaddalenapimponi.it`.

Gli indirizzi di canonical, anteprime social e sitemap si adattano da soli all'indirizzo con cui arriva la richiesta: non c'è nessun valore da cambiare nel codice.

### Alternativa: pubblicazione classica

```powershell
dotnet publish .\SitoMadda.UI -c Release -o C:\publish\sito-madda
```

Su **IIS**: application pool con *No Managed Code*, hosting bundle ASP.NET Core 8 installato, sito HTTPS con certificato valido. In alternativa l'output pubblicato gira su Azure App Service o in un container, purché `ASPNETCORE_ENVIRONMENT=Production`.

### Variabili d'ambiente

| Variabile | Quando serve | Valore |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | sempre | `Production` (attiva HSTS, redirect HTTPS e pagina di errore) |
| `RESEND_API_KEY` | per inviare dal modulo in produzione | chiave dell'account Resend |
| `RESEND_FROM` | con dominio verificato su Resend | `modulo@psicologamaddalenapimponi.it` |
| `Email__SenderAddress` | solo per l'invio SMTP in locale | account Gmail che spedisce |
| `Email__SenderPassword` | solo per l'invio SMTP in locale | password per le app |
| `ReverseProxy__TrustAll` | su Render | `true`: il proxy non ha un indirizzo fisso |
| `AllowedHosts` | quando il dominio è attivo | dominio reale; più domini separati da `;` |
| `DataProtection__KeysPath` | hosting con disco persistente o più istanze | cartella scrivibile, es. `D:\home\data-protection` |
| `Site__PublicBaseUrl` | facoltativa | es. `https://www.example.it`; se assente si usa l'indirizzo della richiesta |

I due punti di attenzione sono i segreti (mai in `appsettings.json`) e `ASPNETCORE_ENVIRONMENT`: senza `Production` il sito non applica HSTS e redirect HTTPS.

### Cosa resta da completare prima del go-live

- Testo definitivo dell'informativa privacy: `Privacy.razor` contiene ancora la bozza da validare con la titolare.
- Valutazione delle voci del modulo che rivelano dati sanitari ("Ansia e stress", "Alimentazione e rapporto con il corpo"): richiedono consenso esplicito oppure vanno rimosse.
- Registrazione del dominio e verifica del dominio su Resend per l'invio email.
- Raccolta dei log: su Render sono consultabili dalla dashboard del servizio; il progetto scrive su console e non su file.
- Chiavi Data Protection: sul piano free il disco è effimero, quindi a ogni deploy cambiano; chi ha il modulo aperto deve ricaricare la pagina.

---

## Manutenzione: dove mettere le mani

| Cosa vuoi cambiare | Dove |
|---|---|
| Telefono, email, WhatsApp, Instagram, MioDottore, sedi | `SitoMadda.Application/Services/SiteContent.cs` |
| Titolo o descrizione di una pagina | `SitoMadda.Application/Services/SitePages.cs` |
| Testi delle pagine | `SitoMadda.UI/Components/Pages/*.razor` |
| Colori, spazi, tipografia, animazioni | `SitoMadda.UI/wwwroot/site.css` |
| Comportamenti allo scroll e menu | `SitoMadda.UI/wwwroot/js/navigation.js` |
| Foto e ritratti | `SitoMadda.UI/wwwroot/images` (vedi sotto per le proporzioni) |
| Icona del sito | `wwwroot/favicon.ico`, `favicon.png`, `favicon.svg`, `apple-touch-icon.png` |
| Testo dell'email ricevuta | `SitoMadda.Infrastructure/Services/SmtpEmailSender.cs` |
| Regole di sicurezza e avvio | `SitoMadda.UI/Program.cs` |
| Immagine e configurazione di Render | `Dockerfile`, `render.yaml` |
| Canale di invio email | `SitoMadda.Infrastructure/DependencyInjection.cs` |

Per aggiungere una pagina nuova servono tre passi: creare il file `.razor` in `Components/Pages`, aggiungerlo a `SitePages.cs` (così entra in sitemap e metadati) e inserire il collegamento nel menu in `MainLayout.razor`.

I due ritratti derivano dall'originale `fotoprofilomadda.jpeg`: `maddalena-ritratto.jpg` è il quadrato 720 × 720 della home, `maddalena-verticale.jpg` è il verticale della pagina Chi sono. Se la foto cambia vanno rigenerati con gli stessi nomi e proporzioni. L'immagine per le anteprime social è il quadrato 720 × 720.

---

## Cronologia delle funzionalità

| Data | Novità |
|---|---|
| 2026-11-04 | Modulo contatti con invio email, gestione errori, pagina 404 e intestazioni di sicurezza |
| 2026-11-05 | Restyling completo, foto in alta definizione, favicon, animazioni allo scroll, collegamento MioDottore |
| 2026-11-06 | Pronto per la produzione: font locali, mappe su richiesta, limite invii e campo esca, CSP applicata, metadati SEO, sitemap e robots, dati strutturati, health check, chiavi Data Protection, lettura header del proxy |
| 2026-11-09 | Pubblicazione su Render con piano free e deploy automatico dal ramo `main`; invio email tramite Resend perché il piano free blocca SMTP |

---

## Com'è organizzato il codice

| Progetto | Ruolo |
|---|---|
| `SitoMadda.UI` | Pagine, layout, stili, immagini, modulo contatti e avvio dell'applicazione |
| `SitoMadda.Application` | Contenuti condivisi (recapiti, sedi, catalogo pagine), interfaccia dell'invio email, limite invii |
| `SitoMadda.Domain` | Modello della sede e generazione dei collegamenti alle mappe |
| `SitoMadda.Infrastructure` | Invio email via SMTP e configurazione del mittente |

Le dipendenze vanno in una direzione sola: `UI → Application → Domain`, mentre `Infrastructure` realizza l'interfaccia email definita in `Application`. I metodi e i componenti sono commentati in italiano, spiegando cosa fanno e perché.

### Tecnologia in breve

Il sito usa il rendering interattivo di Blazor Server: la pagina viene costruita sul server e il browser mantiene una connessione con il programma. Per questo il modulo può inviare l'email senza una API dedicata, e per lo stesso motivo i limiti di invio sono gestiti dentro l'applicazione (`ContactFormLimiter`) e non a livello di singola richiesta HTTP.
