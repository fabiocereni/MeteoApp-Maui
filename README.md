# Documentazione Progetto Meteo .NET MAUI

Questo documento descrive le caratteristiche, l'architettura e le modalita di installazione dell'applicazione Meteo sviluppata con il framework .NET MAUI.

## 1. Descrizione del Progetto
L'applicazione permette la consultazione delle previsioni meteorologiche in tempo reale. Il sistema integra servizi REST esterni per il recupero dei dati, un database locale per la persistenza delle informazioni e funzionalita di geolocalizzazione tramite GPS.

## 2. Funzionalita Implementate

- Visualizzazione Meteo: Dati in tempo reale su temperatura, umidita, vento e condizioni atmosferiche.
- Localizzazione GPS: Utilizzo del sensore GPS per rilevare la posizione attuale dell'utente e fornire il meteo locale istantaneo all'apertura dell'app.
- Notifiche di Avviso: Sistema di monitoraggio che invia notifiche push in caso di temperature estreme (condizioni di troppo caldo o troppo freddo).
- Preferenze Unita di Misura: Gestione delle impostazioni per cambiare la visualizzazione della temperatura tra Celsius (C) e Fahrenheit (F).

## 3. Architettura Tecnica
Il progetto segue il pattern MVVM (Model-View-ViewModel) per garantire una separazione pulita tra logica e interfaccia.

### 3.1 Modelli (Models)
- Database.cs: Gestione del database SQLite locale per il salvataggio e la cronologia delle citta ricercate.
- Entry.cs: Definizione della struttura dati per le voci meteo salvate nel database.
- WeatherResponse.cs: Classi per la deserializzazione dei dati JSON provenienti dal servizio meteo esterno.

### 3.2 ViewModels
- _BaseViewModel.cs: Classe astratta che implementa INotifyPropertyChanged per l'aggiornamento automatico della UI.
- MeteoListViewModel.cs: Contiene la logica per la gestione della lista delle citta, l'integrazione del GPS e il controllo delle soglie per le notifiche termiche.

### 3.3 Servizi (Services)
- WeatherApiService.cs: Client dedicato alle chiamate HTTP verso le API meteorologiche.
- SettingsService.cs: Gestisce la persistenza delle preferenze dell'utente, focalizzandosi sulla scelta tra Celsius e Fahrenheit.

## 4. Tecnologie Utilizzate
- Framework: .NET MAUI
- Linguaggio di programmazione: C#
- Database: SQLite (tramite sqlite-net-pcl)
- Funzionalita di sistema: Geolocalizzazione (GPS) e Notifiche Push tramite MAUI Essentials.

## 5. Installazione e Configurazione

### Prerequisiti
- Visual Studio 2022 con .NET MAUI installato
- Node.js 14+ e npm installati
- Dispositivo Android o emulatore configurato
- Account Firebase (per le notifiche push)
- API Key di OpenWeatherMap

### 5.1 Configurazione del Server Node.js

#### Passaggio 1: Installazione dipendenze
Aprire il terminale nella cartella `MeteoApp/MeteoServer` e eseguire:
```bash
npm install
```

#### Passaggio 2: Firebase Setup
1. Creare un progetto Firebase su [console.firebase.google.com](https://console.firebase.google.com)
2. Scaricare il file `serviceAccountKey.json` dalla sezione "Service Accounts" in Project Settings
3. Posizionare il file `serviceAccountKey.json` nella cartella `MeteoApp/MeteoServer/`

#### Passaggio 3: Avvio del Server
Dalla cartella `MeteoApp/MeteoServer`, eseguire:
```bash
node server.js
```
Il server partirà sulla porta 3000 e inizierà a monitorare le temperature ogni 10 secondi.

**Nota**: Il server deve essere in esecuzione prima di lanciare l'app Android per permettere la registrazione delle notifiche.

### 5.2 Configurazione dell'App .NET MAUI

#### Passaggio 1: Apertura del Progetto
Scaricare i file del progetto e aprire la soluzione (.sln) con Visual Studio 2022.

#### Passaggio 2: Configurazione API OpenWeatherMap
1. Aprire il file `Services/WeatherApiService.cs`
2. Sostituire il valore della `API_KEY` con una chiave valida da [openweathermap.org](https://openweathermap.org/api)

#### Passaggio 3: Configurazione Server
1. Aprire `MeteoApp/MauiProgram.cs`
2. Verificare che l'indirizzo del server Node.js sia configurato correttamente (es. `http://192.168.x.x:3000` per dispositivi fisici o `http://10.0.2.2:3000` per emulatori Android)

#### Passaggio 4: Configurazione Permessi Android
In `Platforms/Android/AndroidManifest.xml`, assicurarsi che siano presenti i seguenti permessi:
```xml
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.INTERNET" />
```

#### Passaggio 5: Setup Google Play Services (per le notifiche)
1. Scaricare il file `google-services.json` da Firebase
2. Posizionarlo nella cartella `MeteoApp/Platforms/Android/`

#### Passaggio 6: Compilazione e Installazione APK
1. Selezionare **Android** come piattaforma target in Visual Studio
2. Collegare il dispositivo Android o avviare un emulatore
3. Compilare il progetto con **Ctrl+Shift+B**
4. Avviare il debug con **F5** (questo installerà l'APK e avvierà l'app)

Oppure, per generare un APK rilasciato:
1. In Visual Studio: **Build > Publish > Pubblica in Android > Release**
2. L'APK si troverà in `bin/Release/net*/android/`

## 6. Utilizzo dell'Applicazione
All'avvio, l'app tenta di localizzare l'utente via GPS per mostrare i dati locali. L'utente può aggiungere manualmente altre città, che verranno salvate nel database locale. 

**Funzionalità principali:**
- Visualizzazione della temperatura attuale tramite GPS
- Aggiunta/rimozione di città alla lista di monitoraggio
- Cambio tra Celsius e Fahrenheit nelle impostazioni
- Notifiche push quando la temperatura scende sotto 0°C o supera 25°C
- Database locale per la cronologia delle ricerche

**Flusso di funzionamento:**
1. L'app all'avvio registra il dispositivo sul server Node.js
2. Il server controlla le temperature ogni 10 secondi
3. Se vengono rilevate temperature critiche, il server invia notifiche push
4. L'app visualizza le notifiche sul dispositivo

## 7. Distribuzione dell'APK per la Consegna

Per consegnare il progetto al professor con l'APK compilato:

1. **Generare il Release APK**: Seguire il Passaggio 6.6 della sezione 5.2
2. **Cartella Release**: L'APK sarà disponibile nella cartella `release/` o `bin/Release/net*/android/`
3. **Documentazione**: Fornire insieme all'APK:
   - Il file `serviceAccountKey.json` (configurato con le credenziali Firebase)
   - Le istruzioni per avviare il server Node.js
   - La lista delle dipendenze Node.js (`package.json` è già incluso)

### Per testare l'APK:
```bash
# 1. Avviare il server Node.js (in una finestra di terminale)
cd MeteoApp/MeteoServer
node server.js

# 2. Installare l'APK sul dispositivo/emulatore
adb install -r bin/Release/net*/android/MeteoApp.apk

# 3. Avviare l'app dal dispositivo
# L'app si registrerà automaticamente al server e inizierà a ricevere notifiche
```
