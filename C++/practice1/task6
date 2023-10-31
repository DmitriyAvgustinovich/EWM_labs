void setup() {
  // yстанавливаем частоту для ШИМ сигнала на светодиоде в 120 Гц
  analogWriteFreq(120);
}

void loop() {
  int potValue = analogRead(A0);
  int ledPin = map(potValue, 0, 4095, 0, 255);
  analogWrite(D0, ledPin);
}
