#include <Wire.h>
#include <Adafruit_ADS1015.h>
#include <JY901.h>

Adafruit_ADS1115 ads(0x48);
float Voltage = 0.0;
int const_dianzu = 20000;
void setup(void) 
{
  Serial.begin(9600);  
  ads.begin();
}

void loop(void) 
{
  int adc0,adc1,adc2,adc3;  // we read from the ADC, we have a sixteen bit integer as a result
  adc0 = ads.readADC_SingleEnded(0);
  adc1 = ads.readADC_SingleEnded(1);
  adc2 = ads.readADC_SingleEnded(2);
  adc3 = ads.readADC_SingleEnded(3);
  float current = adc0/const_dianzu;
  int dz0 = (adc2-adc3);
  int dz1 = (adc1-adc2);
  int dz2 = (adc0-adc1);
  int dz3 = (22170-adc0);

  JY901.GetAcc();
  Serial.print("Acc:");

   Serial.print((JY901.stcAcc.a[0] >> 8) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.print((JY901.stcAcc.a[0]) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.print((JY901.stcAcc.a[1] >> 8) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.print((JY901.stcAcc.a[1]) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.print((JY901.stcAcc.a[2] >> 8) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.println((JY901.stcAcc.a[2]) & 0xFF);// /32768*180

   JY901.GetGyro();  
   Serial.print("Gyro ");
   Serial.print((JY901.stcGyro.w[0] >> 8) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.print((JY901.stcGyro.w[0]) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.print((JY901.stcGyro.w[1] >> 8) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.print((JY901.stcGyro.w[1]) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.print((JY901.stcGyro.w[2] >> 8) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.println((JY901.stcGyro.w[2]) & 0xFF);// /32768*180
   
   JY901.GetAngle();
   Serial.print("Angle: ");
   Serial.print((JY901.stcAngle.Angle[0] >> 8) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.print((JY901.stcAngle.Angle[0]) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.print((JY901.stcAngle.Angle[1] >> 8) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.print((JY901.stcAngle.Angle[1]) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.print((JY901.stcAngle.Angle[2] >> 8) & 0xFF);// /32768*180
   Serial.print(" ");
   Serial.println((JY901.stcAngle.Angle[2]) & 0xFF);// /32768*180
  
  Serial.print(dz0);
  Serial.print(" ");
  Serial.print(dz1);
  Serial.print(" ");
  Serial.print(dz2);
  Serial.print(" ");
  Serial.println(dz3);
  
  //Voltage = (adc0 * 0.1875)/1000;
  
  //Serial.print("AIN0: "); 
  //Serial.print(adc0);
  //Serial.print("\tVoltage: ");
  //Serial.println(Voltage, 7);  
  //Serial.println();
  
  delay(1000);
}
