using Xunit;
using HatTrick.CommandLine;

namespace HatTrick.CommandLine.Test
{
    public class TypeMapTests
    {
        #region aliasing
        [Fact]
        public void Type_GetAliasOrName_ShouldReturn_TypeAliasName_WhenTypeDefined()
        {
            Assert.Equal("byte", OptionTypeMap.GetAliasOrName(typeof(byte)));
            Assert.Equal("sbyte", OptionTypeMap.GetAliasOrName(typeof(sbyte)));
            Assert.Equal("short", OptionTypeMap.GetAliasOrName(typeof(short)));
            Assert.Equal("ushort", OptionTypeMap.GetAliasOrName(typeof(ushort)));
            Assert.Equal("int", OptionTypeMap.GetAliasOrName(typeof(int)));
            Assert.Equal("uint", OptionTypeMap.GetAliasOrName(typeof(uint)));
            Assert.Equal("long", OptionTypeMap.GetAliasOrName(typeof(long)));
            Assert.Equal("ulong", OptionTypeMap.GetAliasOrName(typeof(ulong)));
            Assert.Equal("nint", OptionTypeMap.GetAliasOrName(typeof(nint)));
            Assert.Equal("nuint", OptionTypeMap.GetAliasOrName(typeof(nuint)));
            Assert.Equal("float", OptionTypeMap.GetAliasOrName(typeof(float)));
            Assert.Equal("double", OptionTypeMap.GetAliasOrName(typeof(double)));
            Assert.Equal("decimal", OptionTypeMap.GetAliasOrName(typeof(decimal)));
            Assert.Equal("object", OptionTypeMap.GetAliasOrName(typeof(object)));
            Assert.Equal("bool", OptionTypeMap.GetAliasOrName(typeof(bool)));
            Assert.Equal("char", OptionTypeMap.GetAliasOrName(typeof(char)));
            Assert.Equal("string", OptionTypeMap.GetAliasOrName(typeof(string)));
            Assert.Equal("void", OptionTypeMap.GetAliasOrName(typeof(void)));
            Assert.Equal("byte?", OptionTypeMap.GetAliasOrName(typeof(byte?)));
            Assert.Equal("sbyte?", OptionTypeMap.GetAliasOrName(typeof(sbyte?)));
            Assert.Equal("short?", OptionTypeMap.GetAliasOrName(typeof(short?)));
            Assert.Equal("ushort?", OptionTypeMap.GetAliasOrName(typeof(ushort?)));
            Assert.Equal("int?", OptionTypeMap.GetAliasOrName(typeof(int?)));
            Assert.Equal("uint?", OptionTypeMap.GetAliasOrName(typeof(uint?)));
            Assert.Equal("long?", OptionTypeMap.GetAliasOrName(typeof(long?)));
            Assert.Equal("ulong?", OptionTypeMap.GetAliasOrName(typeof(ulong?)));
            Assert.Equal("nint?", OptionTypeMap.GetAliasOrName(typeof(nint?)));
            Assert.Equal("nuint?", OptionTypeMap.GetAliasOrName(typeof(nuint?)));
            Assert.Equal("float?", OptionTypeMap.GetAliasOrName(typeof(float?)));
            Assert.Equal("double?", OptionTypeMap.GetAliasOrName(typeof(double?)));
            Assert.Equal("decimal?", OptionTypeMap.GetAliasOrName(typeof(decimal?)));
            Assert.Equal("bool?", OptionTypeMap.GetAliasOrName(typeof(bool?)));
            Assert.Equal("char?", OptionTypeMap.GetAliasOrName(typeof(char?)));
            Assert.Equal("DateTime?", OptionTypeMap.GetAliasOrName(typeof(DateTime?)));
            Assert.Equal("DateOnly?", OptionTypeMap.GetAliasOrName(typeof(DateOnly?)));
            Assert.Equal("DateTimeOffset?", OptionTypeMap.GetAliasOrName(typeof(DateTimeOffset?)));
            Assert.Equal("TimeSpan?", OptionTypeMap.GetAliasOrName(typeof(TimeSpan?)));
            Assert.Equal("Guid?", OptionTypeMap.GetAliasOrName(typeof(Guid?)));
        }

