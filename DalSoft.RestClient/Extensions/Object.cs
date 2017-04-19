using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DalSoft.RestClient.Extensions
{
    internal static class Object //TODO: this whole class is ugly and hurts my eye
    {
        /// <summary>Returns a List KeyValuePair to pass into FormUrlEncodedContent supports complex objects People[0]First=Darran&amp;People[0]Last=Darran</summary>
        internal static List<KeyValuePair<string, TValue>> FlattenObjectToKeyValuePairs<TValue>(
            this object o,
            Func<TypeInfo, bool> includeThisType,
            List<KeyValuePair<string, TValue>> nameValueCollection = null, 
            string prefix = null, int recrusions = 0) 
        {
            const int maxRecrusions = 30;
            recrusions = prefix == null ? 0 : recrusions + 1;
            if (recrusions > maxRecrusions) throw new InvalidOperationException("Object supplied to be UrlEncoded is nested too deeply");

            nameValueCollection = nameValueCollection ?? new List<KeyValuePair<string, TValue>>();

            foreach (var property in o.GetType().GetProperties())
            {
                var propertyName = prefix == null ? property.Name : $"{prefix}.{property.Name}";
                var propertyValue = property.GetValue(o);

                if (propertyValue == null) continue;

                if (includeThisType(property.PropertyType.GetTypeInfo()))
                {
                    nameValueCollection.Add(new KeyValuePair<string, TValue>(propertyName, (TValue)(typeof(TValue) == typeof(string) ? propertyValue.FormatAsString() : propertyValue)));
                }
                else if (propertyValue is IEnumerable)
                {
                    var enumerable = ((IEnumerable)propertyValue).Cast<object>().ToArray();

                    for (var i = 0; i < enumerable.Length; i++)
                    {
                        if (includeThisType(enumerable[i].GetType().GetTypeInfo())) 
                        { 
                            nameValueCollection.Add(new KeyValuePair<string, TValue>(propertyName, (TValue)(typeof(TValue) == typeof(string) ? enumerable[i].FormatAsString() : enumerable[i])));
                            continue;
                        }

                        foreach (var propertyItem in enumerable[i].GetType().GetProperties())
                        {
                            var propertyItemName = $"{propertyName}[{i}].{propertyItem.Name}";
                            var propertyItemValue = propertyItem.GetValue(enumerable[i]);

                            if (propertyItemValue == null) continue;

                            if (includeThisType(propertyItem.PropertyType.GetTypeInfo()))
                            {
                                nameValueCollection.Add(new KeyValuePair<string, TValue>(propertyItemName, (TValue)(typeof(TValue) == typeof(string) ? propertyItemValue.FormatAsString() : propertyItemValue)));
                            }
                            else
                            {
                                FlattenObjectToKeyValuePairs<TValue>(propertyItemValue, includeThisType, nameValueCollection, propertyItemName, recrusions);
                            }
                        }
                    }
                }
                else
                {
                    FlattenObjectToKeyValuePairs<TValue>(property.GetValue(o), includeThisType, nameValueCollection, propertyName, recrusions);
                }
            }

            return nameValueCollection;
        }

        internal static string FormatAsString(this object o)
        {
            if (o is DateTime)
                return ((DateTime) o).ToString("s", CultureInfo.InvariantCulture);

            return o.ToString();
        }

        internal static bool IsValueTypeOrPrimitiveOrStringOrGuid(TypeInfo type)
        {
            return type.IsValueType || type.IsPrimitive || type.AsType() == typeof(string) || type.AsType() == typeof(Guid);
        }

        internal static bool IsValueTypeOrPrimitiveOrStringOrGuidOrDateTime(TypeInfo type)
        {
            return IsValueTypeOrPrimitiveOrStringOrGuid(type) || type.AsType() == typeof(DateTime);
        }

        internal static bool IsValueTypeOrPrimitiveOrStringOrGuidOrDateTimeOrByteArrayOrStream(TypeInfo type)
        {
            return IsValueTypeOrPrimitiveOrStringOrGuidOrDateTime(type) || type.AsType() == typeof(byte[]) || type.AsType() == typeof(Stream);
        }
    }
}
