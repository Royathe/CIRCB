using System;

namespace CIRCBot
{
    /// <summary>
    /// Defines the propertiy's JSON mapping 
    ///     ex:
    ///     main, weather, id => the value should be the value of main.weather.id in the json
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    class JSONMapAttribute : Attribute
    {
        private string[] _propertyMap { get; set; }

        public string[] PropertyMap
        {
            get
            {
                return _propertyMap;
            }
        }

        public JSONMapAttribute(params string[] propertyMap)
        {
            _propertyMap = propertyMap;
        }
    }
}
