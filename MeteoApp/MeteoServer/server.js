const express = require('express');
const admin = require('firebase-admin');
const axios = require('axios');
const cron = require('node-cron');

const serviceAccount = require('./serviceAccountKey.json');
admin.initializeApp({
  credential: admin.credential.cert(serviceAccount)
});

const app = express();
app.use(express.json());

const API_KEY = "81f6fb625f1378f589f0ae8161bcb951";

let userSubscriptions = {};

app.post('/register', (req, res) => {
    console.log('📨 POST /register ricevuto, body:', JSON.stringify(req.body));
    const { token, cities } = req.body;
    if(token && cities) {
        userSubscriptions[token] = cities;
        console.log(`📱 Dispositivo registrato per monitorare: ${cities.join(', ')}`);
        res.sendStatus(200);
    } else {
        res.sendStatus(400);
    }
});

cron.schedule('*/10 * * * * *', async () => {
    console.log("⏱️ Avvio controllo meteo programmato...");

    for (const [token, cities] of Object.entries(userSubscriptions)) {
        for (const city of cities) {
            try {
                const url = `https://api.openweathermap.org/data/2.5/weather?q=${city}&appid=${API_KEY}&units=metric&lang=it`;
                const response = await axios.get(url);
                const temp = response.data.main.temp;

                let title = "";
                let body = "";

                if (temp < 0) {
                    title = "Allerta Freddo! ❄️";
                    body = `A ${city} la temperatura è scesa a ${Math.round(temp)}°C`;
                } else if (temp > 25) {
                    title = "Allerta Caldo! ☀️";
                    body = `A ${city} ci sono ben ${Math.round(temp)}°C!`;
                }

                if (title !== "") {
                    const message = {
                        notification: { title, body },
                        token: token
                    };
                    await admin.messaging().send(message);
                    console.log(`🔔 Notifica Push inviata per ${city} (${temp}°C)`);
                } else {
                    console.log(`✅ Tutto ok a ${city} (${temp}°C), nessuna notifica.`);
                }
            } catch (error) {
                console.error(`❌ Errore nel controllo di ${city}:`, error.message);
            }
        }
    }
});

const PORT = 3000;
app.listen(PORT, '0.0.0.0', () => {
    console.log(`🚀 Server meteo in esecuzione sulla porta ${PORT}`);
    console.log(`Il sistema controllerà le temperature ogni 10 sec in automatico.`);
});
