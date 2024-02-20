using System.Device.Gpio;
using Iot.Device.RotaryEncoder;
using Raven.Iot.Device;
using Raven.Iot.Device.GpioExpander;
using UnitsNet;
using UnitsNet.Units;

const int keyPin = 0;
var pinA = DeviceHelper.WiringPiToBcm(0);
var pinB = DeviceHelper.WiringPiToBcm(1);

if (DeviceHelper.GetGpioExpanderDevices() is [var settings])
{
    var expander = new GpioExpander(settings);
    var encoder = new ScaledQuadratureEncoder(pinA, pinB, PinEventTypes.Falling,
        255,
        1,
        0,
        255);
    expander.SetPwmFrequency(Frequency.FromKilohertz(25));
    encoder.ValueChanged += (_, args) =>
    {
        expander.AnalogWrite(keyPin, (int)args.Value);
        Console.WriteLine($"Speed: {args.Value / 2.55}%");
    };
}
else
{
    throw new IoTDeviceException("Expander not found!!!");
}