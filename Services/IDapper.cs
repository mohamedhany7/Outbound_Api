using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Outbound_Api.Services
{
    public interface IDapper : IDisposable
    {
        DbConnection GetDbconnection();
        T Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        List<T> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        DynamicParameters Execute(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        object ExecuteMsg(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        T Insert<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        T Update<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        List<dynamic> executeQuerry(string sql);
    }
}
