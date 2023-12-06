#include <Servo.h> 

#define TOUCH_SENSOR_PIN 2
#define PROBE_PIN 9
#define PROBE_INWARD 90
#define PROBE_OUTWARD 10

// Incoming commands
#define HEARTBEAT 0x01
#define START_DETECTING 0x02
#define REQUEST_OBJECT_DETECTED 0x04

// Outgoing commands
#define SUCCES 0x1
#define OBJECT_DETECTED 0x2
#define OBJECT_NOT_DETECTED 0x4

Servo probe;
bool detecting = false;
bool detected = false;

void setup() { 
  pinMode(13, OUTPUT);

  Serial.begin(115200);
  pinMode(TOUCH_SENSOR_PIN, INPUT_PULLUP);
  probe.attach(PROBE_PIN);
  probe.write(PROBE_INWARD);
} 
 
void loop() { 
  int incomingByte = 0x00;
  while (Serial.available() > 0) {
    incomingByte = Serial.read();
  }

  if (incomingByte & HEARTBEAT) {
    Serial.write(SUCCES);
  }

  if (incomingByte & START_DETECTING) {
    probe.write(PROBE_OUTWARD);
    detected = false;
    detecting = true;

    Serial.write(SUCCES);
  }

  if (incomingByte & REQUEST_OBJECT_DETECTED) {
    Serial.write(detected ? OBJECT_DETECTED : OBJECT_NOT_DETECTED);
    digitalWrite(13, HIGH);
  }

  if (detecting) {
    if (digitalRead(TOUCH_SENSOR_PIN)) {
      probe.write(PROBE_INWARD);
      detected = true;
      detecting = false;
    }
  }
}