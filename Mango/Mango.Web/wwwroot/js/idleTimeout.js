const SESSION_TIMEOUT = 10 * 60 * 1000; // 10 minutes inactivity timeout
const TOKEN_REFRESH_THRESHOLD = 2 * 60 * 1000; // Refresh 2 minutes before expiry
const LOGOUT_WARNING_THRESHOLD = 1 * 60 * 1000; // Show warning 1 minute before logout

let lastActivity = Date.now();
let tokenExpiryTime = null;
let inactivityTimer, refreshTokenTimer, logoutWarningTimer;
let isRefreshingToken = false; // Prevent duplicate refresh requests

// Refresh JWT token using AJAX
function refreshToken() {
    if (isRefreshingToken) return; // Prevent multiple calls
    isRefreshingToken = true;

    $.ajax({
        url: "/Auth/RefreshSession",
        type: "POST",
        xhrFields: { withCredentials: true },
        success: function () {
            console.log("Token refreshed successfully");
            fetchTokenExpiry(); // Update token expiry after refresh
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
    window.location.href = "/Auth/Login"; // Redirect to login page
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

    if (timeUntilExpiry > TOKEN_REFRESH_THRESHOLD) {
        refreshTokenTimer = setTimeout(refreshToken, timeUntilExpiry - TOKEN_REFRESH_THRESHOLD);
    } else {
        refreshToken(); // Refresh immediately if near expiry
    }
}

// Fetch token expiry from the server
function fetchTokenExpiry() {
    $.ajax({
        url: "/Auth/GetTokenExpiry",
        type: "GET",
        xhrFields: { withCredentials: true },
        success: function (data) {
            if (data.expiresAt) {
                tokenExpiryTime = new Date(data.expiresAt).getTime();
                console.log("Token expires at:", new Date(tokenExpiryTime).toISOString());
                scheduleTokenRefresh();
            } else {
                console.warn("Invalid token expiry data received, logging out...");
                logoutUser();
            }
        },
        error: function (xhr) {
            if (xhr.status === 401) {
            //    console.warn("Session expired (401), logging out...");
            //    logoutUser();
            } else {
                console.warn("Failed to fetch token expiry, retrying...");
                //setTimeout(fetchTokenExpiry, 5000); // Retry after 5 seconds
            }
        }
    });
}

// Initialize session tracking
function initSessionTracking() {
    fetchTokenExpiry(); // Get initial token expiry
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
