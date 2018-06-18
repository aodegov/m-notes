using MedNotes.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MedNotes.Controllers
{
    public class NotesController : Controller
    {
        public ActionResult Summary()
        {
           // LogHelper.SaveLogInfo("Med Notes start");
           // LogHelper.SaveLogInfo("Retrieve summary data");
            return View();

        }

        public ActionResult Setup()
        {
            //LogHelper.SaveLogInfo("Opening setup page");
            ViewBag.Message = "Setup page.";

            return View();
        }

        public ActionResult Details(int? note_id)
        {
            try
            {
                //LogHelper.SaveLogInfo(string.Format("Opening Details page with note_id = {0}", note_id));
                ViewBag.NoteId = note_id;
               // throw new Exception("test jsoin exception message");
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: Error occured in NotesController controller in Details Action: \r\n  {0}", ex.Message), ex.StackTrace);
                return Json(new { status = "error", message = ex.Message });
            }
        }
    }
}