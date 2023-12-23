#include <Arduino.h>

const int pin = D25;

void setup() {
    pinMode(pin, OUTPUT);
    analogWriteRange(4095);
    analogWriteFreq(25000);
}

void loop() {
}
