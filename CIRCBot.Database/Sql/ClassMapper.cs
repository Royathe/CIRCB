using System;
using System.Data;
using System.Linq;

namespace CIRCBot.Sql
{
    public static class ClassMapper
    {
        /// <summary>
        /// Map a datarow to the target object type. Column names must match target type property names.
        /// </summary>
        /// <typeparam name="T">Type of target the row is mapped to</typeparam>
        /// <param name="row">DataRow which to map to the target object</param>
        /// <returns>New instance of T typed object with the data from the DataRow</returns>
        public static T Map<T>(DataRow row)
        {
            var properties = typeof(T).GetProperties();

            T instance = Activator.CreateInstance<T>();

            foreach(DataColumn column in row.Table.Columns)
            {
                var property = properties.FirstOrDefault(x => x.Name == column.ColumnName);

                if(property != null)
                {
                    var value = row[property.Name] != DBNull.Value ? row[property.Name] : null;
                    property.SetValue(instance, value);
                }
            }

            return instance;
        }
    }
}
