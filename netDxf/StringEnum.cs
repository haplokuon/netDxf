#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace netDxf
{
    /// <summary>
    /// Helper class for working with 'extended' enums using <see cref="StringValueAttribute"/> attributes.
    /// </summary>
    public class StringEnum<T> where T : Enum
    {
        #region private fields

        private readonly Type enumType;

        #endregion

        #region constructors

        /// <summary>
        /// Creates a new <see cref="StringEnum{T}"/> instance.
        /// </summary>
        public StringEnum()
        {
            this.enumType = typeof(T);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the underlying enum type for this instance.
        /// </summary>
        /// <value></value>
        public Type EnumType
        {
            get { return this.enumType; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Gets the string values associated with the enum.
        /// </summary>
        /// <returns>String value array</returns>
        public List<string> GetStringValues()
        {
            List<string> values = new List<string>();
            //Look for our string value associated with fields in this enum
            foreach (FieldInfo fi in this.enumType.GetFields())
            {
                //Check for our custom attribute
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs == null)
                {
                    continue;
                }

                if (attrs.Length > 0)
                {
                    values.Add(attrs[0].Value);
                }
            }

            return values;
        }

        /// <summary>
        /// Gets the enum entry and string value pairs.
        /// </summary>
        /// <returns>A dictionary containing each enum entry with its corresponding string value.</returns>
        public Dictionary<T, string> GetValues()
        {
            Dictionary<T, string> values = new Dictionary<T, string>();
            //Look for our string value associated with fields in this enum
            foreach (FieldInfo fi in this.enumType.GetFields())
            {
                //Check for our custom attribute
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs == null)
                {
                    continue;
                }

                if (attrs.Length > 0)
                {
                    object str = Enum.Parse(this.enumType, fi.Name);
                    values.Add((T) str, attrs[0].Value);
                }
            }

            return values;
        }

        #endregion

        #region static methods

        /// <summary>
        /// Return the existence of the given string value within the enum.
        /// </summary>
        /// <param name="value">String value.</param>
        /// <returns>Existence of the string value</returns>
        public static bool IsStringDefined(string value)
        {
            return IsStringDefined(value, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Return the existence of the given string value within the enum.
        /// </summary>
        /// <param name="value">String value.</param>
        /// <param name="comparisonType">Specifies how to conduct a case-insensitive match on the supplied string value</param>
        /// <returns>Existence of the string value</returns>
        public static bool IsStringDefined(string value, StringComparison comparisonType)
        {
            List<string> values = new StringEnum<T>().GetStringValues();
            foreach (string s in values)
            {
                if (s.Equals(value, comparisonType)) return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a string value for a particular enum value.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>String Value associated via a <see cref="StringValueAttribute"/> attribute, or null if not found.</returns>
        public static string GetStringValue(T value)
        {
            string output = null;
            Type type = value.GetType();
            Hashtable stringValues = new Hashtable();

            if (stringValues.ContainsKey(value))
            {
                if (stringValues[value] != null)
                {
                    output = ((StringValueAttribute) stringValues[value]).Value;
                }
            }
            else
            {
                //Look for our 'StringValueAttribute' in the field's custom attributes
                FieldInfo fi = type.GetField(value.ToString());
                if (fi != null)
                {
                    if (fi.GetCustomAttributes(typeof(StringValueAttribute), false) is StringValueAttribute[] attrs)
                    {
                        if (attrs.Length > 0)
                        {
                            stringValues.Add(value, attrs[0]);
                            output = attrs[0].Value;
                        }
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Parses the supplied enum and string value to find an associated enum value.
        /// </summary>
        /// <param name="value">String value.</param>
        /// <returns>Enum value associated with the string value, if not found the default enum will be returned.</returns>
        public static T Parse(string value)
        {
            return Parse(value, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Parses the supplied enum and string value to find an associated enum value.
        /// </summary>
        /// <param name="value">String value.</param>
        /// <param name="comparisonType">Specifies how to conduct a case-insensitive match on the supplied string value.</param>
        /// <returns>Enum value associated with the string value, if not found the default enum will be returned.</returns>
        public static T Parse(string value, StringComparison comparisonType)
        {
            Type type = typeof(T);
            T output = default;
            string enumStringValue = null;

            //Look for our string value associated with fields in this enum
            foreach (FieldInfo fi in type.GetFields())
            {
                //Check for our custom attribute
                if (fi.GetCustomAttributes(typeof(StringValueAttribute), false) is StringValueAttribute[] attrs)
                {
                    if (attrs.Length > 0)
                    {
                        enumStringValue = attrs[0].Value;
                    }
                }

                //Check for equality then select actual enum value.
                if (string.Compare(enumStringValue, value, comparisonType) == 0)
                {
                    if (Enum.IsDefined(type, fi.Name))
                    {
                        output = (T) Enum.Parse(type, fi.Name);
                    }
                    break;
                }
            }

            return output;
        }

        #endregion
    }

    #region Class StringValueAttribute

    /// <summary>
    /// Simple attribute class for storing String Values
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StringValueAttribute : Attribute
    {
        private readonly string value;

        /// <summary>
        /// Creates a new <see cref="StringValueAttribute"/> instance.
        /// </summary>
        /// <param name="value">Value.</param>
        public StringValueAttribute(string value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value
        {
            get { return this.value; }
        }
    }

    #endregion
}