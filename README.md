<p align="center"><img src="img/e30.jpg?raw=true"></p>

# E30 BMW Motronics ECU Checksum

After "tunning" adjusting air/fuel ratio maps inside EPROM if the checksum is wrong the engine light stays ON.
To fix the engine light you will need to correct the checksum.

Based on the article found at [Pro2DME.com](https://web.archive.org/web/20130526120948/http://www.pro2dme.com:80/checksum.htm) I wrote a C# program to calculate Motronics 1.3 checksum. It worked for BMW E30 325i - M20 Engine - DME 173.

Later additions were added for Motronic version 1.5.5/3.1/3.3/4.4 but NOT tested in real life.

## Screenshot

![Screenshot](img/screenshot.png?raw=true)

## Download

![Windows](img/win.png?raw=true) [Download for Windows](../../releases/download/1.0/E30_M20_DME173_CRC.zip)

## Author

Dima Dykhman

## Licenses

[![CC0](http://i.creativecommons.org/l/zero/1.0/88x31.png)](http://creativecommons.org/publicdomain/zero/1.0/)