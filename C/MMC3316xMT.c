// Distributed with a free-will license.
// Use it any way you want, profit or free, provided it fits in the licenses of its associated works.
// MMC3316xMT
// This code is designed to work with the MMC3316xMT_I2CS I2C Mini Module available from ControlEverything.com.
// https://www.controleverything.com/content/Magnetic-Sensor?sku=MMC3316xMT_I2CS#tabs-0-product_tabset-2

#include <stdio.h>
#include <stdlib.h>
#include <linux/i2c-dev.h>
#include <sys/ioctl.h>
#include <fcntl.h>

void main() 
{
	// Create I2C bus
	int file;
	char *bus = "/dev/i2c-1";
	if((file = open(bus, O_RDWR)) < 0) 
	{
		printf("Failed to open the bus. \n");
		exit(1);
	}
	// Get I2C device, MMC3316xMT I2C address is 0x30(48)
	ioctl(file, I2C_SLAVE, 0x30);
	
	// Select control register(0x07)
	// Take measurement, continuous mode, coil SET(0x23)
	char config[2] = {0};
	config[0] = 0x07;
	config[1] = 0x23;
	write(file, config, 2);

	// Select control register(0x07)
	// Coil not set(0x00)
	config[0] = 0x07;
	config[1] = 0x00;
	write(file, config, 2);
	
	// Select Control register(0x07)
	// Take measurement, continuous mode, coil RESET(0x43)
	config[0] = 0x07;
	config[1] = 0x43;
	write(file, config, 2);
	sleep(1);

	// Read 6 bytes of data
	// xMag lsb, xMag msb, yMag lsb, yMag msb, zMag lsb, zMag msb
	char reg[1] = {0x00};
	write(file, reg, 1);
	char data[6] = {0};
	if(read(file, data, 6) != 6)
	{
		printf("Error : Input/output Error \n");
	}
	else
	{
		// Convert the data to 14-bits
		int xMag = (((data[1] & 0x3F) * 256) + data[0]);
		if (xMag > 8191)
		{
			xMag -= 16384;
		}
		int yMag = (((data[3] & 0x3F) * 256) + data[2]);
		if (yMag > 8191)
		{
			yMag -= 16384;
		}
		int zMag = (((data[5] & 0x3F) * 256) + data[4]);
		if (zMag > 8191)
		{
			zMag -= 16384;
		}

		// Output data to screen
		printf("Magnetic field in X-Axis : %d \n", xMag);
		printf("Magnetic field in Y-Axis : %d \n", yMag);
		printf("Magnetic field in Z-Axis : %d \n", zMag);
	}
}
