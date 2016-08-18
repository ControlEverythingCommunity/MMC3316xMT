# Distributed with a free-will license.
# Use it any way you want, profit or free, provided it fits in the licenses of its associated works.
# MMC3316xMT
# This code is designed to work with the MMC3316xMT_I2CS I2C Mini Module available from ControlEverything.com.
# https://www.controleverything.com/content/Magnetic-Sensor?sku=MMC3316xMT_I2CS#tabs-0-product_tabset-2

from OmegaExpansion import onionI2C
import time

# Get I2C bus
i2c = onionI2C.OnionI2C()

# MMC3316xMT address, 0x30(48)
# Select control register, 0x07(7)
#		0x23(35)	Initiate measurement, Continous mode ON, Coil set
i2c.writeByte(0x30, 0x07, 0x23)
# MMC3316xMT address, 0x30(48)
# Select control register, 0x07(7)
#		0x00(00)	Coil Not set
i2c.writeByte(0x30, 0x07, 0x00)
# MMC3316xMT address, 0x30(48)
# Select control register, 0x07(7)
#		0x43(67)	Initiate measurement, Continous mode ON, Coil Reset
i2c.writeByte(0x30, 0x07, 0x43)

time.sleep(0.5)

# MMC3316xMT address, 0x30(48)
# Read data back from 0x00(0), 6 bytes
# X-Axis LSB, X-Axis MSB, Y-Axis LSB, Y-Axis MSB, Z-Axis LSB, Z-Axis MSB
data = i2c.readBytes(0x30, 0x00, 6)

# Convert the data
xMag = (data[1] & 0x3F) * 256 + data[0]
if xMag > 8191 :
	xMag -= 16384

yMag = (data[3] & 0x3F) * 256 + data[2]
if yMag > 8191 :
	yMag -= 16384

zMag = (data[5] & 0x3F) * 256 + data[4]
if zMag > 8191 :
	zMag -= 16384

# Output data to screen
print "Magnetic field in X-Axis : %d" %xMag
print "Magnetic field in Y-Axis : %d" %yMag
print "Magnetic field in Z-Axis : %d" %zMag
