using System.Data;
using System.Data.Sql;
using System.Data.SqlServerCe;


class DBConnection
{

    public static void connect()
    {

        string connString = @"Data Source=C:\Documents and Settings\Dziekan\my documents\visual studio 2010\Projects\FTP_XML_APLI\FTP_XML_APLI\Zamowienia.sdf";
        SqlCeConnection connection = new SqlCeConnection(connString);
        connection.Open();

        connection.Close();
    }

}