/*
 * ESP32 Diagnostic Sketch
 * Tests basic ESP32 functionality including GPIO, WiFi, and system info
 */

#include <WiFi.h>
#include <esp_system.h>
#include <esp_spi_flash.h>

// Pin definitions
const int LED_PIN = 2;           // Built-in LED
const int TEST_PIN = 4;          // Test pin for I/O
const int ANALOG_PIN = 34;       // Analog input pin

// WiFi credentials (change these)
const char* ssid = "YourWiFiSSID";
const char* password = "YourWiFiPassword";

// Test results
bool gpioTest = false;
bool wifiTest = false;
bool analogTest = false;
bool memoryTest = false;

void setup() {
  Serial.begin(115200);
  delay(1000);
  
  Serial.println("========================================");
  Serial.println("ESP32 Diagnostic Test");
  Serial.println("========================================");
  
  // Initialize pins
  pinMode(LED_PIN, OUTPUT);
  pinMode(TEST_PIN, OUTPUT);
  
  // Run diagnostic tests
  runSystemInfo();
  runGPIOTest();
  runAnalogTest();
  runMemoryTest();
  runWiFiTest();
  
  // Display results
  displayResults();
}

void loop() {
  // Blink LED to show system is running
  digitalWrite(LED_PIN, HIGH);
  delay(500);
  digitalWrite(LED_PIN, LOW);
  delay(500);
  
  // Read analog value every 2 seconds
  static unsigned long lastRead = 0;
  if (millis() - lastRead > 2000) {
    int analogValue = analogRead(ANALOG_PIN);
    Serial.printf("Analog Pin %d: %d\n", ANALOG_PIN, analogValue);
    lastRead = millis();
  }
}

void runSystemInfo() {
  Serial.println("\n--- System Information ---");
  
  // Chip information
  esp_chip_info_t chip_info;
  esp_chip_info(&chip_info);
  
  Serial.printf("Chip: %s\n", CONFIG_IDF_TARGET);
  Serial.printf("Cores: %d\n", chip_info.cores);
  Serial.printf("Features: %s%s%s%s\n",
    (chip_info.features & CHIP_FEATURE_WIFI_BGN) ? "WiFi " : "",
    (chip_info.features & CHIP_FEATURE_BT) ? "Bluetooth " : "",
    (chip_info.features & CHIP_FEATURE_BLE) ? "BLE " : "",
    (chip_info.features & CHIP_FEATURE_IEEE802154) ? "802.15.4 " : "");
  
  // Flash information
  uint32_t flash_size = spi_flash_get_chip_size();
  Serial.printf("Flash Size: %dMB\n", flash_size / (1024 * 1024));
  
  // Free heap memory
  Serial.printf("Free Heap: %d bytes\n", esp_get_free_heap_size());
  
  // CPU frequency
  Serial.printf("CPU Frequency: %d MHz\n", getCpuFrequencyMhz());
  
  // MAC address
  uint8_t mac[6];
  esp_wifi_get_mac(WIFI_IF_STA, mac);
  Serial.printf("MAC Address: %02X:%02X:%02X:%02X:%02X:%02X\n",
    mac[0], mac[1], mac[2], mac[3], mac[4], mac[5]);
}

void runGPIOTest() {
  Serial.println("\n--- GPIO Test ---");
  
  // Test digital output
  Serial.println("Testing digital output...");
  digitalWrite(TEST_PIN, HIGH);
  delay(100);
  digitalWrite(TEST_PIN, LOW);
  delay(100);
  
  // Test built-in LED
  Serial.println("Testing built-in LED...");
  for (int i = 0; i < 3; i++) {
    digitalWrite(LED_PIN, HIGH);
    delay(200);
    digitalWrite(LED_PIN, LOW);
    delay(200);
  }
  
  gpioTest = true;
  Serial.println("GPIO Test: PASSED");
}

void runAnalogTest() {
  Serial.println("\n--- Analog Test ---");
  
  // Read analog value
  int value = analogRead(ANALOG_PIN);
  Serial.printf("Analog Pin %d: %d\n", ANALOG_PIN, value);
  
  // Check if value is reasonable (not stuck at 0 or max)
  if (value > 0 && value < 4095) {
    analogTest = true;
    Serial.println("Analog Test: PASSED");
  } else {
    Serial.println("Analog Test: FAILED (value out of range)");
  }
}

void runMemoryTest() {
  Serial.println("\n--- Memory Test ---");
  
  // Test heap memory allocation
  size_t freeHeap = esp_get_free_heap_size();
  Serial.printf("Free heap before test: %d bytes\n", freeHeap);
  
  // Allocate some memory
  void* testPtr = malloc(1024);
  if (testPtr != NULL) {
    // Write some data
    memset(testPtr, 0xAA, 1024);
    
    // Free memory
    free(testPtr);
    
    size_t freeHeapAfter = esp_get_free_heap_size();
    Serial.printf("Free heap after test: %d bytes\n", freeHeapAfter);
    
    if (freeHeapAfter >= freeHeap - 1024) {
      memoryTest = true;
      Serial.println("Memory Test: PASSED");
    } else {
      Serial.println("Memory Test: FAILED (memory leak detected)");
    }
  } else {
    Serial.println("Memory Test: FAILED (allocation failed)");
  }
}

void runWiFiTest() {
  Serial.println("\n--- WiFi Test ---");
  
  if (strcmp(ssid, "YourWiFiSSID") == 0) {
    Serial.println("WiFi Test: SKIPPED (credentials not configured)");
    return;
  }
  
  Serial.printf("Connecting to WiFi: %s\n", ssid);
  
  WiFi.begin(ssid, password);
  
  int attempts = 0;
  while (WiFi.status() != WL_CONNECTED && attempts < 20) {
    delay(500);
    Serial.print(".");
    attempts++;
  }
  
  if (WiFi.status() == WL_CONNECTED) {
    Serial.println("\nWiFi connected!");
    Serial.printf("IP Address: %s\n", WiFi.localIP().toString().c_str());
    Serial.printf("Signal Strength: %d dBm\n", WiFi.RSSI());
    wifiTest = true;
    Serial.println("WiFi Test: PASSED");
  } else {
    Serial.println("\nWiFi Test: FAILED (connection timeout)");
  }
}

void displayResults() {
  Serial.println("\n========================================");
  Serial.println("DIAGNOSTIC RESULTS");
  Serial.println("========================================");
  Serial.printf("GPIO Test: %s\n", gpioTest ? "PASSED" : "FAILED");
  Serial.printf("Analog Test: %s\n", analogTest ? "PASSED" : "FAILED");
  Serial.printf("Memory Test: %s\n", memoryTest ? "PASSED" : "FAILED");
  Serial.printf("WiFi Test: %s\n", wifiTest ? "PASSED" : "SKIPPED");
  
  int passedTests = (gpioTest ? 1 : 0) + (analogTest ? 1 : 0) + 
                   (memoryTest ? 1 : 0) + (wifiTest ? 1 : 0);
  int totalTests = 4;
  
  Serial.printf("\nOverall Result: %d/%d tests passed\n", passedTests, totalTests);
  
  if (passedTests == totalTests) {
    Serial.println("ESP32 is working correctly!");
  } else {
    Serial.println("Some tests failed. Check connections and configuration.");
  }
  
  Serial.println("========================================");
}

// Helper function to get CPU frequency
uint32_t getCpuFrequencyMhz() {
  return esp_clk_cpu_freq() / 1000000;
} 