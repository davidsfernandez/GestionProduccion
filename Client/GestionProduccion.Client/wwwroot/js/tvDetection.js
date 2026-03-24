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
        
        // Check ONLY User Agent keywords. 
        // We removed screen-size based detection to avoid false positives on Laptops/PCs.
        return tvKeywords.some(keyword => userAgent.includes(keyword));
    },
    getScreenResolution: function () {
        return {
            width: window.screen.width,
            height: window.screen.height
        };
    }
};