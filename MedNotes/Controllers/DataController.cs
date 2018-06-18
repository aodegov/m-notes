using MedNotes.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MedNotes.Controllers
{
    public class DataController : Controller
    {
        //Logger logger = LogManager.GetCurrentClassLogger();

        public JsonResult GetSummaryData()
        {
            try
            {
                // logger.Info("Get summary data...");
                List<SummaryModel> result = DataAccess.GetSummaries();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: Error occured in NotesController controller ClientSummary Action: \r\n  {0}", ex.Message), ex.StackTrace);
                return new JsonResult();
            }
        }

        // GET: Data
        public JsonResult GetICD10Data(string note_id, string query_val)
        {
            try
            {
                int id;
                if (int.TryParse(note_id, out id))
                {
                    //   logger.Info("Get icd 10 data...");
                    List<ICD10Model> result = DataAccess.GetICD10(id, query_val);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                    return new JsonResult();
            }
            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: Error occured in NotesController controller GetICD10Data Action: \r\n  {0}", ex.Message), ex.StackTrace);
                return new JsonResult();
            }
        }

        public JsonResult GetNLPData(string note_id)
        {
            try
            {
                int id;
                if (int.TryParse(note_id, out id))
                {
                    List<NLP_TableModel> result = DataAccess.Get_NLP_Records(id);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                    return new JsonResult();
            }
            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: Error occured in NotesController controller GetNLPData Action: \r\n  {0}", ex.Message), ex.StackTrace);
                return new JsonResult();
            }
        }

        public JsonResult GetUnidentifiedData(string note_id)
        {
            try
            {
                int id;
                if (int.TryParse(note_id, out id))
                {
                    List<Unidentified_TableModel> result = DataAccess.Get_Unidentified_Records(id);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                    return new JsonResult();
            }
            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: Error occured in NotesController controller GetUserData Action: \r\n  {0}", ex.Message), ex.StackTrace);
                return new JsonResult();
            }
        }

        [HttpPost]
        public void SaveData(string NLPData, string UserData, string note_id, string is_complete, string html_text)
        {
            try
            {
                DataAccess.StoreDataTransaction(NLPData, UserData, note_id, is_complete);
                FilesController.SaveHtmlFile(note_id, html_text);
            }
            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: Error occured in NotesController controller SaveData Action: \r\n  {0}", ex.Message), ex.StackTrace);
            }
        }

        public JsonResult GetClientsData()
        {
            try
            {
                List<PersonsAndClientsModel> result = DataAccess.GetClients();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: Error occured in NotesController controller GetClientsData Action: \r\n  {0}", ex.Message), ex.StackTrace);
                return new JsonResult();
            }
        }

        public JsonResult GetPersonData(string note_id)
        {
            try
            {
                int id;
                if (int.TryParse(note_id, out id))
                {
                    List<PersonsAndClientsModel> result = DataAccess.GetPerson(id);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                    return new JsonResult();
            }
          
            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: Error occured in NotesController controller GetPersonData Action: \r\n  {0}", ex.Message), ex.StackTrace);
                return new JsonResult();
            }
        }

    }
}