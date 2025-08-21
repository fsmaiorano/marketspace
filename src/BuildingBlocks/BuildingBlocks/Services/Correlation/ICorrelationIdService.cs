namespace BuildingBlocks.Services.Correlation;

public interface ICorrelationIdService
{
    string GetCorrelationId();
    void SetCorrelationId(string correlationId);
}