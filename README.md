# AwairWeb

[AwairWeb](https://smklancher.github.io/AwairWeb/) is a blazor WebAssembly website to view detailed graphs from your [Awair](https://getawair.com/) devices.  Awair provides a mobile app but no web interface.  However they do provide a good API, which even has data at more granular intervals than visible in the mobile app.

The initial page will ask you to provide your Awair bearer token (find it from <https://developer.getawair.com/console/access-token>).  This will be saved in browser localstorage for subsequent use.  However if you want to send/bookmark a link, you can include the token in the url by adding "?token={bearer token}".
