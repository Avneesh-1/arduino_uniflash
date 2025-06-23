# MegaCoreX Installation Guide for ATmega4809

If the automatic installation fails, follow these manual steps:

## Prerequisites
1. Make sure Arduino CLI is installed and accessible from command line
2. Open Command Prompt as Administrator

## Manual Installation Steps

### Step 1: Add MegaCoreX Board Manager URL
```bash
arduino-cli config add board_manager.additional_urls https://mcudude.github.io/MegaCoreX/package_MCUdude_MegaCoreX_index.json
```

### Step 2: Update Board Index
```bash
arduino-cli core update-index
```

### Step 3: Install MegaCoreX Core
```bash
arduino-cli core install MegaCoreX:megaavr
```

### Step 4: Verify Installation
```bash
arduino-cli core list
```
You should see `MegaCoreX:megaavr` in the list.

### Step 5: List Available Boards
```bash
arduino-cli board listall MegaCoreX
```
This will show all available ATmega4809 board variants.

## Troubleshooting

### If you get "platform not found" error:
1. Check if the URL was added correctly:
   ```bash
   arduino-cli config dump
   ```
   Look for the `board_manager.additional_urls` section.

2. If the URL is missing, add it manually:
   ```bash
   arduino-cli config add board_manager.additional_urls https://mcudude.github.io/MegaCoreX/package_MCUdude_MegaCoreX_index.json
   ```

### If you get network errors:
1. Check your internet connection
2. Try using a VPN if the repository is blocked in your region
3. Check if your firewall is blocking the connection

### If you get permission errors:
1. Run Command Prompt as Administrator
2. Make sure you have write permissions to the Arduino15 folder

## Testing the Installation

After successful installation, try uploading a simple sketch:

1. Open UniFlash IDE
2. Select your ATmega4809 device
3. Use the test sketch provided in `IDE/sketch.ino`
4. Click Upload

The sketch should compile and upload successfully.

## Common Board Variants

- `MegaCoreX:megaavr:4809` - Standard ATmega4809
- `MegaCoreX:megaavr:4809:clock=internal_20MHz` - Internal 20MHz clock
- `MegaCoreX:megaavr:4809:clock=external_16MHz` - External 16MHz crystal

## Programmer Options

For ATmega4809, you can use:
- SerialUPDI (most common)
- Atmel-ICE
- Curiosity Nano
- JTAG2UPDI

Select the appropriate programmer in the IDE based on your hardware setup.

## Important Notes

- The correct core ID is `MegaCoreX:megaavr` (not `MegaCoreX:megaCoreX`)
- The correct FQBN for ATmega4809 is `MegaCoreX:megaavr:4809`
- Make sure to use the correct board type in your code 