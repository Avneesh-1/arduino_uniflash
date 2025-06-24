# ESP32 Programming Guide for UniFlash IDE

## Overview
This guide covers programming ESP32 boards using the UniFlash IDE. ESP32 is a powerful microcontroller with built-in WiFi and Bluetooth capabilities.

## Supported ESP32 Boards
- **ESP32 WROOM** - Standard ESP32 development board
- **ESP32 Dev Module** - Development module variant
- **ESP32 WROVER** - ESP32 with PSRAM
- **ESP32 PICO** - Compact ESP32 variant
- **ESP32 S2** - Single-core ESP32 variant
- **ESP32 S3** - Dual-core ESP32 with enhanced features
- **ESP32 C3** - RISC-V based ESP32 variant

## Setup Requirements

### 1. Hardware Requirements
- ESP32 board (any supported variant)
- USB Type-C or micro-USB cable
- Computer with USB port

### 2. Software Requirements
- UniFlash IDE
- Arduino CLI (automatically configured)
- ESP32 board core (automatically installed)

### 3. Driver Installation
ESP32 boards typically use one of these USB-to-serial chips:
- **CP210x** (Silicon Labs) - Most common
- **CH340/CH341** (WCH)
- **FTDI** (Future Technology Devices)

The UniFlash project includes CP210x drivers in the `CP210x_Universal_Windows_Driver/` directory.

## Board Selection and Configuration

### 1. Select ESP32 Board
1. Open UniFlash IDE
2. In the device dropdown, select your ESP32 variant:
   - For standard ESP32: Choose "ESP32 WROOM" or "ESP32 Dev Module"
   - For ESP32 with PSRAM: Choose "ESP32 WROVER"
   - For compact boards: Choose "ESP32 PICO"

### 2. Port Selection
1. Connect your ESP32 board via USB
2. The board should appear in the COM port dropdown
3. Select the appropriate COM port (e.g., COM8)

### 3. Baud Rate
- ESP32 uses 115200 baud rate (automatically set)
- This is used for both programming and serial communication

## Programming ESP32

### 1. Basic ESP32 Sketch Structure
```cpp
// ESP32 Basic Sketch Template
void setup() {
  Serial.begin(115200);  // Initialize serial communication
  // Your setup code here
}

void loop() {
  // Your main code here
}
```

### 2. Pin Definitions
ESP32 has different pin layouts depending on the board variant:

#### Standard ESP32 Pins
- **Built-in LED**: GPIO 2 (most boards)
- **Analog Inputs**: GPIO 32-39 (ADC1), GPIO 0, 2, 4, 12-15, 25-27 (ADC2)
- **Digital I/O**: GPIO 0-39 (except input-only pins)
- **PWM**: Most GPIO pins support PWM
- **I2C**: GPIO 21 (SDA), GPIO 22 (SCL) - default
- **SPI**: GPIO 23 (MOSI), GPIO 19 (MISO), GPIO 18 (SCK), GPIO 5 (CS) - default

### 3. Example: Blinking LED
```cpp
const int LED_PIN = 2;  // Built-in LED on most ESP32 boards

void setup() {
  Serial.begin(115200);
  pinMode(LED_PIN, OUTPUT);
}

void loop() {
  digitalWrite(LED_PIN, HIGH);
  delay(1000);
  digitalWrite(LED_PIN, LOW);
  delay(1000);
}
```

## ESP32-Specific Features

### 1. WiFi Connectivity
```cpp
#include <WiFi.h>

const char* ssid = "YourWiFiNetwork";
const char* password = "YourWiFiPassword";

void setup() {
  Serial.begin(115200);
  
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.println("Connecting to WiFi...");
  }
  
  Serial.println("WiFi connected!");
  Serial.print("IP address: ");
  Serial.println(WiFi.localIP());
}
```

### 2. Web Server
```cpp
#include <WiFi.h>
#include <WebServer.h>

WebServer server(80);

void setup() {
  Serial.begin(115200);
  // WiFi setup code here...
  
  server.on("/", handleRoot);
  server.begin();
}

void loop() {
  server.handleClient();
}

void handleRoot() {
  server.send(200, "text/html", "<h1>ESP32 Web Server</h1>");
}
```

### 3. Bluetooth
```cpp
#include "BluetoothSerial.h"

BluetoothSerial SerialBT;

void setup() {
  Serial.begin(115200);
  SerialBT.begin("ESP32_BT");  // Bluetooth device name
}

void loop() {
  if (SerialBT.available()) {
    Serial.write(SerialBT.read());
  }
  if (Serial.available()) {
    SerialBT.write(Serial.read());
  }
}
```

