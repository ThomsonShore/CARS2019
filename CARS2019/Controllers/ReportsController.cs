using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CARS2019.Models;

using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Web.Script.Serialization;

namespace CARS2019.Controllers
{
    public class ReportsController : Controller
    {

        private TSProd dbTS = new TSProd();

        private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";


        public List<string> checkedDepartments = new List<string>();

        
        [Authorize]
        [SessionExpire]
        public ActionResult Index(int reportStatus, string reworkType)
        {
            int count = 12; // crufty hack to set the default sort column number (Date Created), since we optionally hide / show columns, update this as needed.

            if (reworkType != "TR")
            {
                count = count - 3; // remove columns since we do not display Employee, Severity, or Cost for VR and CR.
            }

            if (reworkType == "VR")
            {
                count = count + 1;  // adding a column back for Vendor for VR
            }

            if (Session["canSeeCorrectiveAction"] != null)
            {
                if (Session["canSeeCorrectiveAction"].ToString() == "False")
                {
                    count = count - 1;
                }
            }

            if (Session["canSeeEmployee"] != null)
            {
                if (Session["canSeeEmployee"].ToString() == "False")
                {
                    count = count - 1;
                }
            }
            ViewBag.reportStatus = reportStatus;
            ViewBag.reworkType = reworkType;
            ViewBag.dateSortColumn = count.ToString();
            ViewBag.weekSortColumn = (count + 1).ToString();
            //ViewData["CARSReport"] = dbTS;
            List<Reports> reportView = CARS2019.Models.TSProd.GetAllReports(reportStatus, reworkType);
            return View(reportView);
        }


