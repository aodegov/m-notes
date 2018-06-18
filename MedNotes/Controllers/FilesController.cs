using MedNotes.Models;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using word = Microsoft.Office.Interop.Word;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text;
using iTextSharp.text.html.simpleparser;
using System.Collections.Generic;
using Microsoft.Office.Interop.Word;
using System.Configuration;

namespace MedNotes.Controllers
{
    public class FilesController : Controller
    {
        // GET: Files
        public ActionResult ViewFile(string note_id)
        {
            int parsed_note_id;
            FileStreamResult fsResult = null;

            try
            {
                if (int.TryParse(note_id, out parsed_note_id))
                {
                    string html_file_name = DataAccess.GetHtmlFileName(parsed_note_id).Trim();
                    string output_notes_html_folder = ConfigurationManager.AppSettings["output_notes_html_folder"];
                    string html_path = System.IO.Path.Combine(output_notes_html_folder, html_file_name);
                    string mimeType = "text/html";

                    var fileStream = new FileStream(html_path,
                                              FileMode.Open,
                                              FileAccess.Read
                                            );

                    fsResult = new FileStreamResult(fileStream, mimeType);
                }
            }
            catch (Exception ex)
            {
                fsResult = null;
                LogHelper.SaveLogInfo(string.Format("Exception: {0}", ex.Message), ex.StackTrace);
                return null;
            }
            return fsResult;
        }

        public static void SaveHtmlFile(string note_id, string html_text)
        {
            int parsed_note_id;

            try
            {
                if (int.TryParse(note_id, out parsed_note_id))
                {
                    string html_file_name = DataAccess.GetHtmlFileName(parsed_note_id).Trim();
                    string output_notes_html_folder = ConfigurationManager.AppSettings["output_notes_html_folder"];
                    string output_html_path = System.IO.Path.Combine(output_notes_html_folder, html_file_name);

                    System.IO.File.WriteAllText(output_html_path, html_text);

                }
            }
            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: {0}", ex.Message), ex.StackTrace);
            }
        }

        public ActionResult ViewFile_Old(string note_id)
        {
            //  LogHelper.SaveLogInfo(string.Format("Start reading notes file for note_id = {0}", note_id));

            FileStreamResult fsResult = null;
            List<NLP_TableModel> nlp_html_result = null;

            word.Application app = null;
            word.Document doc = null;

            int parsed_note_id;
            string HTML_FileName = "Notes_Temp.htm";
            string mimeType = "text/html";
            string tempPath = ConfigurationManager.AppSettings["Temp_Files_Path"];
            string html_path = System.IO.Path.Combine(tempPath, HTML_FileName);

            if (int.TryParse(note_id, out parsed_note_id))
            {
                try
                {
                    string FileName = DataAccess.GetHtmlFileName(parsed_note_id).Trim();
                    nlp_html_result = DataAccess.Get_NLP_HTML_Highlight(parsed_note_id);

                    if (!string.IsNullOrEmpty(FileName))
                    {

                        string file_path = System.IO.Path.Combine(Server.MapPath("~/Content/Files/"), FileName);

                        FileInfo info = new FileInfo(FileName);

                        string extension = info.Extension;

                        if (extension == ".pdf")
                        {
                            string html_text = ExtractTextFromPdf(file_path, nlp_html_result);
                            System.IO.File.WriteAllText(html_path, html_text);
                        }
                        else if (extension == ".txt")
                        {
                            string text = System.IO.File.ReadAllText(file_path);
                            System.IO.File.WriteAllText(html_path, TextToHtml(text, nlp_html_result));
                        }
                        else if (extension == ".docx" || extension == ".doc")
                        {
                            app = new word.Application();
                            doc = app.Documents.Open(file_path);

                            doc.SaveAs2(html_path, word.WdSaveFormat.wdFormatText);
                            doc.Close();
                            app.Quit();

                            string text = System.IO.File.ReadAllText(html_path);
                            System.IO.File.WriteAllText(html_path, TextToHtml(text, nlp_html_result));

                        }

                    }

                    var fileStream = new FileStream(html_path,
                                            FileMode.Open,
                                            FileAccess.Read
                                          );

                    fsResult = new FileStreamResult(fileStream, mimeType);

                }
                catch (Exception ex)
                {
                    if (doc != null) doc.Close();
                    if (app != null) app.Quit();
                    fsResult = null;
                    LogHelper.SaveLogInfo(string.Format("Exception: {0}", ex.Message), ex.StackTrace);
                    return null;
                }
            }
            return fsResult;
        }

