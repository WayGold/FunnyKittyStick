#include "Arduino.h"
#include <SPI.h>
#include <RF24.h>
#include <nRF24L01.h>

// This is just the way the RF24 library works:
// Hardware configuration: Set up nRF24L01 radio on SPI bus (pins 10, 11, 12, 13) plus pins 7 & 8
RF24 radio(7, 8);

byte addresses[][6] = {"1Node","2Node"};

int magnet = 2;
int LEDRed = 4;
int LEDGreen = 3;

int response;

// -----------------------------------------------------------------------------
// SETUP   SETUP   SETUP   SETUP   SETUP   SETUP   SETUP   SETUP   SETUP
// -----------------------------------------------------------------------------
void setup() {
  Serial.begin(9600);
  Serial.println("Kitty Stick Receiver");

  pinMode(magnet, OUTPUT);
  digitalWrite(magnet, LOW); 
  
  pinMode(LEDRed, OUTPUT);
  digitalWrite(LEDRed, LOW); 

  pinMode(LEDGreen, OUTPUT);
  digitalWrite(LEDGreen, HIGH); 

  // Initiate the radio object
  radio.begin();

  // Set the transmit power to lowest available to prevent power supply related issues
  radio.setPALevel(RF24_PA_MIN);

  // Set the speed of the transmission to the quickest available
  radio.setDataRate(RF24_250KBPS);

  // Use a channel unlikely to be used by Wifi, Microwave ovens etc
  radio.setChannel(124);

  // Open a writing and reading pipe on each radio, with opposite addresses
  radio.openWritingPipe(addresses[0]);
  radio.openReadingPipe(1, addresses[1]);

  // Start the radio listening for data
  radio.startListening();
}

// -----------------------------------------------------------------------------
// We are LISTENING on this device only (although we do transmit a response)
// -----------------------------------------------------------------------------
void loop() {

  // This is what we receive from the other device (the transmitter)
  unsigned char data;

  // Is there any data for us to get?
  if ( radio.available()) {

    // Go and read the data and put it into that variable
    while (radio.available()) {
      radio.read( &data, sizeof(char));
      Serial.print(data);
    }

    if (data == 1) {
      digitalWrite(magnet, HIGH);
      digitalWrite(LEDRed, HIGH);
      response = 1;
    }

    if (data == 0) {
      digitalWrite(magnet, LOW);
      digitalWrite(LEDRed, LOW);
      response = 0;
    }

    // No more data to get so send it back but add 1 first just for kicks
    // First, stop listening so we can talk
    radio.stopListening();
    radio.write(&response, sizeof(unsigned char) );

    // Now, resume listening so we catch the next packets.
    radio.startListening();

    // Tell the user what we sent back (the random numer + 1)
    Serial.print("Sent response ");
    Serial.println(response);
  }
}
