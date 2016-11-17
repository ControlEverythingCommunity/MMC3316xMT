// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace MMC3316xMT
{
	struct MagneticSensor
	{
		public double X;
		public double Y;
		public double Z;
	};

	// App that reads data over I2C from a MMC3316xMT, 3-Axis Magnetic Sensor
	public sealed partial class MainPage : Page
	{
		private const byte MAG_I2C_ADDR = 0x30;				// I2C address of the MMC3316xMT
		private const byte MAG_REG_CONTROL0 = 0x07;			// Internal Control 0 register
		private const byte MAG_REG_X = 0x00;				// X Axis Low data register
		private const byte MAG_REG_Y = 0x02;				// Y Axis Low data register
		private const byte MAG_REG_Z = 0x04;				// Z Axis Low data register

		private I2cDevice I2CMag;
		private Timer periodicTimer;

		public MainPage()
		{
			this.InitializeComponent();

			// Register for the unloaded event so we can clean up upon exit
			Unloaded += MainPage_Unloaded;

			// Initialize the I2C bus, 3-Axis Magnetic Sensor, and timer
			InitI2CMag();
		}

		private async void InitI2CMag()
		{
			string aqs = I2cDevice.GetDeviceSelector();				// Get a selector string that will return all I2C controllers on the system
			var dis = await DeviceInformation.FindAllAsync(aqs);	// Find the I2C bus controller device with our selector string
			if (dis.Count == 0)
			{
				Text_Status.Text = "No I2C controllers were found on the system";
				return;
			}

			var settings = new I2cConnectionSettings(MAG_I2C_ADDR);
			settings.BusSpeed = I2cBusSpeed.FastMode;
			I2CMag = await I2cDevice.FromIdAsync(dis[0].Id, settings);	// Create an I2C Device with our selected bus controller and I2C settings
			if (I2CMag == null)
			{
				Text_Status.Text = string.Format(
					"Slave address {0} on I2C Controller {1} is currently in use by " +
					"another application. Please ensure that no other applications are using I2C.",
					settings.SlaveAddress,
					dis[0].Id);
				return;
			}

			/*
				Initialize the 3-Axis Magnetic Sensor
				For this device, we create 2-byte write buffers
				The first byte is the register address we want to write to
				The second byte is the contents that we want to write to the register
			*/
			byte[] WriteBuf_SetCtrl0 = new byte[] { MAG_REG_CONTROL0, 0x23 };		// 0x23 sets the sensor, intiates the measurement, enables continuous conversion mode and CM Frequency is 50 Hz
			byte[] WriteBuf_NoSetCtrl0 = new byte[] { MAG_REG_CONTROL0, 0x00 };		// 0x00 writes No Set to sensor
			byte[] WriteBuf_ResetCtrl0 = new byte[] { MAG_REG_CONTROL0, 0x43 };		// 0x43 resets the sensor, intiates the measurement, enables continuous conversion mode and CM Frequency is 50 Hz

			// Write the register settings
			try
			{
				I2CMag.Write(WriteBuf_SetCtrl0);
				I2CMag.Write(WriteBuf_NoSetCtrl0);
				I2CMag.Write(WriteBuf_ResetCtrl0);
			}
			// If the write fails display the error and stop running
			catch (Exception ex)
			{
				Text_Status.Text = "Failed to communicate with device: " + ex.Message;
				return;
			}

			// Create a timer to read data every 500ms
			periodicTimer = new Timer(this.TimerCallback, null, 0, 500);
		}

		private void MainPage_Unloaded(object sender, object args)
		{
			// Cleanup
			I2CMag.Dispose();
		}

		private void TimerCallback(object state)
		{
			string xText, yText, zText;
			string addressText, statusText;

			// Read and format 3-Axis Magnetic Sensor data
			try
			{
				MagneticSensor MAG = ReadI2CMag();
				addressText = "I2C Address of the 3-Axis Magnetic Sensor MMC3316xMT: 0x30";
				xText = String.Format("X Axis: {0:F0}", MAG.X);
				yText = String.Format("Y Axis: {0:F0}", MAG.Y);
				zText = String.Format("Z Axis: {0:F0}", MAG.Z);
				statusText = "Status: Running";
			}
			catch (Exception ex)
			{
				xText = "X Axis: Error";
				yText = "Y Axis: Error";
				zText = "Z Axis: Error";
				statusText = "Failed to read from 3-Axis Magnetic Sensor: " + ex.Message;
			}

			// UI updates must be invoked on the UI thread
			var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				Text_X_Axis.Text = xText;
				Text_Y_Axis.Text = yText;
				Text_Z_Axis.Text = zText;
				Text_Status.Text = statusText;
			});
		}

		private MagneticSensor ReadI2CMag()
		{
			byte[] RegAddrBuf = new byte[] { MAG_REG_X };	// Read data from the register address
			byte[] ReadBuf = new byte[6];					// We read 6 bytes sequentially to get X-Axis and all 3 two-byte axes registers in one read

			/*
				Read from the 3-Axis Magnetic Sensor 
				We call WriteRead() so we first write the address of the X-Axis I2C register, then read all 3 axes
			*/
			I2CMag.WriteRead(RegAddrBuf, ReadBuf);
			
			/*
				In order to get the raw 14-bit data values, we need to concatenate two 8-bit bytes from the I2C read for each axis
			*/
			int MAGRawX = (int)(ReadBuf[0] & 0xFF);
			MAGRawX |= (int)((ReadBuf[2] & 0x3F) * 256);
			if (MAGRawX > 8191)
			{
				MAGRawX -= 16384;
			}
			int MAGRawY = (int)(ReadBuf[2] & 0xFF);
			MAGRawY |= (int)((ReadBuf[3] & 0x3F) * 256);
			if (MAGRawY > 8191)
			{
				MAGRawY -= 16384;
			}
			int MAGRawZ = (int)(ReadBuf[4] & 0xFF);
			MAGRawZ |= (int)((ReadBuf[5] & 0x3F) * 256);
			if (MAGRawZ > 8191)
			{
				MAGRawZ -= 16384;
			}

			MagneticSensor Mag;
			Mag.X = MAGRawX;
			Mag.Y = MAGRawY;
			Mag.Z = MAGRawZ;

			return Mag;
		}
	}
}

