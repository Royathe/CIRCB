using System;

namespace CIRCBot
{
    /// <summary>
    /// Defines the loggable name of an executor, along with it's description
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ClassNameAttribute : Attribute
    {
        private string name;

        private string desc;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Description
        {
            get
            {
                return desc;
            }
        }

        public ClassNameAttribute(string nameOfClass, string description = "")
        {
            name = nameOfClass;
            desc = description;
        }
    }
}
