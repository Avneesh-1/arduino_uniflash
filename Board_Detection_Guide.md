# Board Detection and Compatibility Guide

## Overview

UniFlash now includes automatic board detection and compatibility validation. This feature helps prevent upload failures by detecting connected boards and warning about potential compatibility issues.

## How It Works

### 1. Automatic Board Detection
- Scans all available COM ports
- Uses `arduino-cli board list` to identify connected boards
- Extracts board type, vendor, and product information
- Provides fallback detection for unknown devices

### 2. Compatibility Validation
- Compares detected board with selected board type
- Checks against predefined compatibility rules
- Warns about potential mismatches
- Suggests appropriate board types

### 3. Smart Warnings
- Shows clear compatibility messages
- Provides specific guidance for different scenarios
- Allows users to continue or abort uploads

## Example Scenarios

### Scenario 1: ESP32 Connected, Arduino Uno Selected

**What happens:**
```
[Detecting connected boards...]
[Found 1 connected board(s):]
  - ESP32 ESP32 on COM3
[⚠ Incompatible: ESP32 ESP32 detected on COM3. Selected board type 'arduino:avr:uno' may not be suitable for this hardware.]
[⚠️ WARNING: Board compatibility issue detected!]
[Selected board type: arduino:avr:uno]
[Detected board: ESP32 ESP32 (esp32:esp32:esp32)]
[This may cause upload failures or incorrect behavior.]
[Consider selecting the correct board type for your hardware.]
```

**User should:**
- Change board selection to "ESP32 Dev Module"
- Or connect an Arduino Uno instead

### Scenario 2: ATmega4809 Connected, Arduino Uno Selected

**What happens:**
```
[Detecting connected boards...]
[Found 1 connected board(s):]
  - ATmega4809 on COM4
[⚠ Incompatible: ATmega4809 detected on COM4. Selected board type 'arduino:avr:uno' may not be suitable for this hardware.]
[⚠️ WARNING: Board compatibility issue detected!]
[Selected board type: arduino:avr:uno]
[Detected board: ATmega4809 (MegaCoreX:megaavr:4809)]
[This may cause upload failures or incorrect behavior.]
[Consider selecting the correct board type for your hardware.]
```

**User should:**
- Change board selection to "ATmega4809"
- Ensure UPDI programming hardware is connected

### Scenario 3: Arduino Uno Connected, Arduino Uno Selected

**What happens:**
```
[Detecting connected boards...]
[Found 1 connected board(s):]
  - Arduino UNO on COM3
[✓ Compatible: Arduino UNO detected on COM3]
[Compiling sketch...]
[Uploading... Please wait]
[Upload finished]
```

**Result:** Successful upload with no warnings

## Supported Board Types

### Arduino Boards
- **arduino:avr:uno** - Arduino Uno
- **arduino:avr:nano** - Arduino Nano
- **arduino:avr:mega** - Arduino Mega
- **arduino:avr:leonardo** - Arduino Leonardo
- And many more...

### MegaCoreX Boards
- **MegaCoreX:megaavr:4809** - ATmega4809
- **MegaCoreX:megaavr:4808** - ATmega4808
- **MegaCoreX:megaavr:3208** - ATmega3208

### ESP32 Boards
- **esp32:esp32:esp32** - ESP32 Dev Module
- **esp32:esp32:esp32s3** - ESP32-S3
- **esp32:esp32:esp32c3** - ESP32-C3

### ESP8266 Boards
- **esp8266:esp8266:nodemcuv2** - NodeMCU v2
- **esp8266:esp8266:esp01** - ESP-01

## Compatibility Rules

### Arduino Family Compatibility
- Arduino Uno, Nano, Mega are generally compatible
- Same core (avr) boards can often share code
- Different pin layouts may require code adjustments

### MegaCoreX Family Compatibility
- ATmega4809, ATmega4808, ATmega3208 are compatible
- All use MegaCoreX core
- Pin definitions may vary between models

### ESP32 Family Compatibility
- ESP32, ESP32-S3, ESP32-C3 are compatible
- All use ESP32 core
- Some features may not be available on all variants

### ESP8266 Family Compatibility
- NodeMCU v2, ESP-01 are compatible
- All use ESP8266 core
- Pin layouts differ significantly

## Error Messages and Solutions

### "No boards detected on any COM port"
**Causes:**
- No board connected
- Driver issues
- Wrong USB cable (charge-only cable)

**Solutions:**
- Check physical connections
- Install proper drivers
- Try different USB cable
- Restart the application

### "No board detected on port COM3"
**Causes:**
- Board not properly connected
- Port in use by another application
- Driver issues

**Solutions:**
- Reconnect the board
- Close other applications using the port
- Check device manager for port status

### "Board compatibility issue detected"
**Causes:**
- Wrong board type selected
- Incompatible hardware

**Solutions:**
- Select correct board type
- Use compatible hardware
- Check board documentation

## Testing the Feature

You can test the board detection feature using the provided test program:

```csharp
await BoardDetectionTest.RunTest();
```

This will:
1. Detect all connected boards
2. Test compatibility with different board types
3. Find the best compatible board for each type

## Integration with Existing Code

The board detection is automatically integrated into the upload process:

```csharp
var uploader = new ArduinoUploader();
uploader.SetBoardType("arduino:avr:uno");

// Board detection happens automatically during upload
bool success = await uploader.UploadCode(code, "COM3", progress);
```

## Benefits

1. **Prevents Upload Failures** - Catches compatibility issues before upload
2. **Better User Experience** - Clear warnings and guidance
3. **Reduces Confusion** - Helps users select correct board types
4. **Saves Time** - Identifies issues early in the process
5. **Educational** - Teaches users about board compatibility

## Future Enhancements

- Automatic board type suggestion
- Driver installation guidance
- Board-specific code templates
- Advanced compatibility checking
- Support for more board types 