using System;
using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public static class DbConnectionEx
    {
        public static void ExecuteNonQuery(this IDbConnection openConnection, string cmd)
        {
            using (IDbCommand command = openConnection.CreateCommand())
            {
                command.CommandText = cmd;
                command.ExecuteNonQuery();
            }
        }

        public static T ExecuteScalar<T>(this IDbConnection openConnection, string cmd)
        {
            using (IDbCommand command = openConnection.CreateCommand())
            {
                command.CommandText = cmd;
                object scalarResult = command.ExecuteScalar();
                if (typeof(T) == typeof(int)) return (T) (object) Convert.ToInt32(scalarResult);
                return (T) scalarResult;
            }
        }

        [UsedImplicitly]
        public static IEnumerable<T> ExecuteReader<T>(this IDbConnection openConnection, string cmd, Func<IDataReader, T> forEachResultFunc)
        {
            using (IDbCommand command = openConnection.CreateCommand())
            {
                command.CommandText = cmd;
                IDataReader reader = command.ExecuteReader();
                while (reader.NextResult()) yield return forEachResultFunc(reader);
            }
        }
    }
}