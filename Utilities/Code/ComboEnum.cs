using System;
using System.Collections.Generic;
using System.Linq;

namespace CoP.Enterprise
{
    public class ComboEnum<E> where E : struct
    {
        public E EnumValue { get; set; }
        private ComboEnum(E enumValue)
        { EnumValue = enumValue; }
        public static ComboEnum<E> Make(E enumValue)
        { return new ComboEnum<E>(enumValue); }
        public override string ToString()
        {
            var strVal = Enum.GetName(typeof(E), EnumValue);
            return strVal == null ||
                strVal.Equals("null",
                StringComparison.InvariantCultureIgnoreCase) ?
                String.Empty : strVal;
        }
        public static IEnumerable<ComboEnum<E>> GetValues()
        {
            return from E ev in Enum.GetValues(typeof(E))
                   select Make(ev);
        }
    }
}