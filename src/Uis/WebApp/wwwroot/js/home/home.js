(function () {
    'use strict';

    const HomeModule = {
        init: function () {
            console.log('HomeModule initialized');
        },

        cleanup: function () {
            console.log('HomeModule cleaned up');
            document.removeEventListener('DOMContentLoaded', HomeModule.init.bind(HomeModule));
            window.removeEventListener('beforeunload', HomeModule.cleanup.bind(HomeModule));
        },

        getCatalog: function () {
        }
        
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', HomeModule.init.bind(HomeModule));
    } else {
        HomeModule.init();
    }

    window.addEventListener('beforeunload', HomeModule.cleanup.bind(HomeModule));

    window.HomeModule = HomeModule;
})();
