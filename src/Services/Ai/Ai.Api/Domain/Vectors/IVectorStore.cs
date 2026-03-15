namespace Ai.Api.Domain.Vectors;

public interface IVectorStore  
{  
    Task<IEnumerable<string>> Search(float[] vector);  
}