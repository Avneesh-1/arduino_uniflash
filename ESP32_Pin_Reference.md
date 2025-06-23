# ESP32 Pin Reference Guide

## Pin Layout Overview
The ESP32 has 34 GPIO pins, but not all are available for general use. Some pins are reserved for boot, flash memory, and other system functions.

## GPIO Pin Functions

### Digital I/O Pins
| Pin | GPIO | Function | Notes |
|-----|------|----------|-------|
| D0  | GPIO0  | Digital I/O | Boot mode selection |
| D1  | GPIO1  | TX0 | Serial TX (connected to USB) |
| D2  | GPIO2  | Digital I/O | Built-in LED |
| D3  | GPIO3  | RX0 | Serial RX (connected to USB) |
| D4  | GPIO4  | Digital I/O | General purpose |
| D5  | GPIO5  | Digital I/O | SPI SS (default) |
| D6  | GPIO6  | Flash | Reserved for flash memory |
| D7  | GPIO7  | Flash | Reserved for flash memory |
| D8  | GPIO8  | Flash | Reserved for flash memory |
| D9  | GPIO9  | Flash | Reserved for flash memory |
| D10 | GPIO10 | Flash | Reserved for flash memory |
| D11 | GPIO11 | Flash | Reserved for flash memory |
| D12 | GPIO12 | Digital I/O | Boot mode selection |
| D13 | GPIO13 | Digital I/O | General purpose |
| D14 | GPIO14 | Digital I/O | General purpose |
| D15 | GPIO15 | Digital I/O | General purpose |
| D16 | GPIO16 | Digital I/O | UART2 RX |
| D17 | GPIO17 | Digital I/O | UART2 TX |
| D18 | GPIO18 | Digital I/O | SPI SCK |
| D19 | GPIO19 | Digital I/O | SPI MISO |
| D21 | GPIO21 | Digital I/O | I2C SDA |
| D22 | GPIO22 | Digital I/O | I2C SCL |
| D23 | GPIO23 | Digital I/O | SPI MOSI |
| D25 | GPIO25 | Digital I/O | DAC1 |
| D26 | GPIO26 | Digital I/O | DAC2 |
| D27 | GPIO27 | Digital I/O | General purpose |
| D32 | GPIO32 | Digital I/O | General purpose |
| D33 | GPIO33 | Digital I/O | General purpose |
| D34 | GPIO34 | Input only | ADC1_CH6 |
| D35 | GPIO35 | Input only | ADC1_CH7 |
| D36 | GPIO36 | Input only | ADC1_CH0 |
| D39 | GPIO39 | Input only | ADC1_CH3 |

## Special Pins

### Boot and Flash Pins
- **GPIO0**: Boot mode selection (must be LOW during boot for download mode)
- **GPIO2**: Built-in LED (active LOW)
- **GPIO6-11**: Reserved for flash memory (do not use)
- **GPIO12**: Boot mode selection (must be HIGH during boot)

### Serial Communication
- **GPIO1 (TX0)**: Serial transmit (connected to USB)
- **GPIO3 (RX0)**: Serial receive (connected to USB)
- **GPIO16 (RX2)**: UART2 receive
- **GPIO17 (TX2)**: UART2 transmit

### I2C (Default)
- **GPIO21 (SDA)**: I2C data line
- **GPIO22 (SCL)**: I2C clock line

### SPI (Default)
- **GPIO18 (SCK)**: SPI clock
- **GPIO19 (MISO)**: SPI master in, slave out
- **GPIO23 (MOSI)**: SPI master out, slave in
- **GPIO5 (SS)**: SPI slave select

