# DIY SerialUPDI Programmer Guide

## What You Need
- Arduino Nano (or Uno)
- 4.7kΩ resistor
- 10µF capacitor
- Breadboard and jumper wires
- ATmega4809 target board

## Step 1: Upload jtag2updi Firmware to Arduino Nano

1. Download jtag2updi firmware from: https://github.com/ElTangas/jtag2updi
2. Open Arduino IDE
3. Load the jtag2updi sketch
4. Upload to Arduino Nano

## Step 2: Wire the SerialUPDI Programmer

```
Arduino Nano    ATmega4809
TX (Pin 1)  --> UPDI Pin (Pin 6)
GND         --> GND
5V          --> VCC (if powering target)
```

**Important:** Add a 4.7kΩ resistor between TX and UPDI pin, and a 10µF capacitor between VCC and GND near the ATmega4809.

## Step 3: Use in UniFlash IDE

1. Select "ATmega4809" as your board
2. Select "SerialUPDI" as programmer
3. Upload your code

## Alternative: Use Arduino Nano as ISP

If jtag2updi doesn't work, you can use Arduino as ISP:

1. Upload "Arduino as ISP" sketch to Arduino Nano
2. Wire connections:
   - Nano Pin 10 -> ATmega4809 Reset
   - Nano Pin 11 -> ATmega4809 MOSI  
   - Nano Pin 12 -> ATmega4809 MISO
   - Nano Pin 13 -> ATmega4809 SCK
   - Nano 5V -> ATmega4809 VCC
   - Nano GND -> ATmega4809 GND

## Troubleshooting

- Make sure ATmega4809 is powered (3.3V or 5V)
- Check all connections
- Try different baud rates
- Ensure proper voltage levels (ATmega4809 is 5V tolerant) 