﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using Dapper;
using static Dapper.SqlMapper;

namespace CARS2019.Models
{
    using System.Configuration;
    using System.Data;

    public class TSProd
    {

        public static string SPDebug = ConfigurationManager.AppSettings["SPDebug"];
        
        public static SqlConnection repository
        {
            get
            {
                return new SqlConnection(Settings.TSProdConnectionString);
            }
            set { }
        }

        public static string GetDepartmentNameFromID(int deptID)
        {
            string departmentName = "";

            try
            {
                departmentName = repository.Query<string>(@"exec tsprod.dbo."+ SPDebug +" @TranType='GetDepartmentNameFromID', @DepartmentID=" + deptID, departmentName).First();
            }
            catch { return null; }

            return departmentName;
        }

        public static string GetCSRandSalesEmailStringFromJobNumber(string jobNumber)
        {
            string emails = "";

            try
            {
                emails = repository.Query<string>(@"exec tsprod.dbo." + SPDebug + " @TranType='GetCSRandSalesEmailStringFromJobNumber', @JobNumber=" + jobNumber, emails).First();
            }
            catch { return emails; }

            return emails;
        }

        public static List<Reports> GetAllReports()
        {
            var report = new Reports();
            var reportList = new List<Reports>();
            reportList = repository.Query<Reports>(@"exec tsprod.dbo." + SPDebug + " @TranType='GetAllReports'", report).ToList();
            repository.Dispose();
            return reportList;
        }

        // added this method 3-21-19 jb
        public static List<Reports> GetAllReports(int reportStatus, string reworkType)
        {
            
            var rework = reworkType ?? "TR";
            var report = new Reports();
            var reportList = new List<Reports>();
            reportList = repository.Query<Reports>(@"exec tsprod.dbo." + SPDebug + " @TranType='GetAllReports', @reportStatus=" + reportStatus + ", @reworkType=" + rework, report).ToList();
            repository.Dispose();
            return reportList;
        }


        // added this method 3-21-19 jb
        public static List<Reports> GetAllReports(string reworkType)
        {
            var rework = reworkType ?? "TR";
            var report = new Reports();
            var reportList = new List<Reports>();
            reportList = repository.Query<Reports>(@"exec tsprod.dbo." + SPDebug + " @TranType='GetAllReports', @reworkType=" + rework, report).ToList();
            repository.Dispose();
            return reportList;
        }

        // this method added 3-14-19 jb
        public static List<Reports> GetAllReports(int reportStatus)
        {
            var report = new Reports();
            var reportList = new List<Reports>();
            reportList = repository.Query<Reports>(@"exec tsprod.dbo."+ SPDebug +" @TranType='GetAllReports', @reportStatus=" + reportStatus , report).ToList();
            repository.Dispose();
            return reportList;
        }

        public static List<CARSDepartmentList> CARSDepartmentList()
        {
            var department= new CARSDepartmentList();
            var departmentList = new List<CARSDepartmentList>();
            departmentList = repository.Query<CARSDepartmentList>(@"exec tsprod.dbo."+ SPDebug +" @TranType='GetAllDepartments'", department).ToList();
            repository.Dispose();
            return departmentList;
        }

        public static List<CARSDepartmentList> CARSDepartmentList(string reworkType)
        {
            var department = new CARSDepartmentList();
            var departmentList = new List<CARSDepartmentList>();
            departmentList = repository.Query<CARSDepartmentList>(@"exec tsprod.dbo." + SPDebug + " @TranType='GetAllDepartments', @reworkType='" + reworkType + "'", department).ToList();
            repository.Dispose();
            return departmentList;
        }

        //public static List<CARSProblemList> CARSDepartmentProblemList()
        //{
        //    var prob = new CARSProblemList();
        //    var probList = new List<CARSProblemList>();
        //    probList.Insert(0, (new CARSProblemList { ProblemID = -1, ProblemDescription = "You must select a Deparment First" }));

        //}

        public static Reports ReportGivenID(int id)
        {
            return repository.Query<Reports>(@"exec tsprod.dbo."+ SPDebug +" @TranType='GetReportGivenID', @reportID=" + id).SingleOrDefault();
  
        }

