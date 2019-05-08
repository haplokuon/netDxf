#region netDxf library, Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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
    public class StringEnum<T> where T: struct, IConvertible // TODO: when upgrading to C# 7.3 change to System.Enum
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
            if(!this.enumType.IsEnum)
                throw new ArgumentException("T must be an Enum", nameof(T));
        }

        #endregion

        #region public propertiex

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
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof (StringValueAttribute), false) as StringValueAttribute[];
                if (attrs != null)
                    if (attrs.Length > 0)
                        values.Add(attrs[0].Value);
            }

            return values;
        }

        /// <summary>
        /// Gets the values as a list data source.
        /// </summary>
        /// <returns>IList for data binding</returns>
        public Dictionary<T, string> GetValues()
        {
            Type underlyingType = Enum.GetUnderlyingType(this.enumType);

            Dictionary<T, string> values = new Dictionary<T, string>();
            //Look for our string value associated with fields in this enum
            foreach (FieldInfo fi in this.enumType.GetFields())
            {
                //Check for our custom attribute
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof (StringValueAttribute), false) as StringValueAttribute[];
                if (attrs != null)
                    if (attrs.Length > 0)
                    {
                        object str = Convert.ChangeType(Enum.Parse(this.enumType, fi.Name), underlyingType);
                        if (str == null)
                            throw new Exception();
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
                output = ((StringValueAttribute) stringValues[value]).Value;
            else
            {
                //Look for our 'StringValueAttribute' in the field's custom attributes
                FieldInfo fi = type.GetField(value.ToString());
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof (StringValueAttribute), false) as StringValueAttribute[];
                if (attrs != null)
                    if (attrs.Length > 0)
                    {
                        stringValues.Add(value, attrs[0]);
                        output = attrs[0].Value;
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
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            T output = default(T);
            string enumStringValue = null;

            if (!type.IsEnum)
                throw new ArgumentException(string.Format("The supplied type \"{0}\" must be an Enum.", type));

            //Look for our string value associated with fields in this enum
            foreach (FieldInfo fi in type.GetFields())
            {
                //Check for our custom attribute
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof (StringValueAttribute), false) as StringValueAttribute[];
                if (attrs != null)
                    if (attrs.Length > 0)
                        enumStringValue = attrs[0].Value;

                //Check for equality then select actual enum value.
                if (string.Compare(enumStringValue, value, comparisonType) == 0)
                {
                    if (Enum.IsDefined(type, fi.Name))
                        output = (T) Enum.Parse(type, fi.Name);
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