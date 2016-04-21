namespace Blueclass.Wunderlist
{
    public class HardcodedWunderlistAuthenticator : IWunderlistAuthenticator
    {
        readonly string _accessToken;

        public HardcodedWunderlistAuthenticator(string accessToken)
        {
            _accessToken = accessToken;
        }

        public string GetAccessToken() => _accessToken;

        public string GetClientId() => "be48c0e7b5c882d2964c";
    }
}