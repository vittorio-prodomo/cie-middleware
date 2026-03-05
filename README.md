# MIDDLEWARE CSP-PKCS11 PER LA CIE 3.0 (Fork non ufficiale)

> **Nota:** Questo è un fork non ufficiale del [repository originale](https://github.com/italia/cie-middleware) mantenuto da [@vittorio-prodomo](https://github.com/vittorio-prodomo).
>
> Il motivo di questo fork è la necessità di miglioramenti rapidi alla qualità d’uso (QoL) dell’applicazione CIE ID, in particolare nella funzionalità di firma digitale con firma grafica, senza dover attendere i tempi di rilascio dei manutentori ufficiali.
>
> **Modifiche principali rispetto all’upstream:**
> - Anteprima PDF in finestra separata ridimensionabile (per leggere meglio il testo e posizionare la firma con precisione)
> - Rendering PDF a risoluzione più alta (150 DPI invece di 72)
> - Correzione del rapporto d’aspetto dell’anteprima PDF (non più distorto)
> - Maniglie di ridimensionamento visibili (cerchi blu) sulla firma grafica, con supporto a tutti gli 8 punti (angoli + punti medi)
> - Ridimensionamento da angolo con blocco del rapporto d’aspetto corrente, ridimensionamento da punto medio libero
> - Doppio clic sulla firma per ripristinare il rapporto d’aspetto originale dell’immagine
>
> **Installazione:** Il file `CIEID.exe` fornito nelle [Release](https://github.com/vittorio-prodomo/cie-middleware/releases) è un sostituto drop-in dell’eseguibile installato dal Setup ufficiale. È sufficiente sostituire il file in `C:\Program Files (x86)\CIEPKI\CIEID.exe` (richiede privilegi di amministratore). Non è necessario reinstallare il middleware.

---

## CASO D’USO

Il middleware CIE è una libreria software che implementa le interfacce crittografiche standard **PKCS#11** e **CSP**. Esso consente agli applicativi integranti di utilizzare il certificato di autenticazione e la relativa chiave privata memorizzati sul chip della CIE astraendo dalle modalità di comunicazione di basso livello. 

## ARCHITETTURA
La libreria è sviluppata in C++ su Visual Studio 2017 Community; per compilare il modulo di installazione (progetto **Setup**) è inoltre necessario [NSIS 3.02.1](https://sourceforge.net/projects/nsis/). Allo stato attuale è utilizzabile esclusivamente in ambiente Windows. Entrambe le interfacce sono esposte della stessa libreria (CIEPKI.dll), che viene compilata dal progetto CSP. La libreria viene compilata sia in versione a 32 bit che a 64 bit.

L’interfaccia CSP è conforme alla versione 7 delle specifiche dei Minidriver pubblicate da Microsoft a [questo](http://download.microsoft.com/download/7/E/7/7E7662CF-CBEA-470B-A97E-CE7CE0D98DC2/sc-minidriver_specs_V7.docx) indirizzo.
L’interfaccia PKCS11 è conforme alla specifica [RSA 2.11](https://www.cryptsoft.com/pkcs11doc/v211/).

## CSP
Il Minidriver CIE gestisce la carta in modalità **Read-Only**, come previsto dalle specifiche §7.4, pertanto i comandi di creazione e cancellazione di oggetti non sono supportati. Si faccia riferimento alla specifica Microsoft per i dettagli su quali operazioni possono essere effettuate su una carta Read Only.
Il modulo CSP implementa anche uno store provider per i certificati, in modo tale da non richiedere l’operazione di propagazione dei certificati nello store di sistema.

## PKCS11
Allo stesso modo del CSP, anche il PKCS11 gestisce la carta in modalità **read-only**. Pertanto le operazioni di creazione, modifica e distruzione di qualsiasi oggetto restituiranno un errore.

## Setup
Il modulo di installazione del Middleware si compila tramite il progetto **Setup**, che richiede l'installazione di [NSIS 3.02.1](https://sourceforge.net/projects/nsis/). Il setup installa sia la versione a 32 che a 64 bit, ed effettua la registrazione del CSP e dello Store provider. Il modulo PKCS11 non richiede registrazione, ma il nome del modulo (**CIEPKI.dll**) deve essere noto alle applicazioni che lo utilizzano.

