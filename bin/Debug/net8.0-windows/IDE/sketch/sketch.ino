void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
}

void loop() {
  // put your main code here, to run repeatedly:
  Serial.print("TDS : ");
  Serial.println("110 PPM");

  Serial.print("Voltage : ");
  Serial.println("5.5 V");

  Serial.print("Current : ");
  Serial.println("11.5 A");

  delay(1000);
}