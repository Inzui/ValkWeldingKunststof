#include <Servo.h> 

#define TOUCH_SENSOR_PIN 2
#define PROBE_PIN 9
#define PROBE_INWARD 90
#define PROBE_OUTWARD 10

#define START_DETECTING 0x01
#define ENABLE_PROBE 0x02
#define SYNCED 0x04

Servo probe;
bool started = false;
bool detected = false;

void setup() { 
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
  if (incomingByte & START_DETECTING) {
    started = true;
    Serial.write(started);
  }
  if (incomingByte & ENABLE_PROBE) {
    probe.write(PROBE_OUTWARD);
    detected = false;
  }
  if (incomingByte & SYNCED) {
  }

  if (started) {
    if (!detected) {
      detected = digitalRead(TOUCH_SENSOR_PIN);
      if (detected) {
        probe.write(PROBE_INWARD);
      }
    }
    Serial.write(detected);
  }
}