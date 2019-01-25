       public static int InsertCARSReport(
            string JobNumber
            , string reportingEmployee
            , string DepartmentID // Declare as INT when we change back to using the foreign key ****************************************************************
            , string reworkEmployee
            , string problemID // Declare as INT when we change back to using the foreign key ****************************************************************
            , string severityID // Declare as INT when we change back to using the foreign key ****************************************************************
            , decimal calculatedCost
            , string notes
            , string correctiveAction
            )
        {
            string sp = "SP_CARS_2019_Master";
            int id = 0;
            int errorCode = 1; // set errorcode to true until we succeed ;)

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(repository.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sp, sqlConn))
                    {
                        cmd.Parameters.Add("@TranType", SqlDbType.VarChar).Value = "SaveReport";
                        cmd.Parameters.Add("@JobNumber", SqlDbType.VarChar).Value = JobNumber;
                        cmd.Parameters.Add("@reportingEmployee", SqlDbType.VarChar).Value = reportingEmployee;
                        cmd.Parameters.Add("@DepartmentID", SqlDbType.VarChar).Value = DepartmentID;  // replace with Int when we fix the foreign key issue ******************************
                        cmd.Parameters.Add("@reworkEmployee", SqlDbType.VarChar).Value = reworkEmployee;
                        cmd.Parameters.Add("@problemID", SqlDbType.VarChar).Value = problemID; // replace with Int when we fix the foreign key issue ******************************
                        cmd.Parameters.Add("@severityID", SqlDbType.VarChar).Value = severityID; // replace with Int when we fix the foreign key issue ******************************
                        cmd.Parameters.Add("@calculatedCost", SqlDbType.Decimal).Value = calculatedCost;
                        cmd.Parameters.Add("@notes", SqlDbType.VarChar).Value = notes;
                        cmd.Parameters.Add("@correctiveAction", SqlDbType.VarChar).Value = correctiveAction;

                        cmd.CommandType = CommandType.StoredProcedure;
                        sqlConn.Open();
                        SqlDataReader sqlDr = cmd.ExecuteReader();
                        if (sqlDr.HasRows)
                        {
                            sqlDr.Read();
                            if (sqlDr.GetInt32(0) == 0)
                            {
                                // Successs!
                                errorCode = 0;
                                id = sqlDr.GetInt32(2);
                            }
                            else
                            {
                                // Fail :(
                                id = errorCode;
                                Console.WriteLine("CARSReportClass Error Excuting: " + sp + " InsertCARSReport " + sqlDr.GetString(1));
                            }
                        }
                        return id;
                    }
                }  
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("CARSReportClass Error Excuting: " + sp);
                var emailBody = "CARSReportClass Error Excuting: " + sp + " InsertCARSReport <br />" + ex.ToString();
                MailSendHelper.SendingErrorEmail("donotreply@tshore.com", "jbrennan@tshore.com", emailBody);   

                return id;
            }
        }

        public static int UpdateCARSReport(
            int id
            , string JobNumber
            , string reportingEmployee
            , string DepartmentID // Declare as INT when we change back to using the foreign key ****************************************************************
            , string reworkEmployee
            , string problemID // Declare as INT when we change back to using the foreign key ****************************************************************
            , string severityID // Declare as INT when we change back to using the foreign key ****************************************************************
            , decimal calculatedCost
            , string notes
            , string correctiveAction
            )
        {
            string sp = "SP_CARS_2019_Master";
            int errorCode = 1; // set errorcode to fail until we succeed ;)

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(repository.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sp, sqlConn))
                    {
                        cmd.Parameters.Add("@TranType", SqlDbType.VarChar).Value = "SaveReport";
                        cmd.Parameters.Add("@id", SqlDbType.VarChar).Value = id;
                        cmd.Parameters.Add("@JobNumber", SqlDbType.VarChar).Value = JobNumber;
                        cmd.Parameters.Add("@reportingEmployee", SqlDbType.VarChar).Value = reportingEmployee;
                        cmd.Parameters.Add("@DepartmentID", SqlDbType.VarChar).Value = DepartmentID;  // replace with Int when we fix the foreign key issue ******************************
                        cmd.Parameters.Add("@reworkEmployee", SqlDbType.VarChar).Value = reworkEmployee;
                        cmd.Parameters.Add("@problemID", SqlDbType.VarChar).Value = problemID; // replace with Int when we fix the foreign key issue ******************************
                        cmd.Parameters.Add("@severityID", SqlDbType.VarChar).Value = severityID; // replace with Int when we fix the foreign key issue ******************************
                        cmd.Parameters.Add("@calculatedCost", SqlDbType.Decimal).Value = calculatedCost;
                        cmd.Parameters.Add("@notes", SqlDbType.VarChar).Value = notes;
                        cmd.Parameters.Add("@correctiveAction", SqlDbType.VarChar).Value = correctiveAction;

                        cmd.CommandType = CommandType.StoredProcedure;
                        sqlConn.Open();
                        SqlDataReader sqlDr = cmd.ExecuteReader();
                        if (sqlDr.HasRows)
                        {
                            sqlDr.Read();
                            if (sqlDr.GetInt32(0) == 0)
                            {
                                // Successs!
                                errorCode = 0;
                                id = sqlDr.GetInt32(2);
                            }
                            else
                            {
                                // Fail :(
                                id = errorCode;
                                Console.WriteLine("CARSReportClass Error Excuting: " + sp + " UPDATE " + sqlDr.GetString(1));
                            }
                        }
                        return errorCode;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("CARSReportClass Error Excuting: " + sp + " UPDATE ");
                var emailBody = "CARSReportClass Error Excuting: " + sp + " UPDATE <br />" + ex.ToString();
                MailSendHelper.SendingErrorEmail("donotreply@tshore.com", "jbrennan@tshore.com", emailBody);
                return id;
            }
        }

        public static int DeleteReport(int id)
        {
            string sp = "SP_CARS_2019_Master";
            int errorCode = 1; // set errorcode to fail until we succeed ;)
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(repository.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sp, sqlConn))
                    {
                        cmd.Parameters.Add("@TranType", SqlDbType.VarChar).Value = "DeleteReport";
                        cmd.Parameters.Add("@reportID", SqlDbType.VarChar).Value = id;

                        cmd.CommandType = CommandType.StoredProcedure;
                        sqlConn.Open();
                        SqlDataReader sqlDr = cmd.ExecuteReader();
                        if (sqlDr.HasRows)
                        {
                            sqlDr.Read();
                            if (sqlDr.GetInt32(0) == 0)
                            {
                                // Successs!
                                errorCode = 0;
                                
                            }
                            else
                            {
                                // Fail :(
                                Console.WriteLine("CARSReportClass Error Excuting: " + sp + " DELETE " + sqlDr.GetString(1));
                            }
                        }
                        return errorCode;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("CARSReportClass Error Excuting: " + sp + " DELETE ");
                var emailBody = "CARSReportClass Error Excuting: " + sp + " DELETE <br />" + ex.ToString();
                MailSendHelper.SendingErrorEmail("donotreply@tshore.com", "jbrennan@tshore.com", emailBody);
                return errorCode;
            }
}