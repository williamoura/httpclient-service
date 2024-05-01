namespace HttpClientService.Interfaces
{
    public interface ISerialization
    {
        string Serialize<TRequest>(TRequest content);

        object Deserialize(string content, Type typeExpected);
    }
}
