using System;

namespace CIRCBot
{
    /// <summary>
    /// Marks a property as an extra-data column for the statistics table
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    class ExtraAttribute : Attribute
    {
    }
}