### Analog Inputs
- **GPIO34**: ADC1_CH6 (Input only)
- **GPIO35**: ADC1_CH7 (Input only)
- **GPIO36**: ADC1_CH0 (Input only)
- **GPIO39**: ADC1_CH3 (Input only)
- **GPIO32**: ADC1_CH4
- **GPIO33**: ADC1_CH5
- **GPIO25**: ADC2_CH8, DAC1
- **GPIO26**: ADC2_CH9, DAC2
- **GPIO27**: ADC2_CH7
- **GPIO14**: ADC2_CH6
- **GPIO12**: ADC2_CH5
- **GPIO13**: ADC2_CH4
- **GPIO15**: ADC2_CH3
- **GPIO2**: ADC2_CH2
- **GPIO0**: ADC2_CH1
- **GPIO4**: ADC2_CH0

### DAC (Digital to Analog Converter)
- **GPIO25**: DAC1 (8-bit)
- **GPIO26**: DAC2 (8-bit)

## Pin Usage Guidelines

### Safe to Use Pins
These pins are safe for general use:
- GPIO4, GPIO5, GPIO12, GPIO13, GPIO14, GPIO15
- GPIO16, GPIO17, GPIO18, GPIO19, GPIO21, GPIO22, GPIO23
- GPIO25, GPIO26, GPIO27, GPIO32, GPIO33

### Input Only Pins
These pins can only be used as inputs:
- GPIO34, GPIO35, GPIO36, GPIO39

### Reserved Pins (Avoid Using)
- GPIO6-11: Flash memory
- GPIO0: Boot mode (use carefully)
- GPIO2: Built-in LED (can be used but affects LED)

### Special Considerations
- **GPIO0**: Must be HIGH for normal boot, LOW for download mode
- **GPIO2**: Connected to built-in LED (active LOW)
- **GPIO12**: Must be HIGH during boot
- **GPIO6-11**: Reserved for flash memory access

## Example Pin Configurations

### Basic Digital I/O
```cpp
const int ledPin = 2;      // Built-in LED
const int buttonPin = 4;   // Safe GPIO pin

void setup() {
  pinMode(ledPin, OUTPUT);
  pinMode(buttonPin, INPUT_PULLUP);
}
```

### I2C Configuration
```cpp
#include <Wire.h>

void setup() {
  Wire.begin(21, 22);  // SDA=21, SCL=22
}
```

### SPI Configuration
```cpp
#include <SPI.h>

void setup() {
  SPI.begin(18, 19, 23, 5);  // SCK, MISO, MOSI, SS
}
```

### Analog Input
```cpp
const int analogPin = 34;  // Input only pin

void setup() {
  Serial.begin(115200);
}

void loop() {
  int value = analogRead(analogPin);
  Serial.println(value);
}
```

### PWM Output
```cpp
const int pwmPin = 25;  // DAC1 pin

void setup() {
  ledcSetup(0, 5000, 8);  // Channel 0, 5kHz, 8-bit resolution
  ledcAttachPin(pwmPin, 0);
}

void loop() {
  for(int i = 0; i < 255; i++) {
    ledcWrite(0, i);
    delay(10);
  }
}
```

## Power and Ground
- **3.3V**: 3.3V power output
- **5V**: 5V power input/output
- **GND**: Ground
- **EN**: Enable pin (reset)

## USB Interface
The ESP32 uses a CP210x USB-to-Serial converter:
- **TX**: GPIO1
- **RX**: GPIO3
- **DTR**: Connected to GPIO0 for auto-reset
- **RTS**: Connected to GPIO0 for auto-reset

## Boot Process
1. **Normal Boot**: GPIO0 = HIGH, GPIO2 = HIGH
2. **Download Mode**: GPIO0 = LOW (hold during reset)
3. **Flash Mode**: GPIO0 = LOW, GPIO2 = LOW

## Tips for Reliable Operation
1. Use pull-up resistors for input pins when needed
2. Avoid using flash memory pins (GPIO6-11)
3. Be careful with GPIO0 and GPIO2 during boot
4. Use appropriate voltage levels (3.3V logic)
5. Consider current limitations for output pins
6. Use proper debouncing for buttons
7. Implement proper error handling for critical pins 