# Sito Maddalena Pimponi

Soluzione Visual Studio in C# e Blazor Web App (.NET 8), senza database, autenticazione o API. I progetti separano l'interfaccia, la logica applicativa, i modelli di dominio e le integrazioni esterne.

## Avvio in locale

Apri `SitoMadda.sln` in Visual Studio oppure, da PowerShell:

```powershell
dotnet run --project .\SitoMadda.UI
```

In Visual Studio imposta `SitoMadda.UI` come progetto di avvio e seleziona il profilo `http`. Il profilo apre la pagina iniziale nel browser all'indirizzo `http://localhost:5250/`. Se la soluzione era già aperta durante la modifica della struttura, chiudila e riapri `SitoMadda.sln`; poi, accanto al pulsante verde di avvio, scegli Google Chrome come browser.

## Configurazione dell'invio email

Il modulo invia i messaggi all'indirizzo della psicologa configurato in `SitoMadda.UI\appsettings.json` (`Email:RecipientAddress`). Per la consegna usa l'account Gmail autenticato via SMTP (`Email:SenderAddress`): l'indirizzo inserito nel modulo viene impostato come **Rispondi a** (`Reply-To`) e incluso nel messaggio. In questo modo la psicologa può rispondere direttamente alla persona. Gmail richiede che il mittente SMTP sia l'account autenticato; non è possibile usare come mittente l'indirizzo arbitrario inserito nel modulo.

Per abilitare l'invio:

1. Nell'account Google che spedirà i messaggi, attiva la verifica in due passaggi e genera una [password per le app](https://myaccount.google.com/apppasswords). Non usare la password normale dell'account.
2. Apri PowerShell e sostituisci i valori di esempio con l'indirizzo Gmail mittente e la password per le app appena generata:

```powershell
dotnet user-secrets set "Email:SenderAddress" "ACCOUNT-GMAIL-MITTENTE@gmail.com" --project "C:\Users\andre\source\repos\PsiMaddalenaPimponi.git\SitoMadda.UI"
dotnet user-secrets set "Email:SenderPassword" "PASSWORD-PER-LE-APP" --project "C:\Users\andre\source\repos\PsiMaddalenaPimponi.git\SitoMadda.UI"
```

3. Riavvia l'applicazione da Visual Studio e invia un messaggio di prova dal modulo Contatti.

I comandi salvano le credenziali nei **User Secrets locali** del progetto, non in `appsettings.json` né nella repository Git. Il server e la porta predefiniti sono `smtp.gmail.com` e `587`, con TLS. Senza le credenziali corrette il modulo segnala l'errore e non dichiara che il messaggio sia stato inviato.

## Pagine

- `/` Home
- `/chi-sono` Presentazione e formazione
- `/di-cosa-mi-occupo` Aree di intervento
- `/domande-frequenti` FAQ
- `/contatti` Modulo email, recapiti e mappe Google Maps di Orvieto e Narni
- `/privacy` Informativa preliminare da completare e verificare prima della pubblicazione

Il ritratto in `SitoMadda.UI\wwwroot\images` è stato ritagliato dalla schermata di riferimento. Le schermate originali restano nella cartella `Sito Madda` sul Desktop.

## Struttura del codice

- `SitoMadda.UI`: pagine Razor, layout, modulo contatti, fogli di stile e immagini.
- `SitoMadda.Application`: contenuti condivisi e interfaccia del servizio email.
- `SitoMadda.Domain`: modello della sede e generazione dei collegamenti alle mappe.
- `SitoMadda.Infrastructure`: implementazione SMTP e configurazione del mittente.

La direzione delle dipendenze è `UI → Application → Domain`; `Infrastructure` implementa l'interfaccia email dell'applicazione. Non sono stati aggiunti livelli o dipendenze per autenticazione, database o API. I metodi e i componenti applicativi sono documentati in italiano; le chiavi SMTP restano allineate alla sezione `Email` in `appsettings.json`.
