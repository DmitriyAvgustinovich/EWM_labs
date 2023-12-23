var thermometers = Iot.Device.OneWire.OneWireThermometerDevice.EnumerateDevices().Take(1).ToList();

if (thermometers is [var settings])
{
    while (true)
    {
        var temperature = settings.ReadTemperature(); 
        Console.WriteLine($"Температура: {temperature.DegreesCelsius:F}");
        Thread.Sleep(1000);
    }
}

while (true)
{
    foreach (var thermometer in Iot.Device.OneWire.OneWireThermometerDevice.EnumerateDevices())
    { 
        var temperature = thermometer.ReadTemperature(); 
        Console.WriteLine($"Температура: {temperature.DegreesCelsius:F}");
        await Task.Delay(1000);
    }
}
