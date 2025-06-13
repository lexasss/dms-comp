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
)
{
    public Data.ValueState SampleFlowState => GetValueState(SampleFlowR1Under, SampleFlowR1Over);
    public Data.ValueState SampleHumidityState => GetValueState(SampleHumidityR1Under, SampleHumidityR1Over);
    public Data.ValueState SampleTemperatureState => GetValueState(SampleTemperatureR1Under, SampleTemperatureR1Over);
    public Data.ValueState SamplePressureState => GetValueState(SamplePressureR1Under, SamplePressureR1Over);
    public Data.ValueState SensorFlowState => GetValueState(SensorFlowR1Under, SensorFlowR1Over);
    public Data.ValueState SensorHumidityState => GetValueState(SensorHumidityR1Under, SensorHumidityR1Over);
    public Data.ValueState SensorTemperatureState => GetValueState(SensorTemperatureR1Under, SensorTemperatureR1Over);
    public Data.ValueState SensorPressureState => GetValueState(SensorPressureR1Under, SensorPressureR1Over );
    public Data.ValueState FetTemperatureState => GetValueState(FetTemperatureR1Under, FetTemperatureR1Over);
    public Data.ValueState AmbientHumidityState => GetValueState(AmbientHumidityR1Under, AmbientHumidityR1Over);
    public Data.ValueState AmbientTemperatureState => GetValueState(AmbientTemperatureR1Under, AmbientTemperatureR1Over);
    public Data.ValueState AmbientPressureState => GetValueState(AmbientPressureR1Under, AmbientPressureR1Over);

    private static Data.ValueState GetValueState(bool isBelow, bool isAbove) =>
        isBelow ? Data.ValueState.BelowRange :
        isAbove ? Data.ValueState.AboveRange :
        Data.ValueState.InsideRange;
}

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
