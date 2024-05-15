using SqlSugar;

namespace _2DSurviveGameServer.Helpers
{
    public class SqlSugarHelper
    {
        public static SqlSugarScope Db = new SqlSugarScope(new ConnectionConfig()
        {
            ConnectionString = "server=localhost;Database=game11;Uid=root;Pwd=123456",//连接符字串
            DbType = DbType.MySql,//数据库类型
            IsAutoCloseConnection = true, //不设成true要手动close
            ConfigureExternalServices = new ConfigureExternalServices
            {
                EntityService = (t, column) =>
                {
                    if (column.PropertyName.ToLower() == "id")
                    {
                        column.IsPrimarykey = true;
                    }
                }
            }
        });
    }
}
