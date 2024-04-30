using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OpenPgpBatchJob
{
    internal static class ObjectPrinter
    {

        /// <summary>
        /// This function print out the list of specified properties of an Object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyNames"></param>
        internal static StringBuilder PrintProperties(object obj, List<string> propertyNames)
        {
            StringBuilder sb = new StringBuilder();

            if (obj == null)
            {
                Console.WriteLine("Object is null.");
                return null;
            }

            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (string propName in propertyNames)
            {
                PropertyInfo property = type.GetProperty(propName);

                if (property != null)
                {
                    try
                    {
                        object value = property.GetValue(obj, null);
                        sb.AppendLine($"{property.Name} = {value}");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"{property.Name} could not be read: {ex.Message}");
                    }
                }
                else
                {
                    sb.AppendLine($"Property '{propName}' does not exist on {type.Name}.");
                }
            }

            return sb;
        }
    }
}