using System.Configuration;

namespace RegionDeployer
{
    public class Independent
    {
        public const string ValueSeparator = ": ";
        public const string IpAccessKeyName = "ipstackkey";
        public const string AccessKeyParamKey = "?access_key=";
        public static readonly string PlaceholderIpStackAddress = 
            @"http://api.ipstack.com/google.com" + AccessKeyParamKey + "26928c3a15335fb85a8418e7dff2c3bf";
        public const string JsonMimeType = "application/json";
    }
}