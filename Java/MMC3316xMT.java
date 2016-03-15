// Distributed with a free-will license.
// Use it any way you want, profit or free, provided it fits in the licenses of its associated works.
// MMC3316xMT
// This code is designed to work with the MMC3316xMT_I2CS I2C Mini Module available from ControlEverything.com.
// https://www.controleverything.com/content/Magnetic-Sensor?sku=MMC3316xMT_I2CS#tabs-0-product_tabset-2

import com.pi4j.io.i2c.I2CBus;
import com.pi4j.io.i2c.I2CDevice;
import com.pi4j.io.i2c.I2CFactory;
import java.io.IOException;

public class MMC3316xMT
{
	public static void main(String args[]) throws Exception
    {
		// Create I2C bus
		I2CBus bus = I2CFactory.getInstance(I2CBus.BUS_1);
		// Get I2C device, MMC3316xMT I2C address is 0x30(48)
		I2CDevice device = bus.getDevice(0x30);
		Thread.sleep(100);

		// Select Control register
		// Take measurement, continuous mode, coil SET
		device.write(0x07, (byte)0x23);
		// Select Control register
		// Coil not set
		device.write(0x07, (byte)0x00);
		// Select Control register
		// Take measurement, continuous mode, coil RESET
		device.write(0x07, (byte)0x43);
		Thread.sleep(500);

		// Read 6 bytes of data
		// xMag lsb, xMag msb, yMag lsb, yMag msb, zMag lsb, zMag msb
		byte[] data = new byte[6];
		device.read(0x00, data, 0, 6);

		// Convert the data to 14-bits
		int xMag = ((data[1] & 0x3F) * 256 + (data[0] & 0xFF));
		if(xMag > 8191)
		{
			xMag -= 16384;
		}	

		int yMag = ((data[3] & 0x3F) * 256 + (data[2] & 0xFF)) ;
		if(yMag > 8191)
		{
			yMag -= 16384;
		}

		int zMag = ((data[5] & 0x3F) * 256 + (data[4] & 0xFF)) ;
		if(zMag > 8191)
		{
			zMag -= 16384;
		}

		// Output to the screen
		System.out.printf("Magnetic field in X-Axis : %d %n", xMag);
		System.out.printf("Magnetic field in Y-Axis : %d %n", yMag);
		System.out.printf("Magnetic field in Z-Axis : %d %n", zMag);
  	}
}
