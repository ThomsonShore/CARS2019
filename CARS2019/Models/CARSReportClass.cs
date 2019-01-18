using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Repositories.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using Dapper;

namespace CARS2019.Models
{
    using global::Models.Models;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    public partial class TSProdEntities : DbContext
    {
        public TSProdEntities()
            : base("name=TSProdEntities")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }

        public virtual DbSet<Reports> Reports { get; set; }
    }

    public class TSProd
    {

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
                departmentName = repository.Query<string>(@"exec tsprod.dbo.SP_CARS_2019_Master @TranType='GetDepartmentNameFromID', @DepartmentID=" + deptID, departmentName).First();
            }
            catch { return null; }

            return departmentName;
        }

        public static List<Reports> Reports()
        {
            var report = new Reports();
            var reportList = new List<Reports>();
            reportList = repository.Query<Reports>(@"exec tsprod.dbo.SP_CARS_2019_Master @TranType='GetAllReports'", report).ToList();
            repository.Dispose();
            return reportList;
        }

        public static List<Lists> GetDepartmentProblemsList(string deptId)
        {
            var list = new Lists();
            List<Lists> problems = repository.Query<Lists>(@"exec tsprod.dbo.SP_CARS_2019_Master @TranType='GetProblemListGivenDepartmentID' , @DepartmentID='" + deptId + "'", list).ToList();
            repository.Close();
            return problems;
        }

        public static List<CARSJobDetails> GetCARSJobDetails(string jobNumber)
        {
            var details = new CARSJobDetails();
            List<CARSJobDetails> jobDetails = repository.Query<CARSJobDetails>(@"exec tsprod.dbo.SP_CARS_2019_Master @TranType='GetJobInfoFromJobNumber' , @JobNumber=" + jobNumber, details).ToList();
            repository.Close();
            return jobDetails;
        }

        public static List<DepartmentCheck> GetCARSDepartmentCheck()
        {
            var departmentChex = new DepartmentCheck();
            List<DepartmentCheck> CARSdepartmentChex = repository.Query<DepartmentCheck>(@"exec tsprod.dbo.SP_CARS_2019_Master @TranType='GetAllDepartmentChecks'", departmentChex).ToList();
            repository.Close();
            return CARSdepartmentChex;
        }

        public static List<DepartmentCheck> GetChecksGivenReportID(int departmentID)
        {
            var departmentChex = new DepartmentCheck();
            List<DepartmentCheck> CARSdepartmentChex = repository.Query<DepartmentCheck>(@"exec tsprod.dbo.SP_CARS_2019_Master @TranType='GetChecksGivenReportID', @reportID=" + departmentID, departmentChex).ToList();
            repository.Close();
            return CARSdepartmentChex;
        }

        public static void InsertDeparmentCheck(
            int reportID
            , int departmentID)
        {
            string sql = "exec tsprod.dbo.SP_CARS_2019_Master @TranType='SaveObject'";
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
            }
        }

        public static void UpdateDeparmentCheck(
            int id
            , string operatorName = null
            , int? quantity = null
            , DateTime? completedDate = null)
        {
            string sql = "exec tsprod.dbo.SP_CARS_2019_Master @TranType='SaveObject'";
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
            }
        }

    }



    public partial class Reports
    {
        public int id { get; set; }
        [Display(Name = "Reported By")]
        public string reporting_employee { get; set; }
        [Display(Name = "Job")]
        public string job_ID { get; set; }
        [Display(Name = "Dept")]
        public string department_ID { get; set; }
        [Display(Name = "Problem")]
        public string problem_ID { get; set; }
        [Display(Name = "Severity")]
        public string severity_id { get; set; }
        [Display(Name = "Employee")]
        public string rework_employee { get; set; }
        [Display(Name = "Cost")]
        public decimal? calculated_cost { get; set; }
        [Display(Name = "Rework Description")]
        public string notes { get; set; }
        [Display(Name = "Process Needs")]
        public string corrective_action { get; set; }
        public System.DateTime created_Date { get; set; }
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

    public class CheckedDepartments
    {
        public string deptID { get; set; }
    }
}