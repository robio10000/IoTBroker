using IoTBroker.Models;

namespace IoTBroker.Services;

public record ServiceResult(bool Success, string Message = "");
public interface ISensorService
{
    IEnumerable<SensorPayload> GetAll();
    SensorPayload? GetById(string id);

    public IEnumerable<SensorPayload> GetHistoryById(string id);
    
    bool Exists(string id);
    bool Delete(string id);
    
    ServiceResult ProcessPayload(SensorPayload payload);
}