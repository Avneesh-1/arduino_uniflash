# ESP32 Installation Guide for UniFlash

## Overview
This guide explains how to set up ESP32 support in UniFlash and Arduino IDE for programming ESP32 WROOM boards.

## Prerequisites
- Arduino IDE installed
- USB cable for ESP32 connection
- Windows 10/11 (for CP210x driver installation)

## Step 1: Install CP210x USB Driver

### Automatic Installation
1. Download the CP210x Universal Windows Driver from Silicon Labs
2. Extract the driver package
3. Run the installer as Administrator
4. Follow the installation wizard

### Manual Installation
1. Connect your ESP32 board via USB
2. Open Device Manager
3. Look for "Unknown Device" or "CP210x USB to UART Bridge"
4. Right-click and select "Update Driver"
5. Choose "Browse my computer for drivers"
6. Navigate to the CP210x driver folder
7. Select the appropriate architecture (x64, x86, or ARM64)

## Step 2: Install ESP32 Board Package

### Method 1: Using Arduino IDE
1. Open Arduino IDE
2. Go to **File > Preferences**
3. In "Additional Board Manager URLs", add:
   ```
   https://raw.githubusercontent.com/espressif/arduino-esp32/gh-pages/package_esp32_index.json
   ```
4. Click **OK**
5. Go to **Tools > Board > Boards Manager**
6. Search for "ESP32"
7. Install "ESP32 by Espressif Systems"
8. Wait for installation to complete

### Method 2: Using arduino-cli (UniFlash)
1. Open UniFlash
2. Go to **Tools > Board Manager**
3. Search for "ESP32"
4. Install "ESP32 by Espressif Systems"
5. Wait for installation to complete

## Step 3: Configure Board Settings

### Recommended Settings for ESP32 WROOM
- **Board**: "ESP32 Dev Module"
- **Upload Speed**: 921600
- **CPU Frequency**: 240MHz
- **Flash Frequency**: 80MHz
- **Flash Mode**: QIO
- **Flash Size**: 4MB (32Mb)
- **Partition Scheme**: Default 4MB with spiffs
- **Core Debug Level**: None
- **PSRAM**: Disabled

## Step 4: Test Installation

### Basic Test Sketch
```cpp
void setup() {
  Serial.begin(115200);
  pinMode(2, OUTPUT);  // Built-in LED
}

void loop() {
  digitalWrite(2, HIGH);
  delay(1000);
  digitalWrite(2, LOW);
  delay(1000);
  Serial.println("ESP32 is working!");
}
```

### Upload Process
1. Connect ESP32 via USB cable
2. Select the correct COM port
3. Press and hold the **BOOT** button on the ESP32
4. Click **Upload** in Arduino IDE or UniFlash
5. Release the **BOOT** button when upload starts
6. Monitor serial output at 115200 baud

## Step 5: Verify Installation

### Expected Serial Output
```
ESP32 is working!
ESP32 is working!
ESP32 is working!
...
```

### LED Behavior
- Built-in LED should blink every second
- LED is connected to GPIO2 (active LOW)

## Troubleshooting

### Common Issues

#### 1. "Failed to connect to ESP32"
**Solution:**
- Hold the **BOOT** button during upload
- Try different upload speeds (115200, 460800, 921600)
- Check USB cable and port

#### 2. "Wrong chip detected"
**Solution:**
- Verify board selection is "ESP32 Dev Module"
- Check if correct board package is installed
- Try resetting the board

#### 3. "Upload timeout"
**Solution:**
- Reduce upload speed to 115200
- Hold **BOOT** button during upload
- Check USB connection

#### 4. "Port not found"
**Solution:**
- Install CP210x driver
- Try different USB cable
- Check Device Manager for COM port

#### 5. "Compilation error"
**Solution:**
- Verify ESP32 board package is installed
- Check board settings
- Update Arduino IDE to latest version

### Driver Issues

#### Windows Driver Installation
1. Download latest CP210x driver from Silicon Labs
2. Run installer as Administrator
3. Restart computer after installation
4. Check Device Manager for proper COM port

#### Linux/Mac Installation
- Linux: Usually works out of the box
- Mac: Install CP210x driver if needed

## Advanced Configuration

### Custom Board Settings
You can create custom board configurations for specific ESP32 variants:

1. Go to **Tools > Board > ESP32 Arduino**
2. Select your specific board variant
3. Adjust settings as needed

### Partition Schemes
- **Default 4MB with spiffs**: General purpose
- **Huge APP (3MB No OTA/1MB SPIFFS)**: Large applications
- **Minimal SPIFFS (1.3MB APP/1.5MB SPIFFS)**: File system focus
- **No OTA (Large APP)**: Maximum application space

### Upload Methods
- **UART0 / Hardware CDC**: Standard method
- **UART0 / Hardware CDC + Flash Mode**: For problematic boards
- **UART0 / Hardware CDC + Reset Mode**: Alternative reset method

## Testing Your Setup

### Run Diagnostic Sketch
1. Open the ESP32 diagnostic sketch in UniFlash
2. Upload to your ESP32
3. Monitor serial output for test results
4. Verify all tests pass

### Test WiFi Functionality
```cpp
#include <WiFi.h>

const char* ssid = "YourWiFiSSID";
const char* password = "YourWiFiPassword";

void setup() {
  Serial.begin(115200);
  WiFi.begin(ssid, password);
  
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  
  Serial.println("\nWiFi connected!");
  Serial.println(WiFi.localIP());
}
```

## Next Steps
1. Explore ESP32 libraries and examples
2. Learn about WiFi and Bluetooth capabilities
3. Experiment with different sensors and modules
4. Build your first ESP32 project

## Resources
- [ESP32 Arduino Core Documentation](https://github.com/espressif/arduino-esp32)
- [ESP32 Datasheet](https://www.espressif.com/sites/default/files/documentation/esp32_datasheet_en.pdf)
- [CP210x Driver Downloads](https://www.silabs.com/developers/usb-to-uart-bridge-vcp-drivers)
- [ESP32 Pinout Reference](https://randomnerdtutorials.com/esp32-pinout-reference-gpios/)

## Support
If you encounter issues:
1. Check the troubleshooting section above
2. Verify all installation steps were completed
3. Test with a simple blink sketch
4. Check serial monitor for error messages
5. Consult ESP32 community forums 