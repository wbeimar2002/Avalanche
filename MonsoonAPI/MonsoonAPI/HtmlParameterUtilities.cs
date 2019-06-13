using System.Web;

namespace MonsoonAPI
{
    public static class HtmlParameterUtilities
    {
        public static string UrlEncode(this string value) => HttpUtility.UrlEncode(value);

        public static string UrlDecode(this string value) => HttpUtility.UrlDecode(value);
    }
}

