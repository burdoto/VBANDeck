// this is our global websocket, used to communicate from/to Stream Deck software
// and some info about our plugin, as sent by Stream Deck software
var websocket = null,
    uuid = null,
    actionInfo = {},
    inInfo = {},
    runningApps = [],
    onchangeevt = 'onchange'; // 'oninput'; // change this, if you want interactive elements act on any change, or while they're modified

function connectSocket(inPort, inUUID, inRegisterEvent, inInfo, inActionInfo) {
    uuid = inUUID;
    // please note: the incoming arguments are of type STRING, so
    // in case of the inActionInfo, we must parse it into JSON first
    actionInfo = JSON.parse(inActionInfo); // cache the info
    inInfo = JSON.parse(inInfo);
    websocket = new WebSocket('ws://localhost:' + inPort);

    /** Since the PI doesn't have access to native settings
     * Stream Deck sends some color settings to PI
     * We use these to adjust some styles (e.g. highlight-colors for checkboxes)
     */
    addDynamicStyles(inInfo.colors, 'connectSocket');
    initPropertyInspector(5);

    // if connection was established, the websocket sends
    // an 'onopen' event, where we need to register our PI
    websocket.onopen = function() {
        var json = {
            event: inRegisterEvent,
            uuid: inUUID
        };
        // register property inspector to Stream Deck
        websocket.send(JSON.stringify(json));
        sendValueToPlugin('propertyInspectorConnected', 'property_inspector');
        demoCanvas();
    };

    websocket.onmessage = function(evt) {
        // Received message from Stream Deck
        var jsonObj = JSON.parse(evt.data);
        var event = jsonObj['event'];
    };
}

function object_change(name, id) {

}
