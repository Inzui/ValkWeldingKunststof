#include <Stepper.h>

#define TOUCH_SENSOR_PIN 3
#define START_MOVING 0x01
#define SEND_DISTANCE 0x02
#define RECEIVED 0x04

#define FORWARD_SPEED 100
#define BACKWARD_SPEED 100

Stepper stepper(64, 4, 5, 6 ,7);
int movedSteps = 0;

void setup() {
  Serial.begin(9600);
  pinMode(TOUCH_SENSOR_PIN, INPUT_PULLUP);
  stepper.setSpeed(FORWARD_SPEED);
  // stepper.step(-10000);
}

void loop() {
  int incomingByte = 0x00;
  while (Serial.available() > 0) {
    incomingByte = Serial.read();
  }
  if (incomingByte & START_MOVING) {
    detectMaterial();
  }
  if (incomingByte & SEND_DISTANCE) {
    Serial.println(movedSteps / 57.5);
  }
  if (incomingByte & RECEIVED) {
    stepper.setSpeed(BACKWARD_SPEED);
    stepper.step(-movedSteps);
    movedSteps = 0;
  }
}

void detectMaterial() {
  stepper.setSpeed(FORWARD_SPEED);
  bool materialDetected = false;
  while (!materialDetected) {
    stepper.step(1);
    movedSteps += 1;
    materialDetected = !digitalRead(TOUCH_SENSOR_PIN);
    Serial.write(materialDetected);
  }
}