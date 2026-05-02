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

### Passaggio 1: Clonazione
Scaricare i file del progetto e aprire la soluzione (.sln) con Visual Studio 2022.

### Passaggio 2: API Key
Aprire il file Services/WeatherApiService.cs e sostituire il valore segnaposto con la propria chiave API valida per abilitare il recupero dei dati.

### Passaggio 3: Configurazione Permessi
Assicurarsi che il dispositivo o l'emulatore abbia i permessi di geolocalizzazione (GPS) e notifiche abilitati nelle impostazioni di sistema.

### Passaggio 4: Esecuzione
Selezionare il progetto di avvio e la piattaforma desiderata (Android o iOS), quindi avviare la compilazione premendo F5.

## 6. Utilizzo dell'Applicazione
All'avvio, l'app tenta di localizzare l'utente via GPS per mostrare i dati locali. L'utente puo aggiungere manualmente altre citta, che verranno salvate nel database locale. Tramite le impostazioni, e possibile commutare la visualizzazione dei gradi tra Celsius e Fahrenheit o viciversa. Se la temperatura rilevata scende o sale oltre i limiti di sicurezza, l'app emettera una notifica di avviso.
