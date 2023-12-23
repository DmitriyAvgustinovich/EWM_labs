using System.Device.Gpio;
using Iot.Device.RotaryEncoder;
using Raven.Iot.Device;
using Raven.Iot.Device.GpioExpander;
using UnitsNet;
using UnitsNet.Units;

const int MotorPin = 0;
var encoderPinOne = DeviceHelper.WiringPiToBcm(0);
var encoderPinTwo = DeviceHelper.WiringPiToBcm(1);

if (DeviceHelper.GetGpioExpanderDevices() is [var expanderSettings])
{
    var gpioExpander = new GpioExpander(expanderSettings);
    var rotaryEncoder = new ScaledQuadratureEncoder(encoderPinOne, encoderPinTwo, PinEventTypes.Falling,
        180,
        1,
        0,
        180);

    rotaryEncoder.PulseCountChanged += (_, eventArgs) =>
    {
        var rotationAngle = Angle.FromDegrees(eventArgs.Value);
        gpioExpander.WriteAngle(MotorPin, rotationAngle);
        Console.WriteLine($"Текущий угол наклона: {rotationAngle.Degrees} градусов");
    };
}
else
{
    throw new IoTDeviceException("GPIO expander не найден");
}
