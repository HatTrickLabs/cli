using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatTrick.CommandLine
{
    public static class OptionTypeMap
    {
        #region internals
        private static readonly Dictionary<Type, string> TypeAliases = new()
        {
            { typeof(byte),                     "byte"              },
            { typeof(sbyte),                    "sbyte"             },
            { typeof(short),                    "short"             },
            { typeof(ushort),                   "ushort"            },
            { typeof(int),                      "int"               },
            { typeof(uint),                     "uint"              },
            { typeof(long),                     "long"              },
            { typeof(ulong),                    "ulong"             },
            { typeof(nint),                     "nint"              },
            { typeof(nuint),                    "nuint"             },
            { typeof(float),                    "float"             },
            { typeof(double),                   "double"            },
            { typeof(decimal),                  "decimal"           },
            { typeof(object),                   "object"            },
            { typeof(bool),                     "bool"              },
            { typeof(char),                     "char"              },
            { typeof(string),                   "string"            },
            { typeof(void),                     "void"              },
            { typeof(Nullable<byte>),           "byte?"             },
            { typeof(Nullable<sbyte>),          "sbyte?"            },
            { typeof(Nullable<short>),          "short?"            },
            { typeof(Nullable<ushort>),         "ushort?"           },
            { typeof(Nullable<int>),            "int?"              },
            { typeof(Nullable<uint>),           "uint?"             },
            { typeof(Nullable<long>),           "long?"             },
            { typeof(Nullable<ulong>),          "ulong?"            },
            { typeof(Nullable<nint>),           "nint?"             },
            { typeof(Nullable<nuint>),          "nuint?"            },
            { typeof(Nullable<float>),          "float?"            },
            { typeof(Nullable<double>),         "double?"           },
            { typeof(Nullable<decimal>),        "decimal?"          },
            { typeof(Nullable<bool>),           "bool?"             },
            { typeof(Nullable<char>),           "char?"             },
            { typeof(Nullable<DateTime>),       "DateTime?"         },
            { typeof(Nullable<DateOnly>),       "DateOnly?"         },
            { typeof(Nullable<DateTimeOffset>), "DateTimeOffset?"   },
            { typeof(Nullable<TimeSpan>),       "TimeSpan?"         },
            { typeof(Nullable<Guid>),           "Guid?"             },
        };
        #endregion

        #region get alias or name
        public static string GetAliasOrName(Type type)
        {
            var aliases = OptionTypeMap.TypeAliases;
            return aliases.TryGetValue(type, out string alias) ? alias : type.Name;
        }
        #endregion

        #region change option argument type
        public static T GetTypedArgument<T>(string argument)
        {
            return (T)OptionTypeMap.ChangeType(argument, typeof(T));
        }

        private static object ChangeType(string value, Type changeTo)
        {
            if (changeTo == typeof(string))
                return value;

            if (value is null)
            {
                //if a bool op is provided, no argument should be required...the flag alone results in true.
                if (changeTo == typeof(bool))
                    return true;

                if (changeTo.IsValueType)
                    throw new InvalidCastException($"Cannot parse null into value type of {OptionTypeMap.GetAliasOrName(changeTo)}");

                return null;
            }

            //at this point, we know the value is NOT null
            Type underlying = Nullable.GetUnderlyingType(changeTo) ?? changeTo;

            if (underlying == typeof(bool))
            {
                //if a bool op is provided, no argument should be required...the flag alone results in true.
                if (value == string.Empty)
                    return true;
                else//at this point we've accounted for null and empty string, run it through the bool converter.
                    return BooleanConverter.ToBoolean(value);
            }

            if (typeof(IConvertible).IsAssignableFrom(underlying))
                return Convert.ChangeType(value, changeTo);

            if (underlying == typeof(DateTime))
                return DateTime.Parse(value);

            if (underlying == typeof(DateOnly))
                return DateOnly.Parse(value);

            if (underlying == typeof(DateTimeOffset))
                return DateTimeOffset.Parse(value);

            if (underlying == typeof(TimeSpan))
                return TimeSpan.Parse(value);

            if (underlying == typeof(Guid))
                return Guid.Parse(value);

            throw new NotImplementedException($"Cannot change provided value '{value}' to provided type '{OptionTypeMap.GetAliasOrName(changeTo)}'.");
        }
        #endregion
    }
}
