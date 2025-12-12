const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveNotification", (message) => {
    console.log("Bildirim geldi:", message);
    alert("Bildirim: " + message);
});

async function start() {
    try {
        await connection.start();
        console.log("SignalR bağlı ✅");
    } catch (err) {
        console.error("SignalR bağlanamadı ❌", err);
        setTimeout(start, 2000);
    }
}

start();
