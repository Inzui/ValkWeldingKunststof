#define TOUCH_SENSOR_PIN 3
#define START_DETECTING 0x01
#define RECEIVED 0x04

bool started = false;

void setup() {
  Serial.begin(9600);
  pinMode(TOUCH_SENSOR_PIN, INPUT_PULLUP);
}

void loop() {
  int incomingByte = 0x00;
  while (Serial.available() > 0) {
    incomingByte = Serial.read();
  }
  if (incomingByte & START_DETECTING) {
    started = true;
    Serial.write(started);
  }
  if (incomingByte & RECEIVED) {
  }

  if (started) {
    Serial.write(!digitalRead(TOUCH_SENSOR_PIN));
  }
}