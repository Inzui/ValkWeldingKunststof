#include <Wire.h>
#include "Adafruit_VL6180X.h"

#define START 0x01
#define SEND_DISTANCE 0x02
#define RECEIVED 0x04

Adafruit_VL6180X vl = Adafruit_VL6180X();

void setup() {
  Serial.begin(9600);
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW);

  if (! vl.begin()) {
    digitalWrite(LED_BUILTIN, HIGH);
    while (1);
  }

  Serial.flush();
}

void loop() {
  int incomingByte = 0x00;
  while (Serial.available() > 0) {
    incomingByte = Serial.read();
  }

  if (incomingByte & START){
    Serial.write(true);
  }

  if (incomingByte & SEND_DISTANCE) {
    Serial.println(getDistance());
  }
}

int getDistance(){
  int total = 0;
  int n = 5;
  for(int i = 0; i < n; i++){
    total += retrieveValue();
  }

  return round(total/n);
}

int retrieveValue(){
  uint8_t range = vl.readRange();
  uint8_t status = vl.readRangeStatus();

  if (status == VL6180X_ERROR_NONE) {
    return range - 8;
  }
}
