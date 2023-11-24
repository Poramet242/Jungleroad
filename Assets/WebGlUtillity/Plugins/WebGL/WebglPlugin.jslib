// Creating functions for the 
mergeInto(LibraryManager.library,
{
    SetAllCookies: function (str) {
        document.cookie = Pointer_stringify(str);
    },

    GetAllCookies: function () {
        var returnStr = document.cookie;
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },

    BackToHomePage: function () {
        location.assign("HTTPS://" + window.location.hostname + "/gameplay");
    },
});