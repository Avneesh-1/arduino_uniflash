# ESP32 Programming Guide for UniFlash

## Overview
The ESP32 is a powerful microcontroller with built-in WiFi and Bluetooth capabilities. This guide covers programming ESP32 boards using UniFlash.

## ESP32 WROOM Board Specifications
- **Microcontroller**: ESP32 (Dual-core 240MHz)
- **Flash Memory**: 4MB
- **RAM**: 520KB SRAM
- **Connectivity**: WiFi 802.11 b/g/n, Bluetooth 4.2
- **USB Interface**: CP210x USB-to-Serial converter
- **Operating Voltage**: 3.3V
- **Input Voltage**: 7-12V (recommended)

## Pin Definitions
```cpp
// Digital Pins (GPIO)
#define GPIO0  0
#define GPIO1  1   // TX0
#define GPIO2  2
#define GPIO3  3   // RX0
#define GPIO4  4
#define GPIO5  5
#define GPIO6  6   // Flash memory
#define GPIO7  7   // Flash memory
#define GPIO8  8   // Flash memory
#define GPIO9  9   // Flash memory
#define GPIO10 10  // Flash memory
#define GPIO11 11  // Flash memory
#define GPIO12 12
#define GPIO13 13
#define GPIO14 14
#define GPIO15 15
#define GPIO16 16
#define GPIO17 17
#define GPIO18 18
#define GPIO19 19
#define GPIO21 21
#define GPIO22 22
#define GPIO23 23
#define GPIO25 25
#define GPIO26 26
#define GPIO27 27
#define GPIO32 32
#define GPIO33 33
#define GPIO34 34  // Input only
#define GPIO35 35  // Input only
#define GPIO36 36  // Input only
#define GPIO39 39  // Input only

// Built-in LED
#define LED_BUILTIN 2

// I2C (default)
#define SDA 21
#define SCL 22

// SPI (default)
#define MOSI 23
#define MISO 19
#define SCK  18
#define SS   5

// UART
#define TX0 1
#define RX0 3
#define TX2 17
#define RX2 16
```

## Required Software Setup

### 1. Install ESP32 Board Package
1. Open Arduino IDE
2. Go to File > Preferences
3. Add this URL to "Additional Board Manager URLs":
   ```
   https://raw.githubusercontent.com/espressif/arduino-esp32/gh-pages/package_esp32_index.json
   ```
4. Go to Tools > Board > Boards Manager
5. Search for "ESP32" and install "ESP32 by Espressif Systems"

### 2. Board Configuration
- **Board**: "ESP32 Dev Module"
- **Upload Speed**: 921600
- **CPU Frequency**: 240MHz
- **Flash Frequency**: 80MHz
- **Flash Mode**: QIO
- **Flash Size**: 4MB (32Mb)
- **Partition Scheme**: Default 4MB with spiffs
- **Core Debug Level**: None
- **PSRAM**: Disabled

## Basic ESP32 Sketch
```cpp
#include <WiFi.h>

const char* ssid = "YourWiFiSSID";
const char* password = "YourWiFiPassword";

void setup() {
  Serial.begin(115200);
  pinMode(LED_BUILTIN, OUTPUT);
  
  // Connect to WiFi
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  
  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
}

void loop() {
  digitalWrite(LED_BUILTIN, HIGH);
  delay(1000);
  digitalWrite(LED_BUILTIN, LOW);
  delay(1000);
}
```

## Common Libraries for ESP32
- **WiFi**: Built-in WiFi functionality
- **WebServer**: HTTP server capabilities
- **ESPAsyncWebServer**: Asynchronous web server
- **PubSubClient**: MQTT client
- **ArduinoJson**: JSON parsing and generation
- **SPIFFS**: File system for ESP32
- **Preferences**: Non-volatile storage

## Upload Process
1. Connect ESP32 via USB cable
2. Select correct COM port
3. Press and hold BOOT button while uploading
4. Release BOOT button after upload starts
5. Monitor serial output at 115200 baud

## Troubleshooting

### Upload Issues
- **"Failed to connect to ESP32"**: Hold BOOT button during upload
- **"Wrong chip detected"**: Check board selection
- **"Upload timeout"**: Try different upload speed

### WiFi Issues
- Check SSID and password
- Ensure 2.4GHz network (ESP32 doesn't support 5GHz)
- Check signal strength

### Memory Issues
- Use PROGMEM for large strings
- Optimize code size
- Use appropriate partition scheme

## Advanced Features

### Deep Sleep
```cpp
#include <esp_sleep.h>

void setup() {
  Serial.begin(115200);
  Serial.println("Going to sleep...");
  esp_deep_sleep(10000000); // Sleep for 10 seconds
}

void loop() {
  // This will not run after deep sleep
}
```

### OTA (Over-The-Air) Updates
```cpp
#include <WiFi.h>
#include <ESPAsyncWebServer.h>
#include <AsyncElegantOTA.h>

AsyncWebServer server(80);

void setup() {
  // WiFi setup...
  server.on("/", HTTP_GET, [](AsyncWebServerRequest *request){
    request->send(200, "text/plain", "ESP32 OTA Ready");
  });
  
  AsyncElegantOTA.begin(&server);
  server.begin();
}
```

## Best Practices
1. Always use `Serial.begin(115200)` for debugging
2. Handle WiFi connection errors gracefully
3. Use appropriate pin modes for input/output
4. Consider power consumption for battery applications
5. Use proper error handling for network operations
6. Implement watchdog timers for stability

## Resources
- [ESP32 Arduino Core Documentation](https://github.com/espressif/arduino-esp32)
- [ESP32 Datasheet](https://www.espressif.com/sites/default/files/documentation/esp32_datasheet_en.pdf)
- [ESP32 Programming Guide](https://docs.espressif.com/projects/esp-idf/en/latest/esp32/)
- [ESP32 Pinout Reference](https://randomnerdtutorials.com/esp32-pinout-reference-gpios/) 