        [Fact]
        public void Type_GetAliasOrName_ShouldReturn_TypeName_WhenTypeNotDefined()
        {
            //should return name
            Assert.Equal("TimeSpan", OptionTypeMap.GetAliasOrName(typeof(TimeSpan)));
            Assert.Equal("Guid", OptionTypeMap.GetAliasOrName(typeof(Guid)));
            Assert.Equal("Array", OptionTypeMap.GetAliasOrName(typeof(Array)));
        }
        #endregion

        #region get type argument [string]
        [Fact]
        public void TypeChange_ForString_ShouldReturn_StringProvided()
        {
            string val = "abc";
            string result = OptionTypeMap.GetTypedArgument<string>(val);
            Assert.Equal("abc", result);
        }

        [Fact]
        public void TypeChange_ForString_ShouldReturn_Null_WhenNullProvided()
        {
            string? val = null;
            string result = OptionTypeMap.GetTypedArgument<string>(val);
            Assert.Null(result);
        }

        [Fact]
        public void TypeChange_ForString_ShouldReturn_Empty_WhenEmptyProvided()
        {
            string? val = string.Empty;
            string result = OptionTypeMap.GetTypedArgument<string>(val);
            Assert.Equal(string.Empty, result);
        }
        #endregion

        #region get typed argument [bool]
        [Fact]
        public void TypeChange_ForBool_ShouldReturn_True_WhenNullProvided()
        {
            bool result = OptionTypeMap.GetTypedArgument<bool>(null);
            Assert.True(result);
        }

        [Fact]
        public void TypeChange_ForNullableBool_ShouldReturn_Null_WhenNullProvided()
        {
            bool? result = OptionTypeMap.GetTypedArgument<bool?>(null);
            Assert.Null(result);
        }

        [Fact]
        public void TypeChange_ForBool_ShouldThrow_FormatException_WhenEmptyProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<bool>(string.Empty);
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForNullableBool_ShouldThrow_FormatException_WhenEmptyProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<bool?>(string.Empty);
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForBool_ShouldReturn_True_WhenTruthyValueProvided()
        {
            bool result1 = OptionTypeMap.GetTypedArgument<bool>("true");
            bool result2 = OptionTypeMap.GetTypedArgument<bool>("TRUE");
            bool result3 = OptionTypeMap.GetTypedArgument<bool>("yes");
            bool result4 = OptionTypeMap.GetTypedArgument<bool>("YES");
            bool result5 = OptionTypeMap.GetTypedArgument<bool>("y");
            bool result6 = OptionTypeMap.GetTypedArgument<bool>("Y");
            bool result7 = OptionTypeMap.GetTypedArgument<bool>("1");

            Assert.True(result1);
            Assert.True(result2);
            Assert.True(result3);
            Assert.True(result4);
            Assert.True(result5);
            Assert.True(result6);
            Assert.True(result7);
        }

        [Fact]
        public void TypeChange_ForNullableBool_ShouldReturn_True_WhenTruthyValueProvided()
        {
            bool? result1 = OptionTypeMap.GetTypedArgument<bool?>("true");
            bool? result2 = OptionTypeMap.GetTypedArgument<bool?>("TRUE");
            bool? result3 = OptionTypeMap.GetTypedArgument<bool?>("yes");
            bool? result4 = OptionTypeMap.GetTypedArgument<bool?>("YES");
            bool? result5 = OptionTypeMap.GetTypedArgument<bool?>("y");
            bool? result6 = OptionTypeMap.GetTypedArgument<bool?>("Y");
            bool? result7 = OptionTypeMap.GetTypedArgument<bool?>("1");

            Assert.True(result1);
            Assert.True(result2);
            Assert.True(result3);
            Assert.True(result4);
            Assert.True(result5);
            Assert.True(result6);
            Assert.True(result7);
        }

