(function () {
    'use strict';

    const HomeModule = {
        init: function () {
            console.log('HomeModule initialized');
            this.getCatalog()
        },

        cleanup: function () {
            console.log('HomeModule cleaned up');
            document.removeEventListener('DOMContentLoaded', HomeModule.init.bind(HomeModule));
            window.removeEventListener('beforeunload', HomeModule.cleanup.bind(HomeModule));
        },

        getCatalog: function () {
            fetch('/catalog/getProducts')
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.json();
                })
                .then(data => {
                    console.log('Products:', data);
                })
                .catch(error => {
                    console.error('There was a problem with the fetch operation:', error);
                });
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
