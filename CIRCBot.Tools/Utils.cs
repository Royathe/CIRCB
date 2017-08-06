using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace CIRCBot
{
    public static class Utils
    {
        /// <summary>
        /// Maps a dynamic JSON object to an instance of the target object. 
        /// Target properties must have the <see cref="JSONMapAttribute"/>.
        /// </summary>
        /// <typeparam name="T">Type of the object to create</typeparam>
        /// <param name="json">the dynamic json object to map from</param>
        /// <returns>json mapped to specified type of object</returns>
        public static T JSONMapper<T>(dynamic json)
        {
            T target = (T)Activator.CreateInstance(typeof(T));

            PropertyInfo[] properties = target.GetType()
                .GetProperties()
                .Where(x => x.GetCustomAttribute<JSONMapAttribute>() != null)
                .ToArray();

            foreach (PropertyInfo property in properties)
            {
                JSONMapAttribute jsonMap = property.GetCustomAttribute<JSONMapAttribute>();

                dynamic sub = json;
                foreach (string pm in jsonMap.PropertyMap)
                {
                    sub = sub[pm];
                }

                property.SetValue(target, ((object)sub).ToString());
            }

            return target;
        }

        /// <summary>
        /// Get the final url of a redirecting url
        /// </summary>
        /// <param name="url">url to call that will redirect</param>
        /// <returns>the url redirected to</returns>
        public static string GetRedirectResult(string url)
        {
            HttpWebRequest httpRequest = WebRequest.Create(url) as HttpWebRequest;
            httpRequest.Timeout = 2500;
            using (HttpWebResponse response = httpRequest.GetResponse() as HttpWebResponse)
            {
                return response.ResponseUri.ToString();
            }
        }

        /// <summary>
        /// Call the url and return the JSON response
        /// </summary>
        /// <param name="url">url that will return JSON data</param>
        /// <returns>JSON data from the url</returns>
        public static string GetJSONResponse(string url)
        {
            string json = "";

            HttpWebRequest httpRequest = WebRequest.Create(url) as HttpWebRequest;
            httpRequest.Timeout = 2500;
            using (HttpWebResponse response = httpRequest.GetResponse() as HttpWebResponse)
            {
                Encoding encoding = Encoding.GetEncoding(response.CharacterSet);

                using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding, false))
                {
                    json = reader.ReadToEnd();
                }
            }
            return json;
        }

        /// <summary>
        /// Converts a UnixTimeStamp to a DateTime object
        /// </summary>
        /// <param name="unixTimeStamp">The UnixTimeStamp to convert</param>
        /// <returns>Date corresponding to the UnixTimeStamp</returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            date = date.AddSeconds(unixTimeStamp).ToLocalTime();
            return date;
        }
    }
}
