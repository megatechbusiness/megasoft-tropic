using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class TestLargeDataController : Controller
    {
        //
        // GET: /TestLargeData/

        public ActionResult Index()
        {
            //WarehouseManagementEntities db = new WarehouseManagementEntities("");
            //var result = db.sp_InvMaster().ToList();
            return View();
        }

        public string GetData()
        {
            try
            {
                WarehouseManagementEntities db = new WarehouseManagementEntities("");
                
                //return Json(result, JsonRequestBehavior.AllowGet);
                string  searchKey, orderDir;
                int limit = Request.QueryString["length"] == null ? 0 : Convert.ToInt16(Request.QueryString["length"]);
                int start = Request.QueryString["start"] == null ? 0 : Convert.ToInt16(Request.QueryString["start"]);
                searchKey = Request.QueryString["search[value]"] == null ? "" : Request.QueryString["search[value]"].ToString();
                int orderColumn = Request.QueryString["order[0][column]"] == null ? 0 : Convert.ToInt16(Request.QueryString["order[0][column]"]);
                orderDir = Request.QueryString["order[0][dir]"] == null ? "" : Request.QueryString["order[0][dir]"].ToString();
                string draw = Request.QueryString["draw"] == null ? "" : Request.QueryString["draw"].ToString();

                var result = db.sp_InvMasterList((int)orderColumn, (int)limit, orderDir, (int)start, searchKey).ToList();

                //int TotalCount = result.FirstOrDefault().

                dynamic newtonresult = new
                {
                //    status = "success",
                //    draw = Convert.ToInt32(draw == "" ? "0" : draw),
                //    recordsTotal = result.Count(),
                //    recordsFiltered = result.Count(),
                //    data = result

                    status = "success",
                    draw = Convert.ToInt32(draw == "" ? "0" : draw),
                    recordsTotal = result.FirstOrDefault().NoOfRows,
                    recordsFiltered = result.FirstOrDefault().NoOfRows,
                    data = result
                };
                string jsonString = JsonConvert.SerializeObject(newtonresult);

                return jsonString;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //
        // GET: /TestLargeData/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /TestLargeData/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /TestLargeData/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /TestLargeData/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /TestLargeData/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /TestLargeData/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /TestLargeData/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
