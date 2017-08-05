using System;

namespace CIRCBot
{
    /// <summary>
    /// Marks a property as an Order By property for the statistics table
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    class OrderAttribute : Attribute
    {
    }
}
