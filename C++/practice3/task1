#include <WiFi.h>
#include <Arduino.h>

const char* ssid = "ASOIU";
const char* password = "kaf.asoiu.48";
const char* host = "www.google.com";

void setup() {
Serial.begin(115200);

WiFi.begin(ssid, password);

Serial.println();
Serial.println("Connecting to WiFi...");

while (WiFi.status() != WL_CONNECTED) {
delay(1000);
Serial.println("Connecting to WiFi...");
}

Serial.println("Connected to WiFi");
Serial.print("IP Address: ");
Serial.println(WiFi.localIP());
}

void loop() {
if (WiFi.status() == WL_CONNECTED) {
if (WiFi.ping(host)) {
Serial.print(host);
Serial.println(" is reachable");
} else {
Serial.print(host);
Serial.println(" is not reachable");
}
delay(5000);
}
}