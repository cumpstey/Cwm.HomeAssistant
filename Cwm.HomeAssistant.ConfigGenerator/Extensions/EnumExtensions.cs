using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class EnumExtensions
    {
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var type = value.GetType();
            var member = type.GetMember(value.ToString());
            var attributes = member[0].GetCustomAttributes(typeof(T), false);
            return attributes.Any() ? (T)attributes[0] : null;
        }
    }
}