## Upload Process

### 1. Automatic Setup
When you select an ESP32 board in UniFlash IDE:
1. The ESP32 board manager URL is automatically added
2. The ESP32 board core is installed if needed
3. Required libraries are detected and installed

### 2. Upload Steps
1. Write your code in the IDE
2. Select your ESP32 board from the device dropdown
3. Select the correct COM port
4. Click "Upload" button
5. Wait for compilation and upload to complete

### 3. Upload Troubleshooting

#### Common Issues:
- **"Failed to connect to ESP32"**: 
  - Press and hold the BOOT button on ESP32
  - Click upload, then release BOOT button when upload starts
  - Some boards require this for programming mode

- **"Port not found"**:
  - Check USB cable connection
  - Install appropriate USB drivers
  - Try different USB port

- **"Upload timeout"**:
  - Ensure ESP32 is in programming mode
  - Check baud rate settings
  - Try pressing RESET button on ESP32

## Serial Communication

### 1. Opening Serial Monitor
1. Select the correct COM port
2. Set baud rate to 115200
3. Click "Connect" button in the serial monitor panel

### 2. Serial Commands
ESP32 can receive commands via serial:
```cpp
void loop() {
  if (Serial.available()) {
    String command = Serial.readStringUntil('\n');
    command.trim();
    
    if (command == "led on") {
      digitalWrite(LED_PIN, HIGH);
      Serial.println("LED turned ON");
    }
    else if (command == "led off") {
      digitalWrite(LED_PIN, LOW);
      Serial.println("LED turned OFF");
    }
  }
}
```

## Power Management

### 1. Power Requirements
- **Operating Voltage**: 3.3V
- **Input Voltage**: 7-12V (via barrel jack) or 5V (via USB)
- **Current**: Up to 500mA during WiFi operation

### 2. Deep Sleep Mode
```cpp
#include <esp_sleep.h>

void setup() {
  Serial.begin(115200);
  Serial.println("Going to sleep for 10 seconds...");
  delay(1000);
  
  esp_deep_sleep(10000000);  // Sleep for 10 seconds
}

void loop() {
  // This code won't run after deep sleep
}
```

## Troubleshooting

### 1. Board Detection Issues
- Ensure ESP32 is properly connected
- Check USB drivers are installed
- Try different USB cable or port
- Use the "Debug Board Detection" feature in UniFlash IDE

### 2. Upload Failures
- Press BOOT button during upload if needed
- Check COM port selection
- Verify board type selection
- Ensure sufficient power supply

### 3. WiFi Issues
- Check WiFi credentials
- Ensure strong WiFi signal
- Verify WiFi library is included

## Advanced Features

### 1. Dual Core Programming
```cpp
#include <esp_task_wdt.h>

void setup() {
  Serial.begin(115200);
  
  // Create task on core 0
  xTaskCreatePinnedToCore(
    Task1code,   // Task function
    "Task1",     // Task name
    10000,       // Stack size
    NULL,        // Task parameters
    1,           // Task priority
    NULL,        // Task handle
    0            // Core to run on
  );
}

void Task1code(void * parameter) {
  for(;;) {
    Serial.println("Task 1 running on core 0");
    delay(1000);
  }
}
```

### 2. OTA (Over-The-Air) Updates
```cpp
#include <ArduinoOTA.h>

void setup() {
  Serial.begin(115200);
  // WiFi setup code here...
  
  ArduinoOTA.begin();
}

void loop() {
  ArduinoOTA.handle();
  // Your main code here
}
```

## Resources

### 1. Official Documentation
- [ESP32 Arduino Core Documentation](https://github.com/espressif/arduino-esp32)
- [ESP32 Technical Reference Manual](https://www.espressif.com/sites/default/files/documentation/esp32_technical_reference_manual_en.pdf)

### 2. Pin Reference
- [ESP32 Pin Reference](ESP32_Pin_Reference.md) - Detailed pin mapping
- [ESP32 Installation Guide](ESP32_Installation_Guide.md) - Installation instructions

### 3. Example Sketches
- `IDE/sketch/esp32_test_sketch/esp32_test_sketch.ino` - Basic functionality test
- Additional examples available in the IDE

## Support

If you encounter issues:
1. Check the output panel for error messages
2. Use the "Debug Board Detection" feature
3. Verify hardware connections
4. Check driver installation
5. Consult the troubleshooting section above

The UniFlash IDE provides comprehensive ESP32 support with automatic board detection, library management, and upload capabilities. 