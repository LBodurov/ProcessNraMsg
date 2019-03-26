using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ProcessNRAmsg
{
    public delegate void SelectedItemDelegate(string selectedItem);

    public class MySqlConn
    {
        SqlConnection _dbConn;
        SProcList _sprocs;  //sproc parameter info cache
        SqlParameterCollection _lastParams; //used by Param()

        public MySqlConn(string connStr)
        {
            _dbConn = new SqlConnection(connStr);
            _sprocs = new SProcList(this);
        }

        public void Open()
        { if (_dbConn.State != ConnectionState.Open) _dbConn.Open(); }
        public void Close()
        { if (_dbConn.State == ConnectionState.Open) _dbConn.Close(); }

        SqlCommand NewSProc(string procName)
        {
            SqlCommand cmd = new SqlCommand(procName, _dbConn);
            cmd.CommandTimeout = 0;
            cmd.CommandType = CommandType.StoredProcedure;

#if EmulateDeriveParameters   //see below for our 
                              //own DeriveParameters
            MySqlCmdBuilder.DeriveParameters(cmd);
#else
            Open();
            SqlCommandBuilder.DeriveParameters(cmd);
            //SQL treats OUT params as REF params 
            //(thus requiring those parameters to be passed in)
            //if that's what you really want, remove 
            //the next three lines
            foreach (SqlParameter prm in cmd.Parameters)
                if (prm.Direction == ParameterDirection.InputOutput)
                    //make param a true OUT param
                    prm.Direction = ParameterDirection.Output;
#endif

            return cmd;
        }

        SqlCommand FillParams(string procName,
                                params object[] vals)
        {
            //get cached info (or cache if first call)
            SqlCommand cmd = _sprocs[procName];
            for (int j = 0; j < cmd.Parameters.Count; j++)
            {
                if (cmd.Parameters[j].TypeName.StartsWith(_dbConn.Database))
                    cmd.Parameters[j].TypeName = cmd.Parameters[j].TypeName.Substring(_dbConn.Database.Length + 1);
            }


            //fill parameter values for stored procedure call
            int i = 0;
            foreach (SqlParameter prm in cmd.Parameters)
            {
                //we got info for ALL the params - only 
                //fill the INPUT params
                if (prm.Direction == ParameterDirection.Input
                 || prm.Direction == ParameterDirection.InputOutput)
                    prm.Value = vals[i++];
            }
            //make sure the right number of parameters was passed
            Debug.Assert(i == (vals == null ? 0 : vals.Length));

            //for subsequent calls to Param()
            _lastParams = cmd.Parameters;
            return cmd;
        }

        //handy routine if you are in control of the input.
        //but if user input, vulnerable to sql injection attack
        public DataRowCollection QueryRows(string strQry)
        {
            DataTable dt = new DataTable();
            new SqlDataAdapter(strQry, _dbConn).Fill(dt);
            return dt.Rows;
        }

        public int ExecSProc(string procName,
                              params object[] vals)
        {
            int retVal = -1;  //some error code

            try
            {
                Open();
                FillParams(procName, vals).ExecuteNonQuery();
                retVal = (int)_lastParams[0].Value;
            }
            //any special handling for SQL-generated error here
            //catch (System.Data.SqlClient.SqlException esql) {}
            catch //(System.Exception e)
            {
                
            }
            finally
            {
                Close();
            }
            return retVal;
        }

        public DataSet ExecSProcDS(string procName,
                                     params object[] vals)
        {
            //saveLog(procName, vals);

            DataSet ds = new DataSet();

            try
            {
                Open();
                new SqlDataAdapter(
                      FillParams(procName, vals)).Fill(ds);
            }
            catch { }
            finally
            {
                Close();
            }

            return ds;
        }

        public DataSet ExecSProcDS_Open(string procName,
                                     params object[] vals)
        {
            DataSet ds = new DataSet();

            try
            {
                //  Open();
                new SqlDataAdapter(
                      FillParams(procName, vals)).Fill(ds);
            }
            catch { }
            finally
            {
                // Close();
            }

            return ds;
        }

        //get parameter from most recent ExecSProc
        public object Param(string param)
        {
            return _lastParams[param].Value;
        }

        class SProcList : DictionaryBase
        {
            MySqlConn _db;
            public SProcList(MySqlConn db)
            { _db = db; }

            public SqlCommand this[string name]
            {
                get
                {      //read-only, "install on demand"
                    if (!Dictionary.Contains(name))
                        Dictionary.Add(name, _db.NewSProc(name));
                    return (SqlCommand)Dictionary[name];
                }
            }
        }


    }
}