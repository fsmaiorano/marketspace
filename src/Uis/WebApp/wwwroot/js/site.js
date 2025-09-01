(function () {
    'use strict';

    const SiteModule = {}
    SiteModule.init = function () {
        console.log('SiteModule initialized');
    };

    SiteModule.cleanup = function () {
        console.log('SiteModule cleaned up');
    };

    document.addEventListener('DOMContentLoaded', () => {
        SiteModule.init();
    });

    window.addEventListener('beforeunload', () => {
        SiteModule.cleanup();
    });
})();