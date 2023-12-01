#define TOUCH_SENSOR_PIN 2
#define START_DETECTING 0x01
#define RECEIVED 0x04
#include <Servo.h>

Servo myservo;
bool started = false;
bool detected = false;

void setup() {
  Serial.begin(115200);
  myservo.attach(9);
  pinMode(TOUCH_SENSOR_PIN, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(TOUCH_SENSOR_PIN), detect, RISING);
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
    // HIER SCHRIJVEN
    myservo.write(10);
    if (detected) {
      myservo.write(90);
      detected = false;
    }
    Serial.write(!digitalRead(TOUCH_SENSOR_PIN));
  }
}

void detect() {
  detected = true;
}