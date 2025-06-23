# ATmega4809 Programming Diagnostic Guide

## The Issue
You're getting "Programmer 'serialupdi' not found" error, but the same code works in Arduino IDE.

## Root Cause Analysis

### 1. Check Your UPDI Hardware
**What UPDI programming hardware do you have connected?**

- [ ] **SerialUPDI adapter** (DIY or commercial)
- [ ] **Atmel-ICE** with UPDI support
- [ ] **Curiosity Nano**
- [ ] **JTAG2UPDI** (Arduino Nano as programmer)
- [ ] **PICkit4** with UPDI support
- [ ] **None of the above**

### 2. If You Have UPDI Hardware

#### For SerialUPDI:
1. **Check connections:**
   ```
   SerialUPDI TX → ATmega4809 UPDI pin (usually pin 6)
   SerialUPDI GND → ATmega4809 GND
   SerialUPDI VCC → ATmega4809 VCC (if powering target)
   ```

2. **Add required components:**
   - 4.7kΩ resistor between TX and UPDI pin
   - 10µF capacitor between VCC and GND near ATmega4809

3. **Test with arduino-cli directly:**
   ```bash
   arduino-cli upload -p COM3 --fqbn MegaCoreX:megaavr:4809 --programmer serialupdi_115200 "sketch_folder"
   ```

#### For JTAG2UPDI (Arduino Nano):
1. **Upload jtag2updi firmware to Arduino Nano**
2. **Connect:**
   ```
   Nano Pin 1 (TX) → ATmega4809 UPDI pin
   Nano GND → ATmega4809 GND
   Nano 5V → ATmega4809 VCC
   ```

### 3. If You Don't Have UPDI Hardware

**This is why it's not working!** ATmega4809 requires UPDI programming - regular USB-to-serial cannot program it.

## Quick Fixes to Try

### Fix 1: Update Programmer Selection
In UniFlash IDE:
1. Select "ATmega4809" as device
2. Try different programmers:
   - "SerialUPDI (115200 baud)" ← Try this first
   - "SerialUPDI (230400 baud)"
   - "SerialUPDI (460800 baud)"
   - "SerialUPDI (57600 baud)"

### Fix 2: Check COM Port
Make sure the correct COM port is selected for your UPDI programmer.

### Fix 3: Test with Arduino IDE First
1. **Open Arduino IDE**
2. **Install MegaCoreX board package**
3. **Select ATmega4809 board**
4. **Select your UPDI programmer**
5. **Upload the same code**
6. **If it works in Arduino IDE, the issue is in UniFlash configuration**

### Fix 4: Manual arduino-cli Test
```bash
# Test compilation
arduino-cli compile --fqbn MegaCoreX:megaavr:4809 "IDE/sketch/atmega4809_diagnostic"

# Test upload (replace COM3 with your port)
arduino-cli upload -p COM3 --fqbn MegaCoreX:megaavr:4809 --programmer serialupdi_115200 "IDE/sketch/atmega4809_diagnostic"
```

## Expected Results

### If UPDI Hardware is Working:
- ✅ Compilation successful
- ✅ Upload successful
- ✅ LED on ATmega4809 blinks
- ✅ Serial output shows diagnostic messages

### If UPDI Hardware is Missing:
- ✅ Compilation successful
- ❌ Upload fails with "Programmer not found" or "A programmer is required"

## Next Steps

1. **Identify your UPDI hardware** (or lack thereof)
2. **Test with Arduino IDE** to confirm hardware works
3. **If Arduino IDE works but UniFlash doesn't**, the issue is in our configuration
4. **If neither works**, you need UPDI programming hardware

## Common Solutions

### For DIY SerialUPDI:
- Use Arduino Nano with jtag2updi firmware
- Add proper voltage level conversion
- Check all connections

### For Commercial UPDI:
- Install proper drivers
- Check device manager for COM port
- Try different baud rates

The key question: **What UPDI programming hardware do you have connected?** 