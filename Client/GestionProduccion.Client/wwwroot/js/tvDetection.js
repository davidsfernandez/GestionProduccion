/**
 * tvDetection.js - Detects if the device is a Smart TV or similar display device.
 */
window.tvDetection = {
    isTvDevice: function () {
        const userAgent = navigator.userAgent || navigator.vendor || window.opera;
        const tvKeywords = [
            'SmartTV', 'Tizen', 'Web0S', 'Viera', 'Bravia', 'WebOS', 'LG Browser',
            'Panasonic', 'Philips', 'Samsung', 'Sharp', 'Sony', 'Toshiba', 
            'AndroidTV', 'CrKey', 'Roku', 'AppleTV', 'HbbTV'
        ];
        
        // 1. Check User Agent keywords
        const isTvUA = tvKeywords.some(keyword => userAgent.includes(keyword));
        
        // 2. Check for large screens with no touch support (common for factory TVs)
        const isLargeScreen = window.screen.width >= 1280 && !('ontouchstart' in window);
        
        return isTvUA || isLargeScreen;
    },
    getScreenResolution: function () {
        return {
            width: window.screen.width,
            height: window.screen.height
        };
    }
};