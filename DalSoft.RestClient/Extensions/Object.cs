using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DalSoft.RestClient.Extensions
{
    internal static class Object
    {
        /// <summary>Returns a List KeyValuePair to pass into FormUrlEncodedContent supports complex objects People[0]First=Darran&amp;People[0]Last=Darran</summary>
        internal static List<KeyValuePair<string, string>> ObjectToKeyValuePair(this object o, List<KeyValuePair<string, string>> nameValueCollection = null, string prefix = null, int recrusions = 0)
        {
            const int maxRecrusions = 100;
            recrusions = prefix == null ? 0 : recrusions + 1;
            if (recrusions > maxRecrusions) throw new InvalidOperationException("Object supplied to be UrlEncoded is nested too deeply");

            nameValueCollection = nameValueCollection ?? new List<KeyValuePair<string, string>>();

            foreach (var property in o.GetType().GetProperties())
            {
                var propertyName = prefix == null ? property.Name : $"{prefix}.{property.Name}";
                var propertyValue = property.GetValue(o);

                if (propertyValue == null) continue;

                if (IsValueTypeOrPrimitiveOrStringOrGuidOrDateTime(property.PropertyType.GetTypeInfo()))
                {
                    nameValueCollection.Add(new KeyValuePair<string, string>(propertyName, propertyValue.ToString()));
                }
                else if (propertyValue is IEnumerable)
                {
                    var enumerable = ((IEnumerable)propertyValue).Cast<object>().ToArray();

                    for (var i = 0; i < enumerable.Length; i++)
                    {
                        if (IsValueTypeOrPrimitiveOrStringOrGuidOrDateTime(enumerable[i].GetType().GetTypeInfo()))
                            nameValueCollection.Add(new KeyValuePair<string, string>(propertyName, enumerable[i].ToString()));

                        foreach (var propertyItem in enumerable[i].GetType().GetProperties())
                        {
                            var propertyItemName = $"{propertyName}[{i}].{propertyItem.Name}";
                            var propertyItemValue = propertyItem.GetValue(enumerable[i]);

                            if (propertyItemValue == null) continue;

                            if (IsValueTypeOrPrimitiveOrStringOrGuidOrDateTime(propertyItem.PropertyType.GetTypeInfo()))
                            {
                                nameValueCollection.Add(new KeyValuePair<string, string>(propertyItemName, propertyItemValue.ToString()));
                            }
                            else
                            {
                                ObjectToKeyValuePair(propertyItemValue, nameValueCollection, propertyItemName, recrusions);
                            }
                        }
                    }
                }
                else
                {
                    ObjectToKeyValuePair(property.GetValue(o), nameValueCollection, propertyName, recrusions);
                }
            }

            return nameValueCollection;
        }

        internal static bool IsValueTypeOrPrimitiveOrStringOrGuid(TypeInfo type)
        {
            return type.IsValueType || type.IsPrimitive || type.AsType() == typeof(string) || type.AsType() == typeof(Guid);
        }

        internal static bool IsValueTypeOrPrimitiveOrStringOrGuidOrDateTime(TypeInfo type)
        {
            return IsValueTypeOrPrimitiveOrStringOrGuid(type) || type.AsType() == typeof(DateTime);
        }
    }
}
