#include <Stepper.h>

#define MAX_SENSOR_PIN 3
#define MIN_SENSOR_PIN 4

#define SPEED 100

#define FORWARD_STEPS 10
#define BACKWARD_STEPS -10

Stepper stepper(64, 4, 5, 6 ,7);

bool max = false;
bool min = false;

void setup() {
  pinMode(TOUCH_SENSOR_PIN, INPUT_PULLUP);
  stepper.setSpeed(SPEED);
}

void loop() {
  max = !digitalRead(MAX_SENSOR_PIN); //Is true when pressed -- Needs to be true
  min = !digitalRead(MIN_SENSOR_PIN); //Is true when pressed -- Needs to be false

  if(min){
    moveStepper(false);
  } else if (!max){
    moveStepper(true);
  } else {
    delay(200);
  }
}

void moveStepper(bool forward){
  if(forward){
    stepper.step(FORWARD_STEP);
  } else {
    stepper.step(BACKWARD_STEP);
  }
  delay(100);
}
