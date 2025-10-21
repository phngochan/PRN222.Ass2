// SignalR connection for Management (Dealers & Customers)
const managementConnection = new signalR.HubConnectionBuilder()
    .withUrl("/managementHub")
    .withAutomaticReconnect()
    .build();

// Start connection
managementConnection.start()
    .then(() => {
        console.log("✅ SignalR Management Hub connected");
    })
    .catch(err => {
        console.error("❌ SignalR connection error:", err);
    });

// Reconnection events
managementConnection.onreconnecting(error => {
    console.warn("⚠️ SignalR reconnecting...", error);
    showToast("Đang kết nối lại...", "warning");
});

managementConnection.onreconnected(connectionId => {
    console.log("✅ SignalR reconnected:", connectionId);
    showToast("Đã kết nối lại thành công", "success");
});

managementConnection.onclose(error => {
    console.error("❌ SignalR connection closed:", error);
    showToast("Mất kết nối realtime", "danger");
});

// ============= DEALER EVENTS =============

// Receive new dealer created
managementConnection.on("ReceiveDealerCreated", (dealerData) => {
    console.log("🏢 New dealer created:", dealerData);
    showToast(`Đại lý mới: ${dealerData.name}`, "success");
    
    // Reload page if on dealer list
    if (window.location.pathname.includes("/DealersManagement")) {
        setTimeout(() => location.reload(), 1500);
    }
});

// Receive dealer updated
managementConnection.on("ReceiveDealerUpdated", (dealerData) => {
    console.log("✏️ Dealer updated:", dealerData);
    showToast(`Đã cập nhật: ${dealerData.name}`, "info");
    
    // Reload page if on dealer list or details
    if (window.location.pathname.includes("/DealersManagement")) {
        setTimeout(() => location.reload(), 1500);
    }
});

// Receive dealer deleted
managementConnection.on("ReceiveDealerDeleted", (data) => {
    console.log("🗑️ Dealer deleted:", data);
    showToast(`Đã xóa đại lý: ${data.name}`, "warning");
    
    // Reload page if on dealer list
    if (window.location.pathname.includes("/DealersManagement/Index")) {
        setTimeout(() => location.reload(), 1500);
    }
});

// ============= CUSTOMER EVENTS =============

// Receive new customer created
managementConnection.on("ReceiveCustomerCreated", (customerData) => {
    console.log("👤 New customer created:", customerData);
    showToast(`Khách hàng mới: ${customerData.name}`, "success");
    
    // Reload page if on customer list
    if (window.location.pathname.includes("/CustomerManagement")) {
        setTimeout(() => location.reload(), 1500);
    }
});

// Receive customer updated
managementConnection.on("ReceiveCustomerUpdated", (customerData) => {
    console.log("✏️ Customer updated:", customerData);
    showToast(`Đã cập nhật: ${customerData.name}`, "info");
    
    // Reload page if on customer list or details
    if (window.location.pathname.includes("/CustomerManagement")) {
        setTimeout(() => location.reload(), 1500);
    }
});

// Receive customer deleted
managementConnection.on("ReceiveCustomerDeleted", (data) => {
    console.log("🗑️ Customer deleted:", data);
    showToast(`Đã xóa khách hàng: ${data.name}`, "warning");
    
    // Reload page if on customer list
    if (window.location.pathname.includes("/CustomerManagement/Index")) {
        setTimeout(() => location.reload(), 1500);
    }
});

// ============= GENERAL NOTIFICATION =============

managementConnection.on("ReceiveNotification", (data) => {
    console.log("📢 Notification:", data);
    showToast(data.message, data.type);
});

// ============= HELPER FUNCTIONS =============

// Show toast notification
function showToast(message, type = "info") {
    // Check if Toastr is available
    if (typeof toastr !== 'undefined') {
        toastr[type](message);
    } else {
        // Fallback to browser notification
        if (Notification.permission === "granted") {
            new Notification("EV Dealer System", {
                body: message,
                icon: "/favicon.ico"
            });
        } else {
            console.log("Toast:", message);
        }
    }
}

// Send notification to all clients
async function sendNotification(message, type = "info") {
    try {
        await managementConnection.invoke("SendNotification", message, type);
    } catch (err) {
        console.error("Error sending notification:", err);
    }
}

// Export for use in other scripts
window.managementHub = {
    connection: managementConnection,
    sendNotification: sendNotification
};
