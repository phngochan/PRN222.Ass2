// Kết nối tới hub
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/orderHub")
  .build();

connection.start()
  .then(() => console.log("✅ Connected to SignalR Hub"))
  .catch(err => console.error("❌ SignalR error:", err));

// Khi server gửi sự kiện "ReceiveOrderNotification"
connection.on("ReceiveOrderNotification", (message) => {
  alert("🔔 " + message);
  // có thể reload danh sách đơn hàng nếu muốn
  // location.reload();
});
