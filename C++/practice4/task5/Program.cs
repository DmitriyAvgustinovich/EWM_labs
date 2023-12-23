using System.Device.Gpio;
using Iot.Device.RotaryEncoder;
using Raven.Iot.Device;
using Raven.Iot.Device.GpioExpander;
using Raven.Iot.Device.MicrocontrollerBoard;

const int PicoAddress = 2;
var encoderPinA = DeviceHelper.WiringPiToBcm(0);
var encoderPinB = DeviceHelper.WiringPiToBcm(1);

if (DeviceHelper.GetGpioExpanderDevices() is [var gpioExpanderSettings])
{
    var gpioExpander = new GpioExpander(gpioExpanderSettings);
    var encoder = new ScaledQuadratureEncoder(encoderPinA, encoderPinB, PinEventTypes.Falling,
        1,
        0.01,
        0,
        1);

    if (DeviceHelper.I2cDeviceSearch(
            new ReadOnlySpan<int>(new[] {1}),
            new ReadOnlySpan<int>(new[] {PicoAddress}) ) 
        is [var picoSettings])
    {
        var microcontrollerBoard = new MicrocontrollerBoard<Request, Response>(picoSettings);
        
        encoder.ValueChanged += (sender, args) =>
        {
            microcontrollerBoard.WriteRequest(new PwmRequest {Duty = args.Value});
            _ = microcontrollerBoard.ReadResponse();
        };
    }
    else
    {
        throw new IoTDeviceException("Raspberry Pi Pico не найден");
    }
}
else
{
    throw new IoTDeviceException("Expander не найден");
}

internal struct Request
{
    public double Duty;
}

internal struct Response
{
    private bool _;
}
