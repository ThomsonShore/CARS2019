using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CARS2019.Models;
using Repositories.Repositories;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Web.Script.Serialization;

namespace CARS2019.Controllers
{
    public class ReportsController : Controller
    {
        private TSProdEntities db = new TSProdEntities();
        private TSProd dbTS = new TSProd();
        private TSProdEntities1 ddl = new TSProdEntities1();
        private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";


        public List<string> checkedDepartments = new List<string>();

        // GET: Reports
        [Authorize]
        [SessionExpire]
        public ActionResult Index()
        {
            int count = 9; // crufty hack to set the default sort column number (Date Created), since we optionally hide / show columns, update this as needed.


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

            ViewBag.dateSortColumn = count.ToString();
            //ViewData["CARSReport"] = dbTS;
            return View(db.Reports.ToList());
        }


        // generate excel file from report index
        [Authorize]
        [SessionExpire]
        public ActionResult Export()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("CARS");                
                worksheet.Cells["A1"].LoadFromCollection(db.Reports.ToList(), true, OfficeOpenXml.Table.TableStyles.Medium1);
                var colCnt = worksheet.Dimension.End.Column;
                worksheet.Column(colCnt).Style.Numberformat.Format = "mm-dd-yyyy h:mm";
                worksheet.Cells.AutoFitColumns();
                return File(package.GetAsByteArray(), XlsxContentType, "CARSReport.xlsx");
            }
        }



        // GET: Reports/Details/5
        [Authorize]
        [SessionExpire]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reports reports = db.Reports.Find(id);
            if (reports == null)
            {
                return HttpNotFound();
            }
            var checkedDepartmentList = TSProd.GetChecksGivenReportID(reports.id);
            if (checkedDepartmentList != null)
            {
                ViewData["checkedDepartmentList"] = checkedDepartmentList;
            }

            return View(reports);
        }

        // GET: Reports/Create
        [Authorize]
        [SessionExpire]
        public ActionResult Create()
        {
            var items = ddl.CARSDepartmentLists.ToList();


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
        public ActionResult Create([Bind(Include = "id,reporting_employee,job_ID,department_ID,problem_ID,severity_id,rework_employee,calculated_cost,notes,corrective_action,created_Date")] Reports reports)
        {
            if (ModelState.IsValid)
            {
                if (reports.calculated_cost == null)
                {
                    reports.calculated_cost = 0;
                }
                db.Reports.Add(reports);
                db.SaveChanges();
                db.Entry(reports).GetDatabaseValues();

                //var targetURL = "http://localhost:53080/Reports/Details/" + reports.id; // for dev testing
                var targetURL = "https://cars.tshore.com/Reports/Details/" + reports.id; // for live server, should move to web.config for realzies ***********************
                var emailBody = "Issue submitted for job number: " + reports.job_ID + "<br />";
                MailSendHelper.testSendingEmail("jbrennan@tshore.com", "jbrennan@tshore.com", emailBody, targetURL, reports.job_ID);

                if (TempData["tempChecked"] != null)
                {
                    foreach (var dept in (IEnumerable<String>)TempData["tempChecked"])
                    {
                        TSProd.InsertDeparmentCheck(reports.id, Int32.Parse(dept.ToString()));
                    }
                }

                return RedirectToAction("Index");
            }

            return View(reports);
        }

        // GET: Reports/Edit/5
        [Authorize]
        [SessionExpire]
        public ActionResult Edit(int? id)
        {
            if (id == null)
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

            Reports reports = db.Reports.Find(id);
            if (reports == null)
            {
                return HttpNotFound();
            }

            var items = ddl.CARSDepartmentLists.ToList();
            if (items != null)
            {
                ViewBag.data = items;
            }

            var checkedDepartmentList = TSProd.GetChecksGivenReportID(reports.id);
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
        public ActionResult Edit([Bind(Include = "id,reporting_employee,job_ID,department_ID,problem_ID,severity_id,rework_employee,calculated_cost,notes,corrective_action,created_Date")] Reports reports)
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

                db.Entry(reports).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(reports);
        }

        // GET: Reports/Delete/5
        [Authorize]
        [SessionExpire]
        public ActionResult Delete(int? id)
        {
            if (id == null)
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

            Reports reports = db.Reports.Find(id);
            if (reports == null)
            {
                return HttpNotFound();
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

            Reports reports = db.Reports.Find(id);
            db.Reports.Remove(reports);
            db.SaveChanges();
            return RedirectToAction("Index");
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
                    var quantity = row.quantity;
                    var completedDate = row.completedDate;
                    if (id > 0 && operatorName != null)
                    {
                        TSProd.UpdateDeparmentCheck(id, operatorName, Int32.Parse(quantity), DateTime.Parse(completedDate));
                    }
                    
                }
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
