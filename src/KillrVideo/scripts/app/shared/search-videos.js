﻿define(["knockout", "jquery", "knockout-extenders"], function (ko, $) {
    // Return a view model
    return function() {
        var self = this;

        // The value in the search box, throttled so that we don't constantly update as they are typing
        self.searchValue = ko.observable("").extend({ rateLimit: { method: "notifyWhenChangesStop", timeout: 400 } });

        // A list of tag suggestions, using the async extender to provide a value when a query is in-flight
        self.tagSuggestions = ko.computed(function() {
            var search = self.searchValue();
            if (search.length === 0)
                return [];

            return $.getJSON("/search/suggesttags", { tagStart: search, pageSize: 10 }).then(function (response) {
                // If we failed for some reason, just return an empty array
                if (!response.success)
                    return [];

                return response.data.tags;
            });
        }).extend({ async: [] });
    }
});