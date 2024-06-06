// Register callback events for Quasar

mergeInto(LibraryManager.library, {
    loadSuccess: function(aSubStep) {
        data = UTF8ToString(aSubStep)
        __UnityLib__.loadSuccess(data);
    },
    loadFailed: function(aSubStep) {
        data = UTF8ToString(aSubStep)
        __UnityLib__.loadFailed(data);
    },
    showSuccess: function(aSubStep) {
        data = UTF8ToString(aSubStep)
        __UnityLib__.showSuccess(data);
    },
    showFailed: function(aSubStep) {
        data = UTF8ToString(aSubStep)
        __UnityLib__.showFailed(data);
    },
    hideSuccess: function(aSubStep) {
        data = UTF8ToString(aSubStep)
        __UnityLib__.hideSuccess(data);
    },
    hideFailed: function(aSubStep) {
        data = UTF8ToString(aSubStep)
        __UnityLib__.hideFailed(data);
    },
    resetSuccess: function() {
        // data = UTF8ToString(aSubStep)
        __UnityLib__.resetSuccess();
    },
    resetFailed: function(msg) {
        data = UTF8ToString(msg)
        __UnityLib__.resetFailed(data);
    },
    getBoundsSuccess: function() {
        __UnityLib__.getBoundsSuccess();
    },
    getUpperObjectsSuccess: function() {
        __UnityLib__.getUpperObjectsSuccess();
    },
    getLowerObjectsSuccess: function() {
        __UnityLib__.getLowerObjectsSuccess();
    },
    // getDeltaDistance: function(value) {
    //     data = UTF8ToString(value)
    //     __UnityLib__.getDeltaDistance(data);
    // },
    // getPincMode: function(value) {
    //     data = UTF8ToString(value)
    //     __UnityLib__.getPincMode(data);
    // },
    iprCreateSuccess: function() {
        __UnityLib__.iprCreateSuccess();
    }
})