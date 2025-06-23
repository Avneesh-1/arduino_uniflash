/*
 * ATmega4809 Diagnostic Sketch
 * This will help identify UPDI programming issues
 */

void setup() {
  Serial.begin(9600);
  delay(1000);
  
  Serial.println("=== ATmega4809 Diagnostic ===");
  Serial.println("If you can see this message, the upload worked!");
  Serial.println("This means your UPDI programming setup is working correctly.");
  Serial.println("=============================");
  
  pinMode(13, OUTPUT);
}

void loop() {
  digitalWrite(13, HIGH);
  delay(1000);
  digitalWrite(13, LOW);
  delay(1000);
  
  Serial.println("ATmega4809 is running - LED blinking every 2 seconds");
} 