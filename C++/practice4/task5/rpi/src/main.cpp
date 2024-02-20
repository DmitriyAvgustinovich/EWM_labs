#include <Arduino.h>

const int fan_pin = D25;

void setup() {
    pinMode(fan_pin, OUTPUT);
    analogWriteRange(4095);
    analogWriteFreq(25000);
}

void loop() {
}
