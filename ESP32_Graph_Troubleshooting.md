# ESP32 Graph and Excel Data Troubleshooting Guide

## Problem Description
ESP32 boards are working fine for uploading and basic functionality, but data is not appearing in the graphs and Excel sheets like it does for Arduino boards.

## Root Cause Analysis

The issue is likely related to one or more of the following:

1. **Data Format Mismatch**: ESP32 sketches may be sending data in a different format than what the graph manager expects
2. **Serial Communication Timing**: ESP32 may have different serial timing characteristics
3. **Baud Rate Issues**: ESP32 typically uses 115200 baud, which should be compatible
4. **Data Parsing**: The graph manager expects specific data markers

## Expected Data Format

The UniFlash graph manager expects data in one of these formats:

### Format 1: Marker-based (Recommended)
```
$Voltage$ 3.3V $TDS$ 500 $Temp$ 25.5
```

### Format 2: CSV format
```
3.3,500,25.5
```

## Solution Steps

### Step 1: Use the Correct ESP32 Sketch

Use one of these pre-configured ESP32 sketches:

1. **ESP32 Graph Test Sketch** (`IDE/sketch/esp32_graph_test/esp32_graph_test.ino`)
   - Sends test data in the exact format expected by UniFlash
   - Perfect for testing graph functionality
   - Simulates changing sensor values

2. **ESP32 Sensor Data Sketch** (`IDE/sketch/esp32_sensor_data/esp32_sensor_data.ino`)
   - Reads actual sensors and sends data in UniFlash format
   - Configured for TDS and temperature sensors

### Step 2: Verify Data Format

1. Upload the ESP32 Graph Test Sketch to your ESP32
2. Open the serial monitor in UniFlash IDE
3. Verify that data appears in this format:
   ```
   $Voltage$ 3.30V $TDS$ 500.0 $Temp$ 25.5
   ```

### Step 3: Check Serial Connection

1. Ensure ESP32 is connected to the correct COM port
2. Set baud rate to 115200
3. Click "Connect" in the serial monitor
4. Verify data is being received in the serial monitor

### Step 4: Test Graph Functionality

1. With the ESP32 connected and sending data:
2. Go to the main UniFlash window (Form1)
3. Connect to the same COM port
4. Verify that data appears in the graphs
5. Check that the Excel export works

## Debugging Commands

### ESP32 Commands (send via serial monitor)

```
read        - Send immediate sensor reading
csv         - Send data in CSV format
status      - Show current status and data format
help        - Show all available commands
```

### Debug Output

The enhanced GraphManager now includes debug output. Check the Visual Studio Output window for messages like:

```
Parsing data: '$Voltage$ 3.30V $TDS$ 500.0 $Temp$ 25.5'
Marker parsed: V='3.30'->3.3, TDS='500.0'->500, Temp='25.5'->25.5
Data added: Time=1.5, V=3.3, TDS=500, Temp=25.5
```

## Common Issues and Solutions

### Issue 1: No Data in Graphs
**Symptoms**: ESP32 is sending data but graphs remain empty

**Solutions**:
1. Verify data format matches exactly: `$Voltage$ X.XV $TDS$ XXX $Temp$ XX.X`
2. Check that data is being received in the serial monitor
3. Ensure you're connected to the same COM port in both IDE and main window
4. Try the ESP32 Graph Test Sketch first

### Issue 2: Data Appears in Serial Monitor but Not Graphs
**Symptoms**: Data visible in serial monitor, graphs not updating

**Solutions**:
1. Check debug output in Visual Studio Output window
2. Verify data parsing is successful
3. Ensure the main window is connected to the correct COM port
4. Try restarting the UniFlash application

### Issue 3: Excel Export Contains No Data
**Symptoms**: Graphs show data but Excel export is empty

**Solutions**:
1. Ensure data has been collected for some time before exporting
2. Check that the graph is actually displaying data
3. Try the "Export to Excel" button in the main window

### Issue 4: ESP32 Not Sending Data
**Symptoms**: No data appears in serial monitor

**Solutions**:
1. Verify ESP32 is properly connected
2. Check that the correct sketch is uploaded
3. Ensure ESP32 is powered properly
4. Try pressing the RESET button on ESP32
5. Check that the built-in LED is blinking (indicates sketch is running)

## Testing Procedure

### Quick Test
1. Upload `esp32_graph_test.ino` to ESP32
2. Open serial monitor in UniFlash IDE
3. Verify data format: `$Voltage$ 3.30V $TDS$ 500.0 $Temp$ 25.5`
4. Connect to same COM port in main UniFlash window
5. Verify graphs update with data
6. Test Excel export

### Sensor Test
1. Upload `esp32_sensor_data.ino` to ESP32
2. Connect sensors to GPIO 36 (TDS) and GPIO 39 (Temperature)
3. Follow the same testing procedure as above
4. Verify real sensor data appears in graphs

## Data Format Examples

### Correct Format (Will Work)
```
$Voltage$ 3.30V $TDS$ 500.0 $Temp$ 25.5
$Voltage$ 3.25V $TDS$ 450.0 $Temp$ 24.8
$Voltage$ 3.35V $TDS$ 550.0 $Temp$ 26.2
```

### CSV Format (Will Also Work)
```
3.30,500.0,25.5
3.25,450.0,24.8
3.35,550.0,26.2
```

### Incorrect Formats (Won't Work)
```
Voltage: 3.30V, TDS: 500, Temp: 25.5
3.30V, 500ppm, 25.5C
Sensor1: 3.30, Sensor2: 500, Sensor3: 25.5
```

## Advanced Troubleshooting

### Enable Debug Output
The GraphManager now includes debug output. To see it:
1. Run UniFlash in Visual Studio
2. Open the Output window (View → Output)
3. Look for debug messages when data is received

### Manual Data Testing
You can test the data parsing manually by sending test data through the serial monitor:
1. Connect to ESP32 via serial monitor
2. Send: `$Voltage$ 3.30V $TDS$ 500.0 $Temp$ 25.5`
3. Check if this appears in the graphs

### Timing Issues
If data appears intermittently:
1. Increase the delay in the ESP32 sketch
2. Check for serial buffer overflow
3. Ensure ESP32 has enough processing power

## Support

If you continue to have issues:
1. Check the debug output in Visual Studio
2. Verify the ESP32 sketch is sending data in the correct format
3. Test with the provided ESP32 Graph Test Sketch first
4. Ensure all connections are secure and proper

The ESP32 should work identically to Arduino boards once the correct data format is used. 