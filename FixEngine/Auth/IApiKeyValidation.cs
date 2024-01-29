namespace FixEngine.Auth
{
    public interface IApiKeyValidation
    {
        bool Validate(string apiKey);
    }
}
