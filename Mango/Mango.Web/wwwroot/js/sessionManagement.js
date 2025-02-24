const SESSION_TIMEOUT = 3 * 60 * 1000; // 10 minutes inactivity timeout
const TOKEN_REFRESH_THRESHOLD = 2 * 60 * 1000; // Refresh 2 minutes before expiry
const LOGOUT_WARNING_THRESHOLD = 1 * 60 * 1000; // Show warning 1 minute before logout

let lastActivity = Date.now();
let tokenExpiryTime = null;
let inactivityTimer, refreshTokenTimer, logoutWarningTimer;
let isRefreshingToken = false; // Prevent duplicate refresh requests

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/sessionHub", { withCredentials: true })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Start the SignalR connection
async function startConnection() {
    try {
        await connection.start();
        console.log("Connected to SignalR hub");
        fetchStoredExpiry(); // Retrieve stored expiry time
        scheduleTokenRefresh(); // Schedule token refresh based on expiry time
    } catch (err) {
        console.error("SignalR connection failed:", err);
        setTimeout(startConnection, 5000); // Retry after 5 seconds
    }
}

// Receive session expiry time from the server
connection.on("SessionExpiryTime", function (expiryTime) {
    console.log("Received session expiry time:", expiryTime);

    // Convert expiry time to a proper Date object and store it
    const expiryDate = new Date(expiryTime);
    sessionStorage.setItem("sessionExpiry", expiryDate.getTime()); // Store as a timestamp
    localStorage.setItem("syncSessionExpiry", expiryDate.getTime()); // Sync across tabs

    tokenExpiryTime = expiryDate.getTime();
    console.log("Session expires at:", expiryDate.toISOString());

    scheduleTokenRefresh(); // Recalculate refresh time
});

// Retrieve session expiry from sessionStorage
function fetchStoredExpiry() {
    const storedExpiry = sessionStorage.getItem("sessionExpiry");
    if (storedExpiry) {
        tokenExpiryTime = Number(storedExpiry);
        console.log("Loaded stored expiry:", new Date(tokenExpiryTime).toISOString());
    }
}

// Refresh JWT token using AJAX
function refreshToken() {
    if (isRefreshingToken) return; // Prevent multiple calls
    isRefreshingToken = true;

    $.ajax({
        url: "/Session/RefreshSession",
        type: "POST",
        xhrFields: { withCredentials: true },
        success: function (data) {
            console.log("Token refreshed successfully");

            // Update token expiry after refresh
            if (data.expiresAt) {
                const newExpiry = new Date(data.expiresAt).getTime();
                sessionStorage.setItem("sessionExpiry", newExpiry);
                localStorage.setItem("syncSessionExpiry", newExpiry);
                tokenExpiryTime = newExpiry;
                scheduleTokenRefresh();
            }

            isRefreshingToken = false;
        },
        error: function () {
            console.error("Failed to refresh token, logging out...");
            logoutUser();
        }
    });
}

// Logout user
function logoutUser() {
    console.warn("Logging out user...");
    sessionStorage.clear();
    localStorage.removeItem("syncSessionExpiry");
    window.location.href = "/Auth/Logout"; // Redirect to login page
}

// Reset inactivity timer on user activity
function resetInactivityTimer() {
    lastActivity = Date.now();
    clearTimeout(inactivityTimer);
    clearTimeout(logoutWarningTimer);

    inactivityTimer = setTimeout(() => {
        console.log("User inactive, showing logout warning...");
        Swal.fire({
            title: "Session Expiring Soon!",
            text: "You will be logged out due to inactivity. Click 'Stay Logged In' to continue.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Stay Logged In",
            cancelButtonText: "Logout",
            allowOutsideClick: false
        }).then((result) => {
            if (result.isConfirmed) {
                console.log("User stayed logged in.");
                resetInactivityTimer(); // Reset the timer
            } else {
                console.log("User did not respond, logging out...");
                logoutUser();
            }
        });

        logoutWarningTimer = setTimeout(() => {
            console.log("User did not respond, logging out...");
            logoutUser();
        }, LOGOUT_WARNING_THRESHOLD);
    }, SESSION_TIMEOUT - LOGOUT_WARNING_THRESHOLD);
}

// Schedule token refresh before expiry
function scheduleTokenRefresh() {
    if (!tokenExpiryTime) return;

    const timeUntilExpiry = tokenExpiryTime - Date.now();
    clearTimeout(refreshTokenTimer); // Ensure only one scheduled refresh

    console.log(`Token expires in ${timeUntilExpiry / 1000}s`);

    if (timeUntilExpiry > TOKEN_REFRESH_THRESHOLD) {
        refreshTokenTimer = setTimeout(refreshToken, timeUntilExpiry - TOKEN_REFRESH_THRESHOLD);
    } else {
        refreshToken(); // Refresh immediately if near expiry
    }
}

// Listen for session expiry updates across tabs
window.addEventListener("storage", function (event) {
    if (event.key === "syncSessionExpiry") {
        sessionStorage.setItem("sessionExpiry", event.newValue);
        tokenExpiryTime = Number(event.newValue);
        scheduleTokenRefresh();
    }
});

// Initialize session tracking
function initSessionTracking() {
    startConnection();
    fetchStoredExpiry(); // Load stored expiry time
    resetInactivityTimer();

    // Monitor user activity
    ["mousemove", "keydown", "scroll", "click", "touchstart"].forEach(event => {
        $(document).on(event, resetInactivityTimer);
    });

    console.log("Session tracking initialized");
}

// Start session tracking on page load
document.addEventListener("DOMContentLoaded", function () {
    initSessionTracking();
});
