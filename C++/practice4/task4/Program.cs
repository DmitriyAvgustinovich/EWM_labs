using System.Globalization;
using CsvHelper;
using Iot.Device.Adc;
using Raven.Iot.Device;
using Raven.Iot.Device.Ina219;
using UnitsNet;

var ina219Settings = DeviceHelper.GetIna219Devices() is [var settings];

if (ina219Settings)
{
    var calibrator = Ina219Calibrator.Default with
    {
        VMax = ElectricPotential.FromVolts(5.0),
        IMax = ElectricCurrent.FromAmperes(0.6)
    };

    var calibratedIna219 = calibrator.CreateCalibratedDevice(settings);

    await using var fileWriter = new StreamWriter("file.csv");
    await using var csvWriter = new CsvWriter(fileWriter, CultureInfo.InvariantCulture);
    await csvWriter.WriteRecordsAsync(GenerateMeasurements(calibratedIna219));
}

return;

async IAsyncEnumerable<Measurements> GenerateMeasurements(Ina219 device)
{
    for (var decaseconds = 0; decaseconds < 6; decaseconds++)
    {
        var currentAmperes = device.ReadCurrent().Amperes;
        var powerWatts = device.ReadPower().Watts;
        var voltageVolts = (double)powerWatts / currentAmperes;

        yield return new Measurements
        {
            Timestamp = decaseconds,
            Watts = (double)powerWatts,
            Amperes = currentAmperes,
            Volts = voltageVolts
        };

        await Task.Delay(10000);
    }
}

internal struct Measurements
{
    public int Timestamp;
    public double Watts;
    public double Amperes;
    public double Volts;
}
