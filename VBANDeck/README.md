# VBANDeck
### Version 1.0.2
#### VBAN Remote Plugin for the Elgato StreamDeck

## How to use
For help with the VoiceMeeter scripting language, please refer to [this Document, chapter "MACRO Buttons > VoiceMeeter Remote Requests"](https://vb-audio.com/Voicemeeter/VoicemeeterBanana_UserManual.pdf).

### VoiceMeeter Setup
- You need at least [VoiceMeeter Banana](https://vb-audio.com/Voicemeeter/banana.htm) to use this plugin.
- In the VBAN Screen;
  - Turn VBAN On using the switch in the top-left corner
  - Turn the Remoting stream on, its the last entry in the "Incoming Streams" area
    - Default name: `Command1`
    - This name must match your StreamDeck configuration
  - The "IP Address From" field may be left empty, for Command-streams this is ignored T

The VBAN screen should look something like this:
![VBAN Screen Configuration](https://raw.githubusercontent.com/burdoto/VBANDeck/master/docs/vban-screen.png)

### Simple Script
A Simple Script Button will send a static script every time it is pressed.

- The IP Address is the Address of the Computer that VoiceMeeter runs on
  - This is shown in the VBAN configuration screen within VoiceMeeeter
- The Port is the UDP Port configured in the VBAN configuration screen
- The StreamName must match the Stream Name of the Incoming Remoting Stream in the VoiceMeeter instance

Here is a configuration example for a Simple Script Button that will disable the first hardware-output bus:

![Simple Script configuration example](https://raw.githubusercontent.com/burdoto/VBANDeck/master/docs/simple-script-example.png) 

### Toggle Button / Double State Button
The Double State Button has two states; enabled and disabled.

It sends one static script every time the state changes.

- The IP Address is the Address of the Computer that VoiceMeeter runs on
  - This is shown in the VBAN configuration screen within VoiceMeeeter
- The Port is the UDP Port configured in the VBAN configuration screen
- The StreamName must match the Stream Name of the Incoming Remoting Stream in the VoiceMeeter instance
- The "Request for Trigger IN" is the Script sent when the button becomes enabled
- The "Request for Trigger OUT" is the Script sent when the button becomes disabled

Here is a configuration example for a Double State Button that toggles the first hardware-input strip:

![Double State configuration example](https://raw.githubusercontent.com/burdoto/VBANDeck/master/docs/double-state-example.png)