        public static List<Lists> GetDepartmentProblemsList(string deptId)
        {
            var list = new Lists();

            if (deptId == "-1")
            {
                var problems = new List<Lists>();
                problems.Insert(0, (new Lists { ProblemID = "-1", ProblemDescription = "You must select a Deparment First" }));
                // selectList.Insert(0, (new CARSDepartmentList { id = -1, departmentName = "Select Department" }));
                //List<Lists> problems = 
                return problems;
            } else
            {
                List<Lists> problems = repository.Query<Lists>(@"exec tsprod.dbo." + SPDebug + " @TranType='GetProblemListGivenDepartmentID' , @DepartmentID='" + deptId + "'", list).ToList();
                repository.Close();
                return problems;
            }
            

        }

        public static List<CARSJobDetails> GetCARSJobDetails(string jobNumber)
        {
            var details = new CARSJobDetails();
            List<CARSJobDetails> jobDetails = repository.Query<CARSJobDetails>(@"exec tsprod.dbo."+ SPDebug +" @TranType='GetJobInfoFromJobNumber' , @JobNumber=" + jobNumber, details).ToList();
            repository.Close();
            return jobDetails;
        }


        public static int InsertCARSReport(
            string JobNumber
            , string reportingEmployee
            , string DepartmentID // Declare as INT when we change back to using the foreign key ****************************************************************
            , string reworkEmployee
            , int expectedQuantity
            , string component
            , string problemID // Declare as INT when we change back to using the foreign key ****************************************************************
            , string severityID // Declare as INT when we change back to using the foreign key ****************************************************************
            , decimal calculatedCost
            , string throwOutInitials
            //, DateTime? throwOutDate
            , string notes
            , string correctiveAction
            , string pages
            , string pressSections
            , string proofsRequired
            , string reworkCompleteLocation
            , string SOMaterials
            , string reworkProcess
            , string reworkType
            , string vendor
            , string reportStatus

            )
        {
            string sp = SPDebug;
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
                        cmd.Parameters.Add("@expectedQuantity", SqlDbType.Int).Value = expectedQuantity;
                        cmd.Parameters.Add("@component", SqlDbType.VarChar).Value = component;
                        cmd.Parameters.Add("@problemID", SqlDbType.VarChar).Value = problemID; // replace with Int when we fix the foreign key issue ******************************
                        cmd.Parameters.Add("@severityID", SqlDbType.VarChar).Value = severityID; // replace with Int when we fix the foreign key issue ******************************
                        cmd.Parameters.Add("@calculatedCost", SqlDbType.Decimal).Value = calculatedCost;
                        cmd.Parameters.Add("@throwOutInitials", SqlDbType.VarChar).Value = throwOutInitials;
                        //cmd.Parameters.Add("@throwOutDate", SqlDbType.DateTime).Value = throwOutDate;
                        cmd.Parameters.Add("@notes", SqlDbType.VarChar).Value = notes;
                        cmd.Parameters.Add("@correctiveAction", SqlDbType.VarChar).Value = correctiveAction;
                        cmd.Parameters.Add("@pages", SqlDbType.VarChar).Value = pages;
                        cmd.Parameters.Add("@pressSections", SqlDbType.VarChar).Value = pressSections;
                        cmd.Parameters.Add("@proofsRequired", SqlDbType.VarChar).Value = proofsRequired;
                        cmd.Parameters.Add("@reworkCompleteLocation", SqlDbType.VarChar).Value = reworkCompleteLocation;
                        cmd.Parameters.Add("@SOMaterials", SqlDbType.VarChar).Value = SOMaterials;
                        cmd.Parameters.Add("@reworkProcess", SqlDbType.VarChar).Value = reworkProcess; // added 3-21-19 jb
                        cmd.Parameters.Add("@reworkType", SqlDbType.VarChar).Value = reworkType;  // added 3-21-19 jb
                        cmd.Parameters.Add("@vendor", SqlDbType.VarChar).Value = vendor;  // added 3-21-19 jb
                        cmd.Parameters.Add("@reportStatus", SqlDbType.VarChar).Value = reportStatus;  // added 3-21-19 jb

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
            , int expectedQuantity
            , string component
            , string problemID // Declare as INT when we change back to using the foreign key ****************************************************************
            , string severityID // Declare as INT when we change back to using the foreign key ****************************************************************
            , decimal calculatedCost
            , string throwOutInitials
            //, DateTime? throwOutDate
            , string notes
            , string correctiveAction
            , string pages
            , string pressSections
            , string proofsRequired
            , string reworkCompleteLocation
            , string SOMaterials
            , string reworkProcess
            , string reportStatus  // added 3-21-19 jb
            , string reworkType  // added 3-21-19 jb
            , string vendor  // added 3-21-19 jb
            )
        {
            string sp = SPDebug;
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
                        cmd.Parameters.Add("@expectedQuantity", SqlDbType.Int).Value = expectedQuantity;
                        cmd.Parameters.Add("@component", SqlDbType.VarChar).Value = component;
                        cmd.Parameters.Add("@problemID", SqlDbType.VarChar).Value = problemID; // replace with Int when we fix the foreign key issue ******************************
                        cmd.Parameters.Add("@severityID", SqlDbType.VarChar).Value = severityID; // replace with Int when we fix the foreign key issue ******************************
                        cmd.Parameters.Add("@calculatedCost", SqlDbType.Decimal).Value = calculatedCost;
                        cmd.Parameters.Add("@throwOutInitials", SqlDbType.VarChar).Value = throwOutInitials;
                        //cmd.Parameters.Add("@throwOutDate", SqlDbType.DateTime).Value = throwOutDate;
                        cmd.Parameters.Add("@notes", SqlDbType.VarChar).Value = notes;
                        cmd.Parameters.Add("@correctiveAction", SqlDbType.VarChar).Value = correctiveAction;
                        cmd.Parameters.Add("@pages", SqlDbType.VarChar).Value = pages;
                        cmd.Parameters.Add("@pressSections", SqlDbType.VarChar).Value = pressSections;
                        cmd.Parameters.Add("@proofsRequired", SqlDbType.VarChar).Value = proofsRequired;
                        cmd.Parameters.Add("@reworkCompleteLocation", SqlDbType.VarChar).Value = reworkCompleteLocation;
                        cmd.Parameters.Add("@SOMaterials", SqlDbType.VarChar).Value = SOMaterials;
                        cmd.Parameters.Add("@reworkProcess", SqlDbType.VarChar).Value = reworkProcess;
                        cmd.Parameters.Add("@reportStatus", SqlDbType.VarChar).Value = reportStatus;  // added 3-21-19 jb
                        cmd.Parameters.Add("@reworkType", SqlDbType.VarChar).Value = reworkType;  // added 3-21-19 jb
                        cmd.Parameters.Add("@vendor", SqlDbType.VarChar).Value = vendor;  // added 3-21-19 jb
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
            string sp = SPDebug;
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

        public static int ChangeStatus(int id, int status)
        {
            string sp = SPDebug;
            string strStatus = "ERROR";
            switch (status)
            {
                case -1:
                    strStatus = "DELETED";
                    break;
                case 0:
                    strStatus = "ACTIVE";
                    break;
                case 1:
                    strStatus = "COMPLETE";
                    break;
                default:
                    strStatus = "ERROR - INVALID STATUS CODE";
                    break;
            }

            int errorCode = 1; // set errorcode to fail until we succeed ;)
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(repository.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sp, sqlConn))
                    {
                        cmd.Parameters.Add("@TranType", SqlDbType.VarChar).Value = "ChangeStatus";
                        cmd.Parameters.Add("@reportID", SqlDbType.VarChar).Value = id;
                        cmd.Parameters.Add("@reportStatus", SqlDbType.VarChar).Value = status;

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
                                //var emailBody = "CARSReportClass Report Status Updated to : " + strStatus + "<br /> For Report ID:" + id.ToString();
                                //MailSendHelper.SendingErrorEmail("donotreply@tshore.com", "jbrennan@tshore.com", emailBody);
                            }
                            else
                            {
                                // Fail :(
                                Console.WriteLine("CARSReportClass Error Excuting: " + sp + " CHANGE STATUS FAILED " + sqlDr.GetString(1));
                                var emailBody = "CARSReportClass Report Status Failed : " + strStatus + "<br /> For Report ID:" + id.ToString();
                                MailSendHelper.SendingErrorEmail("donotreply@tshore.com", "jbrennan@tshore.com", emailBody);
                            }
                        }
                        return errorCode;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("CARSReportClass Error Excuting: " + sp + " CHANGE STATUS ");
                var emailBody = "CARSReportClass Error Excuting: " + sp + " CHANGE STATUS <br />" + ex.ToString();
                MailSendHelper.SendingErrorEmail("donotreply@tshore.com", "jbrennan@tshore.com", emailBody);
                return errorCode;
            }
        }


        public static List<DepartmentCheck> GetCARSDepartmentCheck()
        {
            var departmentChex = new DepartmentCheck();
            List<DepartmentCheck> CARSdepartmentChex = repository.Query<DepartmentCheck>(@"exec tsprod.dbo."+ SPDebug +" @TranType='GetAllDepartmentChecks'", departmentChex).ToList();
            repository.Close();
            return CARSdepartmentChex;
        }

        public static List<DepartmentCheck> GetChecksGivenReportID(int departmentID)
        {
            var departmentChex = new DepartmentCheck();
            List<DepartmentCheck> CARSdepartmentChex = repository.Query<DepartmentCheck>(@"exec tsprod.dbo."+ SPDebug +" @TranType='GetChecksGivenReportID', @reportID=" + departmentID, departmentChex).ToList();
            repository.Close();
            return CARSdepartmentChex;
        }


        public static string GetDepartmentEmail(int deptID)
        {
            string departmentEmail = "donotreply@tshore.com";

            try
            {
                departmentEmail = repository.Query<string>(@"exec tsprod.dbo." + SPDebug + " @TranType='GetDepartmentEmail', @DepartmentID=" + deptID, departmentEmail).First();
            }
            catch { return departmentEmail.Trim(); }

            return departmentEmail.Trim();
        }

        public static void InsertDeparmentCheck(
            int reportID
            , int departmentID)
        {
            string sql = "exec tsprod.dbo."+ SPDebug +" @TranType='SaveObject'";
            sql += Settings.addIntParam("reportID", reportID, 0);
            sql += Settings.addIntParam("departmentID", departmentID, 0);
            try
            {
                SqlConnection sqlConn = new SqlConnection(repository.ConnectionString);
                sqlConn.Open();
                SqlCommand sqlComm = new SqlCommand(sql, sqlConn);
                SqlDataReader sqlDr = sqlComm.ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                var emailBody = "CARSReportClass Error Excuting: " + sql + " InsertDeparmentCheck <br />" + ex.ToString();
                MailSendHelper.SendingErrorEmail("donotreply@tshore.com", "jbrennan@tshore.com", emailBody);
            }
        }

       

        public static void UpdateDeparmentCheck(
            int id
            , string operatorName = null
            , int? quantity = null
            , DateTime? completedDate = null)
        {
            string sql = "exec tsprod.dbo."+ SPDebug +" @TranType='SaveObject'";
            sql += Settings.addIntParam("id", id, 0);
            if (operatorName != null)
            {
                sql += Settings.addParam("operatorName", operatorName);
            }            
            if (quantity != null)
            {
                sql += Settings.addIntParam("quantity", quantity??0, 0);
            }
            if (completedDate != null)
            {
                sql += ", @completedDate='" + completedDate +"'";
            }

            try
            {
                SqlConnection sqlConn = new SqlConnection(repository.ConnectionString);
                sqlConn.Open();
                SqlCommand sqlComm = new SqlCommand(sql, sqlConn);
                SqlDataReader sqlDr = sqlComm.ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                var emailBody = "CARSReportClass Error Excuting: " + sql + " UpdateDeparmentCheck <br />" + ex.ToString();
                MailSendHelper.SendingErrorEmail("donotreply@tshore.com", "jbrennan@tshore.com", emailBody);
            }
        }



        public static void DeleteCheckGivenId(int reportid, int departmentID)
        {
            repository.Query<Reports>(@"exec tsprod.dbo." + SPDebug + " @TranType='DeleteCheckGivenId', @reportid=" + reportid + ", @departmentID=" + departmentID).SingleOrDefault();

        }

    }



    public partial class Reports
    {
        public int id { get; set; }
        [Required]
        [Display(Name = "Reported By")]
        public string reporting_employee { get; set; }
        [Display(Name = "Job")]
        public string job_ID { get; set; }
        [Required]
        [Display(Name = "Dept")]
        public string department_ID { get; set; }
        [Required]
        [Display(Name = "Problem")]
        public string problem_ID { get; set; }

        [Display(Name = "Severity")]
        public string severity_id { get; set; }
        [Display(Name = "Employee")]
        public string rework_employee { get; set; }
        [Required]
        [Display(Name = "Expected Quantity")]
        public int expectedQuantity { get; set; }
        [Display(Name = "Component")]
        public string component { get; set; }
        [Display(Name = "Throw Out Initials / Date")]
        public string throwOutInitials { get; set; }
        //[Display(Name = "Throw Out Date")]
        //public DateTime? throwOutDate { get; set; }

        [Display(Name = "Cost")]
        public decimal? calculated_cost { get; set; }
        [Display(Name = "Rework Description")]
        public string notes { get; set; }
        [Display(Name = "Process Improvements")]
        public string corrective_action { get; set; }

        [Display(Name = "Pages")]
        public string pages { get; set; }
        [Display(Name = "Press Sections")]
        public string pressSections { get; set; }
        [Display(Name = "Proofs Required")]
        public string proofsRequired { get; set; }
        [Display(Name = "Rework Complete Location")]
        public string reworkCompleteLocation { get; set; }
        [Display(Name = "S.O. Materials")]
        public string SOMaterials { get; set; }
        [Display(Name = "Rework Process")]
        public string reworkProcess { get; set; }
        [Display(Name = "Status")]
        public string reportStatus { get; set; }
        [Display(Name = "Rework Type")]
        public string reworkType { get; set; }
        [Display(Name = "Vendor")]
        public string vendor { get; set; }


        public System.DateTime created_Date { get; set; }
        public DateTime? week {
            get
            {
                //System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
                //int weekNum = cul.Calendar.GetWeekOfYear(
                //    created_Date,
                //    System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                //    DayOfWeek.Monday);
                DateTime datetime = new DateTime();
                if (created_Date == DateTime.MinValue)
                {
                    created_Date = datetime;
                }
                var weekEnding = DateTimeExtensions.LastDayOfWeek(created_Date);
                return weekEnding;
            }
        }
    }

    public partial class CARSDepartmentList
    {
        public int id { get; set; }
        public string departmentName { get; set; }
    }


    public class DepartmentCheck
    {
        public int id { get; set; }
        public int reportID { get; set; }
        [Display(Name = "DeptID")]
        public int departmentID { get; set; }
        [Display(Name = "Dept Name")]
        public string departmentName { get; set; }
        [Display(Name = "Employee")]
        public string operatorName { get; set; }
        public string quantity { get; set; }
        [Display(Name = "Date Completed")]
        public string completedDate { get; set; }

    }

    public class SpExecuteResults
    {
        public int ErrNo { get; set; }
        public string ErrText { get; set; }
        int id { get; set; }
    }

    public class CheckedDepartments
    {
        public string deptID { get; set; }
    }

    public static partial class DateTimeExtensions
    {
        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;
            if (diff < 0)
                diff += 7;
            return dt.AddDays(-diff).Date;
        }

        public static DateTime LastDayOfWeek(this DateTime dt)
        {
            return dt.FirstDayOfWeek().AddDays(6);
        }

        public static DateTime FirstDayOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime dt)
        {
            return dt.FirstDayOfMonth().AddMonths(1).AddDays(-1);
        }

        public static DateTime FirstDayOfNextMonth(this DateTime dt)
        {
            return dt.FirstDayOfMonth().AddMonths(1);
        }
    }

    public class Settings
    {
        //public static string Gams1ConnectionString = "Data Source=SQL2016\\SQL2016;Initial Catalog=gams1;User ID=db2_readonly;Password=HatCommandMortals;";
        //public static string HiFlexJobsConnectionString = "Data Source=SQL2016\\SQL2016;Initial Catalog=HiFlexJobs;User ID=db2_readonly;Password=HatCommandMortals;";
        //public static string TSConnectionString = "Data Source=SQL2016\\SQL2016;Initial Catalog=TS;User ID=db2_readonly;Password=HatCommandMortals;";

        public static string Gams1ConnectionString = "Data Source=SQL2016\\SQL2016;Initial Catalog=gams1;User ID=demo;Password=demo;";
        public static string HiFlexJobsConnectionString = "Data Source=SQL2016\\SQL2016;Initial Catalog=HiFlexJobs;User ID=demo;Password=demo;";
        public static string TSConnectionString = "Data Source=SQL2016\\SQL2016;Initial Catalog=TS;User ID=demo;Password=demo;";
        public static string TSProdConnectionString = "Data Source=SQL2016\\SQL2016;Initial Catalog=TSProd;User ID=demo;Password=demo;";


        public static string addParam(string param, string value)
        {
            string result = "";
            if (!String.IsNullOrEmpty(value))
            {
                result = ", @" + param + "='" + value.Replace("'", "''") + "'";
            }
            return result;
        }

        public static string addIntParam(string param, int value, int? AllowZero)
        {
            AllowZero = AllowZero ?? 0;
            string result = "";
            if (value > 0 || AllowZero == 1)
            {
                result = ", @" + param + "=" + value;
            }
            return result;
        }
    }

}