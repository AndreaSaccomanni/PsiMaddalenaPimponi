# Sito Maddalena Pimponi

Soluzione Visual Studio in C# e Blazor Web App (.NET 8), senza database, autenticazione o API. I progetti separano l'interfaccia, la logica applicativa, i modelli di dominio e le integrazioni esterne.

## Avvio in locale

Apri `SitoMadda.sln` in Visual Studio oppure, da PowerShell:

```powershell
dotnet run --project .\SitoMadda.UI
```

In Visual Studio imposta `SitoMadda.UI` come progetto di avvio e seleziona il profilo `http`. Il profilo apre la pagina iniziale nel browser all'indirizzo `http://localhost:5250/`. Se la soluzione era già aperta durante la modifica della struttura, chiudila e riapri `SitoMadda.sln`; poi, accanto al pulsante verde di avvio, scegli Google Chrome come browser.

## Configurazione dell'invio email

Il destinatario è configurato in `SitoMadda.UI\appsettings.json`. Per inviare email, imposta mittente e password SMTP con User Secrets, così le credenziali non vengono salvate nei file del progetto:

```powershell
dotnet user-secrets set "Email:SenderAddress" "psi.maddalenapimponi@gmail.com" --project .\SitoMadda.UI
dotnet user-secrets set "Email:SenderPassword" "PASSWORD-APP-GMAIL" --project .\SitoMadda.UI
```

Con Gmail usa una password per le app, non la password normale dell'account. Il server e la porta predefiniti sono `smtp.gmail.com` e `587`. Senza credenziali il modulo avvisa che l'invio non è disponibile, indica l'indirizzo email diretto come alternativa e registra il dettaglio nei log; non dichiara mai che l'email sia stata inviata.

## Pagine

- `/` Home
- `/chi-sono` Presentazione e formazione
- `/di-cosa-mi-occupo` Aree di intervento
- `/domande-frequenti` FAQ
- `/contatti` Modulo email, recapiti e mappe Google Maps di Orvieto e Narni
- `/privacy` Informativa preliminare da completare e verificare prima della pubblicazione

Qualsiasi altro percorso viene servito con stato HTTP 404 dalla pagina `PaginaNonTrovata`. Le intestazioni di sicurezza (`nosniff`, `Referrer-Policy`, `Permissions-Policy` e una CSP in modalità di sola segnalazione) sono applicate da `Program.cs`; HSTS e redirect HTTPS restano attivi solo fuori dall'ambiente di sviluppo.

Il ritratto in `SitoMadda.UI\wwwroot\images` è stato ritagliato dalla schermata di riferimento. Le schermate originali restano nella cartella `Sito Madda` sul Desktop.

## Struttura del codice

- `SitoMadda.UI`: pagine Razor, layout, modulo contatti, fogli di stile e immagini.
- `SitoMadda.Application`: contenuti condivisi e interfaccia del servizio email.
- `SitoMadda.Domain`: modello della sede e generazione dei collegamenti alle mappe.
- `SitoMadda.Infrastructure`: implementazione SMTP e configurazione del mittente.

La direzione delle dipendenze è `UI → Application → Domain`; `Infrastructure` implementa l'interfaccia email dell'applicazione. Non sono stati aggiunti livelli o dipendenze per autenticazione, database o API. I metodi e i componenti applicativi sono documentati in italiano; le chiavi SMTP restano allineate alla sezione `Email` in `appsettings.json`.
