namespace Blueclass.Wunderlist
{
    public interface IWunderlistAuthenticator
    {
        string GetAccessToken();
        string GetClientId();
    }
}