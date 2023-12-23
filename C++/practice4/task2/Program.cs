using System.Device.Gpio;
using Iot.Device.RotaryEncoder;
using Raven.Iot.Device;
using Raven.Iot.Device.GpioExpander;
using UnitsNet;
using UnitsNet.Units;

const int KeyPin = 0;
var encoderPinOne = DeviceHelper.WiringPiToBcm(0);
var encoderPinTwo = DeviceHelper.WiringPiToBcm(1);

if (DeviceHelper.GetGpioExpanderDevices() is [var expanderSettings])
{
    var gpioExpander = new GpioExpander(expanderSettings);
    var rotaryEncoder = new ScaledQuadratureEncoder(encoderPinOne, encoderPinTwo, PinEventTypes.Falling,
        255,
        1,
        0,
        255);

    gpioExpander.SetPwmFrequency(Frequency.FromKilohertz(25));

    rotaryEncoder.ValueChanged += (_, eventArgs) => 
    {
        gpioExpander.AnalogWrite(KeyPin, (int)eventArgs.Value);
        Console.WriteLine($"Текущая скорость: {eventArgs.Value / 2.55}%");
    };
}
else
{
    throw new IoTDeviceException("GPIO expander не найден");
}
