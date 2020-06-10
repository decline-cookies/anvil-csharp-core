using System;


namespace TinyJSON
{
    public class ProxyString : Variant
    {
        protected readonly string value;


        public ProxyString( string value )
        {
            this.value = value;
        }


        public override string ToString( IFormatProvider provider )
        {
            return value;
        }

        public override char ToChar( IFormatProvider provider )
        {
            return value[0];
        }

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(Guid))
            {
                return new Guid( value );
            }

            return base.ToType( conversionType, provider );
        }
    }
}
