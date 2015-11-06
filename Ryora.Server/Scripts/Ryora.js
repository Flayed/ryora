/// <reference path="jquery-ui-1.11.4.js" />
/// <reference path="jquery-1.10.2.intellisense.js" />
/// <reference path="knockout-3.3.0.debug.js" />
/// <reference path="jquery.signalR-2.2.0.js" />

$(function () {

    function dataUse(channel) {
        var self = this;
        self.channel = channel;
        self.transmitted = ko.observable(0);
        self.transmittedKb = ko.computed(function () {
            return (self.transmitted() / 1000).toFixed(2);
        }, self);
        self.transmittedMb = ko.computed(function () {
            return (self.transmitted() / 1000000).toFixed(2);
        }, self);
        self.transmittedGb = ko.computed(function () {
            return (self.transmitted() / 1000000000).toFixed(2);
        }, self);
        self.addTransmitted = function(data) {
            self.transmitted(self.transmitted() + data);
        }
    };

    function viewModel() {
        var self = this;

        self.channels = ko.observableArray([]);

        var hubProxy = $.connection.remoteAssistHub;
        $.connection.hub.logging = true;        

        //self.createSupportRequest = function (detail) {
        //    if (self.processing()) return;
        //    self.errorMessage();
        //    hubProxy.server.createSupportRequest(detail.dealerId);
        //    self.processing(true);
        //}

        $.extend(hubProxy.client, {
            channelListing: function (channels) {
                ko.utils.arrayForEach(channels, function(channel) {
                    self.channels.push(new dataUse(channel));
                });
            },
            newChannel: function (channel) {
                self.channels.push(new dataUse(channel));
            },        
            moreData: function (channel, data) {
                var chan = ko.utils.arrayFirst(self.channels(), function (ch) { return ch.channel === channel; });
                if (!chan) return;
                chan.addTransmitted(data);
            }        
        });
        
        $.connection.hub.start().done(function () {
            console.log("Connection started");
        });

    }

    ko.applyBindings(new viewModel());

}());