        // generate excel file from report index
        [Authorize]
        [SessionExpire]
        public ActionResult Export(int reportStatus, string reworkType)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("CARS");                
                worksheet.Cells["A1"].LoadFromCollection(CARS2019.Models.TSProd.GetAllReports(reportStatus, reworkType).ToList(), true, OfficeOpenXml.Table.TableStyles.Medium1);
                var colCnt = worksheet.Dimension.End.Column;
                worksheet.Column(colCnt).Style.Numberformat.Format = "mm-dd-yyyy h:mm";
                worksheet.Cells.AutoFitColumns();
                return File(package.GetAsByteArray(), XlsxContentType, "CARSReport.xlsx");
            }
        }



        // GET: Reports/Details/5
        [Authorize]
        [SessionExpire]
        public ActionResult Details(int id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            var reports = new Reports();
            reports = CARS2019.Models.TSProd.ReportGivenID(id);

            if (reports == null)
            {
                return HttpNotFound();
            }
            var checkedDepartmentList = TSProd.GetChecksGivenReportID(id);
            if (checkedDepartmentList != null)
            {
                ViewData["checkedDepartmentList"] = checkedDepartmentList;
            }

            return View(reports);
        }

        // GET: Reports/Create
        [Authorize]
        [SessionExpire]
        public ActionResult Create(int reportStatus, string reworkType)
        {
            var items = CARS2019.Models.TSProd.CARSDepartmentList().ToList();
            ViewBag.reportStatus = reportStatus;
            ViewBag.reworkType = reworkType;
            //CARS2019.Models.TSProd.Reports

            if (items != null)
            {
                ViewBag.data = items;
            }

            return View();
        }

        // POST: Reports/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,reporting_employee,job_ID,department_ID,component,problem_ID,severity_id," +
            "rework_employee,expectedQuantity,calculated_cost,throwOutInitials,notes,corrective_action," +
            "created_Date,pages,pressSections,proofsRequired,reworkCompleteLocation,SOMaterials,reworkProcess,reworkType,vendor,reportStatus")] Reports reports)
        {
            if (ModelState.IsValid)
            {

                if (reports.calculated_cost == null)
                {
                    reports.calculated_cost = 0;
                }

                int insertResults = TSProd.InsertCARSReport(
                    reports.job_ID
                    , reports.reporting_employee
                    , reports.department_ID
                    , reports.rework_employee
                    , reports.expectedQuantity
                    , reports.component
                    , reports.problem_ID
                    , reports.severity_id
                    , (reports.calculated_cost ?? 0)
                    , reports.throwOutInitials
                    //, reports.throwOutDate
                    , reports.notes
                    , reports.corrective_action
                    , reports.pages
                    , reports.pressSections
                    , reports.proofsRequired
                    , reports.reworkCompleteLocation
                    , reports.SOMaterials
                    , reports.reworkProcess // added 3-21-19 jb
                    , reports.reworkType // added 3-21-19 jb
                    , reports.vendor // added 3-21-19 jb
                    , reports.reportStatus // added 3-21-19 jb
                    );

                if (insertResults > 0) // Successfully inserted report
                {
                    //db.Reports.Add(reports);
                    //db.SaveChanges();
                    //db.Entry(reports).GetDatabaseValues();

                    string departmentEmailList = "robinf@tshore.com; carlt@tshore.com";

                    departmentEmailList += ";" + TSProd.GetCSRandSalesEmailStringFromJobNumber(reports.job_ID);

                    if (TempData["tempChecked"] != null)
                    {
                        foreach (var dept in (IEnumerable<String>)TempData["tempChecked"])
                        {
                            TSProd.InsertDeparmentCheck(insertResults, Int32.Parse(dept.ToString()));
                            string departmentEmail = TSProd.GetDepartmentEmail(Int32.Parse(dept));
                            departmentEmailList += ";" + departmentEmail;
                        }

                    }

                    var targetURL = "https://cars.tshore.com/Reports/Details/" + insertResults;
                    var emailBody = "Issue submitted for job number: " + reports.job_ID + "<br />";
                    MailSendHelper.SendingDepartmentEmail(reports.reporting_employee, departmentEmailList, emailBody, targetURL, reports.job_ID);


                    return RedirectToAction("Index", new { reports.reportStatus, reports.reworkType });
                }


            }

            return View(reports);
        }

        // GET: Reports/Edit/5
        [Authorize]
        [SessionExpire]
        public ActionResult Edit(int id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Session["canEditEntry"] != null)
            {
                if (Session["canEditEntry"].ToString() == "False")
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            //Reports reports = db.Reports.Find(id);
            var reports = new Reports();
            reports = CARS2019.Models.TSProd.ReportGivenID(id);
            if (reports == null)
            {
                return HttpNotFound();
            }

            var items = CARS2019.Models.TSProd.CARSDepartmentList().ToList();
            if (items != null)
            {
                ViewBag.data = items;
            }

            var checkedDepartmentList = TSProd.GetChecksGivenReportID(id);
            if (checkedDepartmentList != null)
            {
                ViewData["checkedDepartmentList"] = checkedDepartmentList;
            }

            return View(reports);
        }

        // POST: Reports/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,reporting_employee,job_ID,department_ID,component,problem_ID,severity_id,rework_employee," +
            "expectedQuantity,calculated_cost,throwOutInitials,notes,corrective_action,created_Date,pages,pressSections," +
            "proofsRequired,reworkCompleteLocation,SOMaterials,reworkProcess,reportStatus,reworkType,vendor")] Reports reports)
        {
            if (ModelState.IsValid)
            {
                if (Session["canEditEntry"] != null)
                {
                    if (Session["canEditEntry"].ToString() == "False")
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }

                int insertResults = TSProd.UpdateCARSReport(
                     reports.id
                    , reports.job_ID
                    , reports.reporting_employee
                    , reports.department_ID
                    , reports.rework_employee
                    , reports.expectedQuantity
                    , reports.component
                    , reports.problem_ID
                    , reports.severity_id
                    , (reports.calculated_cost ?? 0)
                    , reports.throwOutInitials
                    //, reports.throwOutDate
                    , reports.notes
                    , reports.corrective_action
                    , reports.pages
                    , reports.pressSections
                    , reports.proofsRequired
                    , reports.reworkCompleteLocation
                    , reports.SOMaterials
                    , reports.reworkProcess 
                    , reports.reportStatus // added 3-21-19 jb
                    , reports.reworkType // added 3-21-19 jb
                    , reports.vendor // added 3-21-19 jb
                    );

                if (insertResults == 0) // Successfully inserted report
                {
                    if (TempData["tempChecked"] != null)
                    {
                        foreach (var dept in (IEnumerable<String>)TempData["tempChecked"])
                        {
                            TSProd.InsertDeparmentCheck(reports.id, Int32.Parse(dept.ToString()));
                        }
                    }
                    return RedirectToAction("Index", new { reports.reportStatus, reports.reworkType });
                }

                //db.Entry(reports).State = EntityState.Modified;
                //db.SaveChanges();



            }
            return View(reports);
        }

        // GET: Reports/ChangeStatus/5
        [Authorize]
        [SessionExpire]
        public ActionResult ChangeStatus(int id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Session["canDeleteEntry"] != null)
            {
                if (Session["canDeleteEntry"].ToString() == "False")
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            var reports = new Reports();
            reports = CARS2019.Models.TSProd.ReportGivenID(id);

            if (reports == null)
            {
                return HttpNotFound();
            }
            var checkedDepartmentList = TSProd.GetChecksGivenReportID(id);
            if (checkedDepartmentList != null)
            {
                ViewData["checkedDepartmentList"] = checkedDepartmentList;
            }

            return View(reports);
        }

        // POST: Reports/ChangeStatus/5
        [Authorize]
        [SessionExpire]
        [HttpPost, ActionName("ChangeStatus")]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeStatus(int id, int reportStatus, string reworkType)
        {
            if (Session["canDeleteEntry"] != null)
            {
                if (Session["canDeleteEntry"].ToString() == "False")
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            int statusResults = TSProd.ChangeStatus(id, reportStatus);

            if (statusResults == 0) // Successfully inserted report
            {

                return RedirectToAction("Index", new { reportStatus, reworkType });
            }

            var checkedDepartmentList = TSProd.GetChecksGivenReportID(id);
            if (checkedDepartmentList != null)
            {
                ViewData["checkedDepartmentList"] = checkedDepartmentList;
            }

            return RedirectToAction("Index", new { reportStatus, reworkType }); // Should go to an error page *************************************************
        }

        // GET: Reports/Delete/5
        [Authorize]
        [SessionExpire]
        public ActionResult Delete(int id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Session["canDeleteEntry"] != null)
            {
                if (Session["canDeleteEntry"].ToString() == "False")
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            var reports = new Reports();
            reports = CARS2019.Models.TSProd.ReportGivenID(id);

            if (reports == null)
            {
                return HttpNotFound();
            }
            var checkedDepartmentList = TSProd.GetChecksGivenReportID(id);
            if (checkedDepartmentList != null)
            {
                ViewData["checkedDepartmentList"] = checkedDepartmentList;
            }

            return View(reports);
        }

        // POST: Reports/Delete/5
        [Authorize]
        [SessionExpire]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["canDeleteEntry"] != null)
            {
                if (Session["canDeleteEntry"].ToString() == "False")
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            int deleteResults = TSProd.DeleteReport(id);

            if (deleteResults == 0) // Successfully inserted report
            {

                return RedirectToAction("Index");
            }

            var checkedDepartmentList = TSProd.GetChecksGivenReportID(id);
            if (checkedDepartmentList != null)
            {
                ViewData["checkedDepartmentList"] = checkedDepartmentList;
            }

            return RedirectToAction("Index"); // Should go to error page *************************************************
        }


        [Authorize]
        [SessionExpire]
        public JsonResult ProblemList(string deptID) //It will be fired from Jquery ajax call  
        {
            if (deptID != "")
            {
                //int intdeptID = Int32.Parse(deptID);
                string strJsonProblems = JsonConvert.SerializeObject(TSProd.GetDepartmentProblemsList(deptID));
                return Json(strJsonProblems, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string strJsonProblems = "[{\"ProblemID\":-1,\"ProblemDescription\":\"ERROR\"}]";
                return Json(strJsonProblems, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [SessionExpire]
        public JsonResult JobDetails(string jobNumber)  
        {
            if (jobNumber != "")
            {
                string jsonJobDetails = JsonConvert.SerializeObject(TSProd.GetCARSJobDetails(jobNumber));
                return Json(jsonJobDetails, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string jsonJobDetails = "[{\"job_id\":\"ERROR\",\"cust_name\":\"ERROR\",\"Title\":\"ERROR\",\"csr_name\":\"ERROR\",\"cust_key\":\"ERROR\"}]";
                return Json(jsonJobDetails, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [SessionExpire]
        public string GetDepartmentNameFromID(int deptID)
        {
            if (deptID > -1)
            {
                string strDepartmentName = TSProd.GetDepartmentNameFromID(deptID);
                return strDepartmentName;
            }
            else
            {
                string strDepartmentName = "ERROR";
                return strDepartmentName;
            }
        }


        public void StoreCheckedDepartments(string[] Parameters)
        {
            if (Parameters != null)
            {
                foreach (var department in Parameters)
                {
                    checkedDepartments.Add(department);
                    TempData["tempChecked"] = checkedDepartments;
                }
            }   
        }

        public void ToggleCheckedDepartment(int reportID, int departmentID)
        {
            TSProd.DeleteCheckGivenId(reportID, departmentID);
        }

        public void StoreDepartmentValues(DepartmentCheck[] departmentJSON)
        {
            if (departmentJSON != null)
            {
                //Deserialize JSON from Edit page

                //JavaScriptSerializer js = new JavaScriptSerializer();
                //DepartmentCheck[] departmentData = js.Deserialize<DepartmentCheck[]>(departmentJSON);
                //DepartmentCheck departmentData = departmentJSON;
                
                // Loop through each row
                // call UpdateDeparmentCheck(pass in id, operatorName, quantity, completedDate)
                foreach (var row in departmentJSON)
                {
                    var id = row.id;
                    var operatorName = row.operatorName;
                    var quantity = row.quantity ?? "0";
                    var completedDate = row.completedDate ?? DateTime.Now.ToString();
                    if (id > 0 && operatorName != null)
                    {
                        TSProd.UpdateDeparmentCheck(id, operatorName, Int32.Parse(quantity), DateTime.Parse(completedDate));
                    }
                    
                }
            }
        }


        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
