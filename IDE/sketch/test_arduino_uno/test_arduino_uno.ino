/*
 * Simple Arduino Uno Test
 * This will work with any Arduino Uno, Nano, or Mega
 * No special hardware required!
 */

void setup() {
  Serial.begin(9600);
  delay(1000);
  
  Serial.println("=== Arduino Uno Test ===");
  Serial.println("If you see this message, the IDE is working!");
  Serial.println("LED on pin 13 will blink every second");
  Serial.println("=========================");
  
  pinMode(13, OUTPUT);
}

void loop() {
  digitalWrite(13, HIGH);
  delay(500);
  digitalWrite(13, LOW);
  delay(500);
  
  Serial.println("LED blinked!");
} 