using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Extensions
{
    public static class SqlCeResultSetExtensions
    {
        public static TValue SafeGet<TValue>(this SqlCeResultSet self, string columnName, TValue defaultValue = default(TValue))
        {
            var t = typeof(TValue);
            var ordinal = self.GetOrdinal(columnName);
            if (self.IsDBNull(ordinal)) return defaultValue;

            dynamic value;
            if (t == typeof(int)) value = self.GetInt32(ordinal);
            else if (t == typeof(long)) value = self.GetInt64(ordinal);
            else if (t == typeof(bool)) value = self.GetBoolean(ordinal);
            else if (t == typeof(object)) value = self.GetValue(ordinal);
            else if (t == typeof(string)) value = self.GetString(ordinal);
            else if (t == typeof(int?) || t == typeof(long?) || t == typeof(bool?)) value = self.GetValue(ordinal);
            else throw new ApplicationException($"{nameof(SafeGet)} does not support type '{t.Name}'!");

            return value == null ? defaultValue : (TValue)Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue));
        }
    }
}
