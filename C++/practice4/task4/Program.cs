using System.Globalization;
using CsvHelper;
using Iot.Device.Adc;
using Raven.Iot.Device;
using Raven.Iot.Device.Ina219;
using UnitsNet;

if (DeviceHelper.GetIna219Devices() is [var settings])
{
    var calibrator = Ina219Calibrator.Default with
    {
        VMax = ElectricPotential.FromVolts(5.0),
        IMax = ElectricCurrent.FromAmperes(0.6)
    };

    var ina219 = calibrator.CreateCalibratedDevice(settings);

    await using var writer = new StreamWriter("measure.csv");
    await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
    await csv.WriteRecordsAsync(GetMeasurements(ina219));
}

return;


async IAsyncEnumerable<Measurements> GetMeasurements(Ina219 device)
{
    foreach (var decaseconds in Enumerable.Range(0, 6))
    {
        var i = device.ReadCurrent().Amperes;
        var p = device.ReadPower().Watts;
        var v = (double)p / i;

        yield return new Measurements
        {
            CurrentAmperes = i,
            PowerWatts = (double)p,
            VoltageVolts = v,
            TimestampSeconds = decaseconds,
        };

        await Task.Delay(10000);
    }
}

internal struct Measurements
{
    public int TimestampSeconds;
    public double PowerWatts;
    public double CurrentAmperes;
    public double VoltageVolts;
}
