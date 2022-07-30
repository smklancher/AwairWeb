# AwairWeb

[AwairWeb](https://smklancher.github.io/AwairWeb/) is a blazor WebAssembly website to view detailed graphs from your [Awair](https://getawair.com/) devices.  Awair provides a mobile app but no web interface.  However they do provide a good API, which even has data at more granular intervals than visible in the mobile app.  

A detail toggle allows for switching between 1 hour graphs with raw data at 10 second intervals and 24 hour graphs with averages from 5 minute intervals.

The initial page will ask you to provide your Awair bearer token (find it from <https://developer.getawair.com/console/access-token>).  This will be saved in browser localstorage for subsequent use.  However if you want to send/bookmark a link, you can include the token in the url by adding "?token={bearer token}".

Fahrenheit is used by default, but you can add UseFahrenheit=false to the query string to change to Celsius.  So example bookmark would be "?token={bearer token}&UseFahrenheit=false"  This will persist in localstorage.

Device colors are initialized to random colors initially and then persisted to localstorage.

If you would like to manually change the device color values (or delete them so they randomize again) or change the UseFahrenheit setting, use your browser to [view and edit localstorage](https://developer.chrome.com/docs/devtools/storage/localstorage/)
