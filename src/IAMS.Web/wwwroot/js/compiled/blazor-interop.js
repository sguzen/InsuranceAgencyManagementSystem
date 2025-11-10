export class BlazorInterop {
    static initialize() {
        console.log('IAMS Blazor App Initialized');
        this.setupGlobalErrorHandler();
    }
    static setupGlobalErrorHandler() {
        window.addEventListener('error', (event) => {
            console.error('Global error:', event.error);
        });
        window.addEventListener('unhandledrejection', (event) => {
            console.error('Unhandled promise rejection:', event.reason);
        });
    }
    static formatTurkishCurrency(amount) {
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: 'TRY'
        }).format(amount);
    }
    static formatTurkishDate(date) {
        return new Intl.DateTimeFormat('tr-TR', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        }).format(date);
    }
    static validateTurkishNationalId(nationalId) {
        if (!nationalId || nationalId.length !== 11) {
            return false;
        }
        if (!/^\d{11}$/.test(nationalId)) {
            return false;
        }
        const digits = nationalId.split('').map(Number);
        if (digits[0] === 0) {
            return false;
        }
        const sum1 = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        const sum2 = digits[1] + digits[3] + digits[5] + digits[7];
        const check1 = (sum1 * 7 - sum2) % 10;
        const check2 = (sum1 + sum2 + digits[9]) % 10;
        return check1 === digits[9] && check2 === digits[10];
    }
    static async downloadFile(filename, content, mimeType = 'text/plain') {
        const blob = new Blob([content], { type: mimeType });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
    }
    static async downloadFileFromBase64(filename, base64Content, mimeType = 'text/plain') {
        // Decode base64 to binary
        const binaryString = atob(base64Content);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        const blob = new Blob([bytes], { type: mimeType });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
    }
    static setLocalStorage(key, value) {
        try {
            localStorage.setItem(key, value);
        }
        catch (error) {
            console.warn('LocalStorage not available:', error);
        }
    }
    static getLocalStorage(key) {
        try {
            return localStorage.getItem(key);
        }
        catch (error) {
            console.warn('LocalStorage not available:', error);
            return null;
        }
    }
    static removeLocalStorage(key) {
        try {
            localStorage.removeItem(key);
        }
        catch (error) {
            console.warn('LocalStorage not available:', error);
        }
    }
    static printElement(elementId) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.warn(`Element with ID ${elementId} not found`);
            return;
        }
        const printWindow = window.open('', '_blank');
        if (!printWindow) {
            console.warn('Could not open print window');
            return;
        }
        printWindow.document.write(`
            <!DOCTYPE html>
            <html>
            <head>
                <title>IAMS - Yazdır</title>
                <style>
                    body { font-family: Arial, sans-serif; margin: 20px; }
                    .no-print { display: none !important; }
                    table { border-collapse: collapse; width: 100%; }
                    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                    th { background-color: #f2f2f2; }
                </style>
            </head>
            <body>
                ${element.innerHTML}
            </body>
            </html>
        `);
        printWindow.document.close();
        printWindow.focus();
        printWindow.print();
        printWindow.close();
    }
    static showBrowserNotification(title, message, icon) {
        if (!('Notification' in window)) {
            console.warn('Browser does not support notifications');
            return;
        }
        if (Notification.permission === 'granted') {
            new Notification(title, {
                body: message,
                icon: icon || '/favicon.png'
            });
        }
        else if (Notification.permission !== 'denied') {
            Notification.requestPermission().then(permission => {
                if (permission === 'granted') {
                    new Notification(title, {
                        body: message,
                        icon: icon || '/favicon.png'
                    });
                }
            });
        }
    }
}
document.addEventListener('DOMContentLoaded', () => {
    BlazorInterop.initialize();
});
window.BlazorInterop = BlazorInterop;

// Global wrapper functions for easier Blazor interop
window.downloadFile = (filename, base64Content, mimeType) => {
    return BlazorInterop.downloadFileFromBase64(filename, base64Content, mimeType);
};
//# sourceMappingURL=blazor-interop.js.map