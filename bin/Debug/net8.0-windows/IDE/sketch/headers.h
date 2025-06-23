#ifndef HEADERS_H
#define HEADERS_H

// Pin definitions for BME63M001
#define TDS_RX_PIN 6
#define TDS_TX_PIN 7

// Pin definitions for SoftwareSerial
#define SERIAL_RX_PIN 10
#define SERIAL_TX_PIN 11

// Baud rates
#define SERIAL_BAUD 9600
#define SOFTWARE_SERIAL_BAUD 4800

// ATmega4809 Pin Definitions for MegaCoreX
// Digital Pins (can be used as INPUT or OUTPUT)
#define PIN_0  0   // Digital pin 0
#define PIN_1  1   // Digital pin 1
#define PIN_2  2   // Digital pin 2
#define PIN_3  3   // Digital pin 3
#define PIN_4  4   // Digital pin 4
#define PIN_5  5   // Digital pin 5
#define PIN_6  6   // Digital pin 6
#define PIN_7  7   // Digital pin 7
#define PIN_8  8   // Digital pin 8
#define PIN_9  9   // Digital pin 9
#define PIN_10 10  // Digital pin 10
#define PIN_11 11  // Digital pin 11
#define PIN_12 12  // Digital pin 12
#define PIN_13 13  // Digital pin 13 (built-in LED)

// Analog Pins (can be used as analog INPUT or digital INPUT/OUTPUT)
#define A0 14  // Analog pin 0
#define A1 15  // Analog pin 1
#define A2 16  // Analog pin 2
#define A3 17  // Analog pin 3
#define A4 18  // Analog pin 4
#define A5 19  // Analog pin 5
#define A6 20  // Analog pin 6
#define A7 21  // Analog pin 7

// Built-in LED (same as PIN_13)
#define LED_BUILTIN 13

// Common pin aliases for ATmega4809
#define LED_PIN 13      // Built-in LED
#define LED_Drive A5    // Example: Using A5 as LED drive pin

#endif 