        private string TextToHtml(string text, List<NLP_TableModel> nlp_html_result)
        {
            text = HttpUtility.HtmlEncode(text);
            text = text.Replace("\r\n", "\r");
            text = text.Replace("\n", "\r");
            text = text.Replace("\r", "<br>\r\n");
            text = text.Replace("  ", " &nbsp;");

            text = addHighlight(text, nlp_html_result);

            return text;
        }

        private string ExtractTextFromPdf(string path, List<NLP_TableModel> nlp_html_result)
        {
            ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.LocationTextExtractionStrategy();

            using (PdfReader reader = new PdfReader(path))

            {
                StringBuilder text = new StringBuilder();

                for (int i = 1; i <= reader.NumberOfPages; i++)

                {
                    string thePage = PdfTextExtractor.GetTextFromPage(reader, i, its);

                    thePage = addHighlight(thePage, nlp_html_result);

                    string[] theLines = thePage.Split('\n');

                    foreach (var theLine in theLines)

                    {
                        text.AppendLine(theLine + "</br>");
                    }
                }

                return text.ToString();
            }

        }

        private string addHighlight(string text, List<NLP_TableModel> nlp_html_result)
        {
            foreach (var item in nlp_html_result)
            {
                if (item != null)
                {
                    if (!string.IsNullOrEmpty(item.Nlp_Rejected_Descr))
                        text = text.Replace(item.Nlp_Rejected_Descr, "<a id=\"id_rej\" onclick=\"linkClicker(this);\" data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"Dx Rejected\"  style=\"text-decoration: underline; font-weight:bold; color:red; cursor: pointer;user-select: none;-moz-user-select: none; -ms-user-select: none;  \">" + item.Nlp_Rejected_Descr + "</a>");
                    else
                    {
                        if (item.Nlp_Flag == "A")
                        {
                            text = text.Replace(item.Nlp_Accepted_Descr, "<a id=\"id_acc\" onclick=\"linkClicker(this);\" data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"Dx Accepted\" style=\"text-decoration: underline; font-weight:bold; color:black; cursor: pointer;user-select: none;-moz-user-select: none; -ms-user-select: none;  \">" + item.Nlp_Accepted_Descr + "</a>");
                        }
                        else if (item.Nlp_Flag == "U" && !string.IsNullOrEmpty(item.Related_text))
                        {
                            text = text.Replace(item.Related_text, "<a id=\"id_add\"  onclick=\"linkClicker(this);\" data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"Dx Added\" style=\"text-decoration: underline; font-weight:bold; color:#0BABC8; cursor: pointer;\">" + item.Related_text + "</a>");
                        }
                        else
                        {
                            text = text.Replace(item.Nlp_Accepted_Descr, "<a id=\"id_nm\" onclick=\"linkClicker(this);\" data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"Dx not marked\" style=\"text-decoration: underline; font-weight:bold; color:green; cursor: pointer;user-select: none;-moz-user-select: none; -ms-user-select: none;  \">" + item.Nlp_Accepted_Descr + "</a>");
                        }
                    }
                }
            }

            text += @"<script type='text/javascript'>function linkClicker(obj){  localStorage.setItem('clicked_dx', obj.innerHTML); }</script>";
            return text;
        }


        private void FindAndReplace(Microsoft.Office.Interop.Word.Application doc, object findText, object replaceWithText)
        {
            //options
            object matchCase = false;
            object matchWholeWord = true;
            object matchWildCards = false;
            object matchSoundsLike = false;
            object matchAllWordForms = false;
            object forward = true;
            object format = false;
            object matchKashida = false;
            object matchDiacritics = false;
            object matchAlefHamza = false;
            object matchControl = false;
            object read_only = false;
            object visible = true;
            object replace = 2;
            object wrap = 1;
            //execute find and replace
            doc.Selection.Find.Execute(ref findText, ref matchCase, ref matchWholeWord,
                ref matchWildCards, ref matchSoundsLike, ref matchAllWordForms, ref forward, ref wrap, ref format, ref replaceWithText, ref replace,
                ref matchKashida, ref matchDiacritics, ref matchAlefHamza, ref matchControl);
        }

        public JsonResult SearchValueInHtmlFiles(string search_val, string patient_id)
        {
            JsonResult json_result = null; ; ;

            try
            {
                int parsed_patient_id;
                if (int.TryParse(patient_id, out parsed_patient_id))
                {
                    List<FileModel> patient_docs = DataAccess.GetPatientFiles(parsed_patient_id);
                    List<FileModel> result = FileModel.Get_PopUp_File_List(search_val.ToLower(), patient_docs);
                    json_result = Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: Error occured in FilesController controller SearchValueInHtmlFiles Action: \r\n  {0}", ex.Message), ex.StackTrace);
                json_result = new JsonResult();
            }

            return json_result;
        }
    }

}