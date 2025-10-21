// ActivityLog SignalR Client
const activityLogConnection = new signalR.HubConnectionBuilder()
    .withUrl("/activityLogHub")
    .withAutomaticReconnect()
    .build();

// Start connection
activityLogConnection.start()
    .then(() => {
        console.log("ActivityLog SignalR connected");
    })
    .catch(err => {
        console.error("ActivityLog SignalR connection error:", err);
    });

// Receive new log notification
activityLogConnection.on("ReceiveNewLog", function (logData) {
    console.log("New log received:", logData);
    
    // Show toast notification
    if (typeof showToast === 'function') {
        showToast(`New Activity: ${logData.action}`, 'info');
    }

    // If on ActivityLogs page, reload the table
    if (window.location.pathname.includes('/ActivityLogs')) {
        reloadActivityLogs();
    }
});

// Receive log update notification
activityLogConnection.on("ReceiveLogUpdate", function () {
    console.log("Activity log updated");
    
    // If on ActivityLogs page, reload the table
    if (window.location.pathname.includes('/ActivityLogs')) {
        reloadActivityLogs();
    }
});

// Reload activity logs table
function reloadActivityLogs() {
    // Simple page reload for now
    setTimeout(() => {
        window.location.reload();
    }, 500);
}

// Toast notification function
function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toast-container') || createToastContainer();
    
    const toast = document.createElement('div');
    toast.className = `alert alert-${type === 'info' ? 'info' : type === 'success' ? 'success' : 'warning'} alert-dismissible fade show`;
    toast.role = 'alert';
    toast.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    toastContainer.appendChild(toast);
    
    // Auto remove after 3 seconds
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 150);
    }, 3000);
}

function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toast-container';
    container.style.position = 'fixed';
    container.style.top = '20px';
    container.style.right = '20px';
    container.style.zIndex = '9999';
    container.style.maxWidth = '400px';
    document.body.appendChild(container);
    return container;
}
