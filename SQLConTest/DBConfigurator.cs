using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using DataProtection;
using Microsoft.Win32;

namespace SQLConTest
{
    public class DBConfigurator
    {
        private String _strDBConn;
        private String _strSQL;

        private SqlConnection adSqlConn;

        public String DBConn {
            get { return _strDBConn; }
        }

        public DBConfigurator()
        {
            _strSQL = "SELECT session_id, blocking_session_id FROM SYS.dm_exec_requests WHERE session_id > 50";
        }

        public void ConnectionsTest() {
            var sDate = DateTime.Now.ToShortDateString();
            var noOfConn = Convert.ToInt32(ConfigurationManager.AppSettings["noOfConn"]);
            for (int i = 1; i < noOfConn + 1; i++) {
                //Console.WriteLine("Start Connection to " + ConfigurationManager.AppSettings["DBServer" + i] + " " + ConfigurationManager.AppSettings["DBName" + i] + "...");

                _strDBConn = "Data Source=" + ConfigurationManager.AppSettings["DBServer" + i];
                _strDBConn = _strDBConn + ";Initial Catalog=" + ConfigurationManager.AppSettings["DBName" + i];
                _strDBConn = _strDBConn + ";User ID=" + ConfigurationManager.AppSettings["DBUser" + i];
                //_strDBConn = _strDBConn + ";Password=" + ConfigurationManager.AppSettings["DBPswd" + i];
                _strDBConn = _strDBConn + ";Password=" + DPAPIDecryptFromReg(ConfigurationManager.AppSettings["DBPswd" + i]);
                
                try
                {
                    if (ConnTest())
                    {
                        Console.WriteLine("[" + sDate + "][DBConnectionTest][" + ConfigurationManager.AppSettings["DBServer" + i] + "][" + ConfigurationManager.AppSettings["DBName" + i] + "]Succeed");
                    }
                    else
                    {
                        Console.WriteLine("[" + sDate + "][DBConnectionTest][" + ConfigurationManager.AppSettings["DBServer" + i] + "][" + ConfigurationManager.AppSettings["DBName" + i] + "]Failed");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[" + sDate + "][DBConnectionTest][" + ConfigurationManager.AppSettings["DBServer" + i] + "][" + ConfigurationManager.AppSettings["DBName" + i] + "]Failed : " + ex.Message);
                }
                
            }
        }

        public bool ConnTest()
        {
            var mResult = new DataSet();

            try
            {
                using (adSqlConn = new SqlConnection(_strDBConn))
                {
                    using (SqlCommand adcmd = new SqlCommand(_strSQL, adSqlConn))
                    {
                        adSqlConn.Open();
                        adcmd.CommandTimeout = Int32.Parse(ConfigurationManager.AppSettings["DBTimeOut"]);
                        adcmd.ExecuteNonQuery();

                        SqlDataAdapter da = new SqlDataAdapter();
                        DataSet ds = new DataSet();
                        da.SelectCommand = adcmd;

                        da.Fill(ds, "COMMON");
                        mResult = ds;

                        if (mResult.Tables[0].Rows.Count > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                //return false;
                throw ex;
            }
        }

        private String DPAPIDecryptFromReg(String Key)
        {
            string sResult = "";

            DataProtector dp = new DataProtector(DataProtection.DataProtector.Store.USE_MACHINE_STORE);

            try
            {
                Byte[] entropy = { 9, 4, 7, 5, 1, 3, 2 };
                Byte[] dataToDecrypt = { };

                //A 32-bit application on a 64-bit OS will be looking at the HKLM\Software\Wow6432Node node by default. 
                //To read the 64-bit version of the key, you'll need to specify the RegistryView:
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    //RegistryKey regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\DBSAMS\\ECPORTAL", true);
                    using (var regKey = hklm.OpenSubKey("SOFTWARE\\DBSAMS\\ECPORTAL", true))
                    {
                        if (regKey == null)
                        {
                            sResult = Key;
                            return sResult;
                        }
                        dataToDecrypt = (Byte[])regKey.GetValue(Key);
                    }
                }

                if (dataToDecrypt != null && entropy != null)
                {
                    sResult = Encoding.ASCII.GetString(dp.Decrypt(dataToDecrypt, entropy));
                }
                else
                {
                    sResult = Key;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Decrypt err msg: " + ex.Message);
                return sResult = "";
            }

            return sResult;
        }
    }
}