        [Fact]
        public void TypeChange_ForBool_ShouldReturn_False_WhenFalsyValueProvided()
        {
            bool result1 = OptionTypeMap.GetTypedArgument<bool>("false");
            bool result2 = OptionTypeMap.GetTypedArgument<bool>("False");
            bool result3 = OptionTypeMap.GetTypedArgument<bool>("no");
            bool result4 = OptionTypeMap.GetTypedArgument<bool>("NO");
            bool result5 = OptionTypeMap.GetTypedArgument<bool>("n");
            bool result6 = OptionTypeMap.GetTypedArgument<bool>("N");
            bool result7 = OptionTypeMap.GetTypedArgument<bool>("0");

            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.False(result4);
            Assert.False(result5);
            Assert.False(result6);
            Assert.False(result7);
        }

        [Fact]
        public void TypeChange_ForNullableBool_ShouldReturn_False_WhenFalsyValueProvided()
        {
            bool? result1 = OptionTypeMap.GetTypedArgument<bool?>("false");
            bool? result2 = OptionTypeMap.GetTypedArgument<bool?>("False");
            bool? result3 = OptionTypeMap.GetTypedArgument<bool?>("no");
            bool? result4 = OptionTypeMap.GetTypedArgument<bool?>("NO");
            bool? result5 = OptionTypeMap.GetTypedArgument<bool?>("n");
            bool? result6 = OptionTypeMap.GetTypedArgument<bool?>("N");
            bool? result7 = OptionTypeMap.GetTypedArgument<bool?>("0");

            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.False(result4);
            Assert.False(result5);
            Assert.False(result6);
            Assert.False(result7);
        }

        [Fact]
        public void TypeChanage_ForBool_ShouldThrow_FormatException_WhenInvalidValueProvide()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<bool>("xxxx");
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChanage_ForNullableBool_ShouldThrow_FormatException_WhenInvalidValueProvide()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<bool?>("xxxx");
            Assert.Throws<FormatException>(action);
        }
        #endregion

