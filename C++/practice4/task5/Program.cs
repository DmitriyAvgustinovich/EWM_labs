using System.Device.Gpio;
using Iot.Device.RotaryEncoder;
using Raven.Iot.Device;
using Raven.Iot.Device.GpioExpander;
using Raven.Iot.Device.MicrocontrollerBoard;

const int picoAddress = 2;
var pinA = DeviceHelper.WiringPiToBcm(0);
var pinB = DeviceHelper.WiringPiToBcm(1);

if (DeviceHelper.GetGpioExpanderDevices() is [var settings])
{
    var expander = new GpioExpander(settings);
    var encoder = new ScaledQuadratureEncoder(pinA, pinB, PinEventTypes.Falling,
        1,
        0.01,
        0,
        1);
    if (DeviceHelper.I2cDeviceSearch(
            new ReadOnlySpan<int>(new[] {1}),
            new ReadOnlySpan<int>(new[] {picoAddress}) ) 
        is [var picoSettings])
    {
        var board = new MicrocontrollerBoard<PwmRequest, Response>(picoSettings);
        
        encoder.ValueChanged += (o, args) =>
        {
            board.WriteRequest(new PwmRequest {Duty = args.Value});
            _ = board.ReadResponse();
        };
    }
    else
    {
        throw new IoTDeviceException("RPi Pico not found!!!");
    }
}
else
{
    throw new IoTDeviceException("Expander not found!!!");
}

internal struct PwmRequest
{
    public double Duty;
}

internal struct Response
{
    private bool _-;
}