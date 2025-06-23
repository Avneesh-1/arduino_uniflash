# ATmega4809 Pin Reference Guide for MegaCoreX

## ⚠️ IMPORTANT: Pin Naming Differences

**ATmega4809 with MegaCoreX uses DIFFERENT pin naming than other boards:**

- ❌ `PA5`, `PB3`, `PC1` - **STM32 style** (NOT valid for ATmega4809)
- ✅ `A5`, `3`, `1` - **ATmega4809 style** (CORRECT)

## Digital Pins (0-13)

| Pin Name | Physical Pin | Function |
|----------|--------------|----------|
| `0` or `PIN_0` | Pin 0 | Digital I/O |
| `1` or `PIN_1` | Pin 1 | Digital I/O |
| `2` or `PIN_2` | Pin 2 | Digital I/O |
| `3` or `PIN_3` | Pin 3 | Digital I/O |
| `4` or `PIN_4` | Pin 4 | Digital I/O |
| `5` or `PIN_5` | Pin 5 | Digital I/O |
| `6` or `PIN_6` | Pin 6 | Digital I/O |
| `7` or `PIN_7` | Pin 7 | Digital I/O |
| `8` or `PIN_8` | Pin 8 | Digital I/O |
| `9` or `PIN_9` | Pin 9 | Digital I/O |
| `10` or `PIN_10` | Pin 10 | Digital I/O |
| `11` or `PIN_11` | Pin 11 | Digital I/O |
| `12` or `PIN_12` | Pin 12 | Digital I/O |
| `13` or `PIN_13` or `LED_BUILTIN` | Pin 13 | Digital I/O, Built-in LED |

## Analog Pins (A0-A7)

| Pin Name | Physical Pin | Function |
|----------|--------------|----------|
| `A0` | Pin 14 | Analog Input / Digital I/O |
| `A1` | Pin 15 | Analog Input / Digital I/O |
| `A2` | Pin 16 | Analog Input / Digital I/O |
| `A3` | Pin 17 | Analog Input / Digital I/O |
| `A4` | Pin 18 | Analog Input / Digital I/O |
| `A5` | Pin 19 | Analog Input / Digital I/O |
| `A6` | Pin 20 | Analog Input / Digital I/O |
| `A7` | Pin 21 | Analog Input / Digital I/O |

## Common Pin Definitions

```cpp
// Built-in LED
#define LED_BUILTIN 13

// Example LED pin (using analog pin as digital output)
#define LED_Drive A5

// Digital pins
#define BUTTON_PIN 2
#define RELAY_PIN 4

// Analog pins
#define TEMP_SENSOR A0
#define LIGHT_SENSOR A1
```

## What NOT to Use

❌ **These pin definitions are NOT valid for ATmega4809:**
```cpp
#define LED_Drive PA5    // WRONG - STM32 style
#define LED_Drive PB3    // WRONG - STM32 style
#define LED_Drive PC1    // WRONG - STM32 style
```

✅ **Use these instead:**
```cpp
#define LED_Drive A5     // CORRECT - ATmega4809 analog pin 5
#define LED_Drive 5      // CORRECT - ATmega4809 digital pin 5
#define LED_Drive 13     // CORRECT - ATmega4809 built-in LED
```

## Example Sketch

```cpp
// Correct ATmega4809 sketch
#define LED_Drive A5  // Use A5, not PA5!

void setup() {
  pinMode(LED_Drive, OUTPUT);
}

void loop() {
  digitalWrite(LED_Drive, HIGH);
  delay(1000);
  digitalWrite(LED_Drive, LOW);
  delay(1000);
}
```

## Why This Matters

- **ATmega4809** is a different microcontroller family than STM32
- **MegaCoreX** provides ATmega4809 support with specific pin naming
- Using wrong pin names causes compilation errors
- The compiler will suggest the correct alternative (like `A5` instead of `PA5`)

## Troubleshooting

If you get errors like:
```
error: 'PA5' was not declared in this scope
note: suggested alternative: 'A5'
```

**Solution:** Replace `PA5` with `A5` (or the suggested alternative). 