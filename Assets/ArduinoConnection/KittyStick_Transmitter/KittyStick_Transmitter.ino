#include "Arduino.h"
#include <SPI.h>
#include <RF24.h>
#include <nRF24L01.h>


// This is just the way the RF24 library works:
// Hardware configuration: Set up nRF24L01 radio on SPI bus (pins 10, 11, 12, 13) plus pins 7 & 8
RF24 radio(7, 8);

byte addresses[][6] = {"1Node", "2Node"};
int data;

// -----------------------------------------------------------------------------
// SETUP   SETUP   SETUP   SETUP   SETUP   SETUP   SETUP   SETUP   SETUP
// -----------------------------------------------------------------------------
void setup() {
  Serial.begin(9600);
  Serial.println("Kitty Stick Transmitter");

  // Initiate the radio object
  radio.begin();

  // Set the transmit power to lowest available to prevent power supply related issues
  radio.setPALevel(RF24_PA_MIN);

  // Set the speed of the transmission to the quickest available
  radio.setDataRate(RF24_250KBPS);

  // Use a channel unlikely to be used by Wifi, Microwave ovens etc
  radio.setChannel(124);

  // Open a writing and reading pipe on each radio, with opposite addresses
  radio.openWritingPipe(addresses[1]);
  radio.openReadingPipe(1, addresses[0]);

  // Random number seeding
  // randomSeed(analogRead(A0));
}

// -----------------------------------------------------------------------------
// LOOP     LOOP     LOOP     LOOP     LOOP     LOOP     LOOP     LOOP     LOOP
// -----------------------------------------------------------------------------
void loop() {
    
  // Ensure we have stopped listening
  radio.stopListening(); 
  
  // Turn magnet on or off from unity signal
    switch (Serial.read())
    {
        case 'O':
        {
            Serial.println("Electromagnet turning On!");
            data = 1;

            // Did we manage to SUCCESSFULLY transmit the data

            if (!radio.write( &data, sizeof(unsigned char) )) {
            Serial.println("Kitty Stick Receiver not connecting");    
            }

            // Now listen for a response
            radio.startListening();
  
            // 200 milliseconds is enough
            unsigned long started_waiting_at_On = millis();

            // Loop here until we get indication that some data is ready for us to read (or we time out)
            while ( ! radio.available() ) {
          
              // Oh dear, no response received within our timescale
              if (millis() - started_waiting_at_On > 200 ) {
                Serial.println("No response received - timeout!");
                return;
              }
            }
          
            // Now read the data that is waiting for us in the nRF24L01's buffer
            unsigned char responseOn;
            radio.read( &responseOn, sizeof(unsigned char) );
          
            // Show user what we sent and what we got back
            Serial.print("Sent: ");
            Serial.print(data);
            Serial.print(", received: ");
            Serial.println(responseOn);
            Serial.println("This means electromagnet is on");
            break;
        }

         case 'P':
         {
         
            Serial.println("Electromagnet turning Off!");
            data = 0;

            // Did we manage to SUCCESSFULLY transmit the data

            if (!radio.write( &data, sizeof(unsigned char) )) {
            Serial.println("Kitty Stick Receiver not connecting");    
            }

            // Now listen for a response
            radio.startListening();
  
            // 200 milliseconds is enough
            unsigned long started_waiting_at_Off = millis();

            // Loop here until we get indication that some data is ready for us to read (or we time out)
            while ( ! radio.available() ) {
          
              // Oh dear, no response received within our timescale
              if (millis() - started_waiting_at_Off > 200 ) {
                Serial.println("No response received - timeout!");
                return;
              }
            }
          
            // Now read the data that is waiting for us in the nRF24L01's buffer
            unsigned char responseOff;
            radio.read( &responseOff, sizeof(unsigned char) );
          
            // Show user what we sent and what we got back
            Serial.print("Sent: ");
            Serial.print(data);
            Serial.print(", received: ");
            Serial.println(responseOff);
            Serial.println("This means electromagnet is off");
            break;

            Default:
            {
            Serial.print("Other Key Pressed");
            break;
            }
         }
            
    }     
}