        #region get typed argument [IConvertible]
        //Target IConvertable types: (i'm not going to write tests for all of these)
        //Byte,Char,DateTime,Decimal,Double,Enum,Int16,Int32,Int64,SByte,Single,UInt16,UInt32,UInt64
        [Fact]
        public void TypeChange_ForIConvertible_ShouldThrow_InvalidCastException_WhenNullProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<int>(null);
            Assert.Throws<InvalidCastException>(action);
        }

        [Fact]
        public void TypeChange_ForIConvertible_ShouldThrow_FormatException_WhenEmptyProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<int>(string.Empty);
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForIConvertible_ShouldReturn_ConvertedType_WhenInputIsValid()
        {
            DateTime target = new DateTime(2020, 04, 01, 01, 01, 01);
            DateTime result = OptionTypeMap.GetTypedArgument<DateTime>(target.ToString());
            Assert.Equal(target, result);
        }

        [Fact]
        public void TypeChanage_ForIConvertable_ShouldThrow_FormatException_WhenInvalidValueProvide()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<long>("xxxx");
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForNullableIConvertible_ShouldReturn_Null_WhenNullProvided()
        {
            int? result = OptionTypeMap.GetTypedArgument<int?>(null);
            Assert.Null(result);
        }

        [Fact]
        public void TypeChange_ForNullableIConvertible_ShouldThrow_FormatException_WhenEmptyProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<int?>(string.Empty);
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForNullableIConvertible_ShouldReturn_ConvertedType_WhenInputIsValid()
        {
            DateTime target = new DateTime(2020, 04, 01, 01, 01, 01);
            DateTime? result = OptionTypeMap.GetTypedArgument<DateTime?>(target.ToString());
            Assert.Equal(target, result);
        }

        [Fact]
        public void TypeChanage_ForNullableIConvertable_ShouldThrow_FormatException_WhenInvalidValueProvide()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<long?>("xxxx");
            Assert.Throws<FormatException>(action);
        }
        #endregion

        #region get typed argument [dateonly]
        [Fact]
        public void TypeChange_ForDateOnly_ShouldThrow_InvalidCastException_WhenNullProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<DateOnly>(null);
            Assert.Throws<InvalidCastException>(action);
        }

        [Fact]
        public void TypeChange_ForDateOnly_ShouldThrow_FormatExceptionException_WhenEmptyProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<DateOnly>(string.Empty);
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForDateOnly_ShouldReturn_ConvertedType_WhenInputIsValid()
        {
            DateOnly target = new DateOnly(2020, 04, 01);
            DateOnly result = OptionTypeMap.GetTypedArgument<DateOnly>(target.ToString());
            Assert.Equal(target, result);
        }

        [Fact]
        public void TypeChanage_ForDateOnly_ShouldThrow_FormatException_WhenInvalidValueProvide()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<DateOnly>("abcdef");
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForNullableDateOnly_ShouldReturn_Null_WhenNullProvided()
        {
            DateOnly? result = OptionTypeMap.GetTypedArgument<DateOnly?>(null);
            Assert.Null(result);
        }

        [Fact]
        public void TypeChange_ForNullableDateOnly_ShouldThrow_FormatExceptionException_WhenEmptyProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<DateOnly?>(string.Empty);
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForNullableDateOnly_ShouldReturn_ConvertedType_WhenInputIsValid()
        {
            DateOnly? target = new DateOnly(2020, 04, 01);
            DateOnly? result = OptionTypeMap.GetTypedArgument<DateOnly?>(target.Value.ToString());
            Assert.Equal(target, result);
        }

        [Fact]
        public void TypeChanage_ForNullableDateOnly_ShouldThrow_FormatException_WhenInvalidValueProvide()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<DateOnly?>("abcdef");
            Assert.Throws<FormatException>(action);
        }
        #endregion

        #region get typed argument [datetimeoffset]
        [Fact]
        public void TypeChange_ForDateTimeOffset_ShouldThrow_InvalidCastException_WhenNullProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<DateTimeOffset>(null);
            Assert.Throws<InvalidCastException>(action);
        }

        [Fact]
        public void TypeChange_ForDateTimeOffset_ShouldThrow_FormatException_WhenEmptyProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<DateTimeOffset>(string.Empty);
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForDateTimeOffset_ShouldReturn_ConvertedType_WhenInputIsValid()
        {
            DateTimeOffset target = new DateTimeOffset(new DateTime(2020, 04, 01), new TimeSpan(1, 5, 0));
            DateTimeOffset result = OptionTypeMap.GetTypedArgument<DateTimeOffset>(target.ToString());
            Assert.Equal(target, result);
        }


        [Fact]
        public void TypeChanage_ForDateTimeOffset_ShouldThrow_FormatException_WhenInvalidValueProvide()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<DateTimeOffset>("abcdef");
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForNullableDateTimeOffset_ShouldReturn_Null_WhenNullProvided()
        {
            DateTimeOffset? result = OptionTypeMap.GetTypedArgument<DateTimeOffset?>(null);
            Assert.Null(result);
        }

        [Fact]
        public void TypeChange_ForNullableDateTimeOffset_ShouldThrow_FormatExceptionException_WhenEmptyProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<DateTimeOffset?>(string.Empty);
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForNullableDateTimeOffset_ShouldReturn_ConvertedType_WhenInputIsValid()
        {
            DateTimeOffset? target = new DateTimeOffset(new DateTime(2020, 04, 01), new TimeSpan(1, 5, 0));
            DateTimeOffset? result = OptionTypeMap.GetTypedArgument<DateTimeOffset?>(target.Value.ToString());
            Assert.Equal(target, result);
        }

        [Fact]
        public void TypeChanage_ForNullableDateTimeOffsetShouldThrow_FormatException_WhenInvalidValueProvide()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<DateTimeOffset?>("abcdef");
            Assert.Throws<FormatException>(action);
        }
        #endregion

        #region get typed argument [timespan]
        [Fact]
        public void TypeChange_ForTimeSpan_ShouldThrow_InvalidCastException_WhenNullProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<TimeSpan>(null);
            Assert.Throws<InvalidCastException>(action);
        }

        [Fact]
        public void TypeChange_ForTimeSpan_ShouldThrow_FormatException_WhenEmptyProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<TimeSpan>(string.Empty);
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForTimeSpan_ShouldReturn_ConvertedType_WhenInputIsValid()
        {
            TimeSpan target = new TimeSpan(1, 2, 30);
            TimeSpan result = OptionTypeMap.GetTypedArgument<TimeSpan>(target.ToString());
            Assert.Equal(target, result);
        }

        [Fact]
        public void TypeChanage_ForTimeSpan_ShouldThrow_FormatException_WhenInvalidValueProvide()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<TimeSpan>("abcdef");
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForNullableTimeSpan_ShouldReturn_Null_WhenNullProvided()
        {
            TimeSpan? result = OptionTypeMap.GetTypedArgument<TimeSpan?>(null);
            Assert.Null(result);
        }

        [Fact]
        public void TypeChange_ForNullableTimeSpan_ShouldThrow_FormatExceptionException_WhenEmptyProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<TimeSpan?>(string.Empty);
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForNullableTimeSpan_ShouldReturn_ConvertedType_WhenInputIsValid()
        {
            TimeSpan? target = new TimeSpan(2020, 04, 01);
            TimeSpan? result = OptionTypeMap.GetTypedArgument<TimeSpan?>(target.Value.ToString());
            Assert.Equal(target, result);
        }

        [Fact]
        public void TypeChanage_ForNullableTimeSpan_ShouldThrow_FormatException_WhenInvalidValueProvide()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<TimeSpan?>("xxxxx");
            Assert.Throws<FormatException>(action);
        }
        #endregion

        #region get typed argument [guid]
        [Fact]
        public void TypeChange_ForGuid_ShouldThrow_InvalidCastException_WhenNullProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<Guid>(null);
            Assert.Throws<InvalidCastException>(action);
        }

        [Fact]
        public void TypeChange_ForGuid_ShouldThrow_FormatException_WhenEmptyProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<Guid>(string.Empty);
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForGuid_ShouldReturn_ConvertedType_WhenInputIsValid()
        {
            Guid target = Guid.NewGuid();
            Guid result = OptionTypeMap.GetTypedArgument<Guid>(target.ToString("D"));
            Assert.Equal(target, result);
        }

        [Fact]
        public void TypeChanage_ForGuid_ShouldThrow_FormatException_WhenInvalidValueProvide()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<Guid>("abcdef");
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForNullableGuid_ShouldReturn_Null_WhenNullProvided()
        {
            Guid? result = OptionTypeMap.GetTypedArgument<Guid?>(null);
            Assert.Null(result);
        }

        [Fact]
        public void TypeChange_ForNullableGuid_ShouldThrow_FormatException_WhenEmptyProvided()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<Guid?>(string.Empty);
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void TypeChange_ForNullableGuid_ShouldReturn_ConvertedType_WhenInputIsValid()
        {
            Guid? target = Guid.NewGuid();
            Guid? result = OptionTypeMap.GetTypedArgument<Guid?>(target.Value.ToString("D"));
            Assert.Equal(target, result);
        }

        [Fact]
        public void TypeChanage_ForNullableGuid_ShouldThrow_FormatException_WhenInvalidValueProvide()
        {
            Action action = () => OptionTypeMap.GetTypedArgument<Guid?>("abcdef");
            Assert.Throws<FormatException>(action);
        }
        #endregion
    }
}
