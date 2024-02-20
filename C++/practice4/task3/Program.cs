
if (Iot.Device.OneWire.OneWireThermometerDevice.EnumerateDevices().Take(1).ToList() is [var settings])
{
    while (true)
    {
        var temperature = settings.ReadTemperature(); 
        Console.WriteLine($"Temperature: {temperature.DegreesCelsius:F}");
        Thread.Sleep(1000);
    }
}

while (true)
{
    foreach (var term in Iot.Device.OneWire.OneWireThermometerDevice.EnumerateDevices())
    { 
        var temperature = term.ReadTemperature(); 
        Console.WriteLine($"Temperature: {temperature.DegreesCelsius:F}");
        await Task.Delay(1000);
    }
}