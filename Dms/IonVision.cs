namespace DmsComparison.IonVision;

/* Definitions of DMS JSON file */

public record class ErrorRegister(
    bool AmbientPressureR1Under,
    bool AmbientPressureR1Over,
    bool AmbientHumidityR1Under,
    bool AmbientHumidityR1Over,
    bool AmbientTemperatureR1Under,
    bool AmbientTemperatureR1Over,
    bool FetTemperatureR1Under,
    bool FetTemperatureR1Over,
    bool SampleFlowR1Under,
    bool SampleFlowR1Over,
    bool SampleTemperatureR1Under,
    bool SampleTemperatureR1Over,
    bool SamplePressureR1Under,
    bool SamplePressureR1Over,
    bool SampleHumidityR1Under,
    bool SampleHumidityR1Over,
    bool SensorFlowR1Under,
    bool SensorFlowR1Over,
    bool SensorTemperatureR1Under,
    bool SensorTemperatureR1Over,
    bool SensorPressureR1Under,
    bool SensorPressureR1Over,
    bool SensorHumidityR1Under,
    bool SensorHumidityR1Over,
    bool SampleHeaterTemperatureR1Under,
    bool SampleHeaterTemperatureR1Over,
    bool SensorHeaterTemperatureR1Under,
    bool SensorHeaterTemperatureR1Over
); 
public record class RangeAvg(
    double Avg,
    double Min,
    double Max
);
public record class Detector(
    RangeAvg Temperature,
    RangeAvg Pressure,
    RangeAvg Humidity
);
public record class FlowDetector(
    RangeAvg Flow,
    RangeAvg Temperature,
    RangeAvg Pressure,
    RangeAvg Humidity,
    RangeAvg PumpPWM
) : Detector(Temperature, Pressure, Humidity);
public record class SystemData(
    ErrorRegister ErrorRegister,
    RangeAvg FetTemperature,
    FlowDetector Sample,
    FlowDetector Sensor,
    Detector Ambient
);
public record class MeasurementData(
    bool DataValid,
    int DataPoints,
    float[] IntensityTop,
    float[] IntensityBottom,
    float[] Usv,
    float[] Ucv,
    float[] Vb,
    float[] PP,
    float[] PW,
    short[] NForSampleAverages
);
public record class Scan(
    string Id,
    string? Measurer,
    string StartTime,
    string FinishTime,
    string Parameters,
    string Project,
    object Comments,
    int FormatVersion,
    SystemData SystemData,
    MeasurementData MeasurementData
);
