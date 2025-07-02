// Blazor JavaScript Interop
export class BlazorInterop {
    static initialize(): void {
        console.log('IAMS Blazor App Initialized');
        this.setupGlobalErrorHandler();
        //this.setupBeforeUnloadHandler();
    }

    private static setupGlobalErrorHandler(): void {
        window.addEventListener('error', (event) => {
            console.error('Global error:', event.error);
            // Could send to logging service
        });

        window.addEventListener('unhandledrejection', (event) => {
            console.error('Unhandled promise rejection:', event.reason);
            // Could send to logging service
        });
    }

    //private static setupBeforeUnloadHandler(): void {
    //    window.addEventListener('beforeunload', (event) => {
    //        // Could save unsaved data or warn user
    //    });
    //}

    // Customer management helpers
    static formatTurkishCurrency(amount: number): string {
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: 'TRY'
        }).format(amount);
    }

    static formatTurkishDate(date: Date): string {
        return new Intl.DateTimeFormat('tr-TR', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        }).format(date);
    }

    static validateTurkishNationalId(nationalId: string): boolean {
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

    // File handling
    static async downloadFile(filename: string, content: string, mimeType: string = 'text/plain'): Promise<void> {
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

    // Local storage helpers
    static setLocalStorage(key: string, value: string): void {
        try {
            localStorage.setItem(key, value);
        } catch (error) {
            console.warn('LocalStorage not available:', error);
        }
    }

    static getLocalStorage(key: string): string | null {
        try {
            return localStorage.getItem(key);
        } catch (error) {
            console.warn('LocalStorage not available:', error);
            return null;
        }
    }

    static removeLocalStorage(key: string): void {
        try {
            localStorage.removeItem(key);
        } catch (error) {
            console.warn('LocalStorage not available:', error);
        }
    }

    // Print helpers
    static printElement(elementId: string): void {
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

    // Notification helpers
    static showBrowserNotification(title: string, message: string, icon?: string): void {
        if (!('Notification' in window)) {
            console.warn('Browser does not support notifications');
            return;
        }

        if (Notification.permission === 'granted') {
            new Notification(title, {
                body: message,
                icon: icon || '/favicon.png'
            });
        } else if (Notification.permission !== 'denied') {
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

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    BlazorInterop.initialize();
});

// Export for global access
(window as any).BlazorInterop = BlazorInterop;