using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text;
using word = Microsoft.Office.Interop.Word;

namespace cTakesXMLParser
{
    class FilesHelper
    {

        public static void ConvertToTxt(string input_file, string input_notes_folder, string new_file_txt)
        {
            string text = string.Empty;

            FileInfo info = new FileInfo(input_file);
            string extension = info.Extension;

            try {

                if (extension != ".txt")
                {
                    Console.WriteLine("\r\nProcessing of the file: {0}.....\r\n", System.IO.Path.GetFileName(input_file));

                    if (extension == ".pdf")
                    {
                        text = ExtractTextFromPdf(input_file);
                        System.IO.File.WriteAllText(new_file_txt, text);
                    }
                    else if (extension == ".docx" || extension == ".doc")
                    {

                        try
                        {
                            word.Application app = new word.Application();
                            word.Document doc = app.Documents.Open(input_file);

                            doc.SaveAs2(new_file_txt, word.WdSaveFormat.wdFormatText);
                            doc.Close();
                            app.Quit();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Perhaps you do not have MS Office on your PC");
                            LogHelper.SaveLogInfo(string.Format("Exception: {0}", ex.Message), ex.StackTrace);
                        }
                    }

                    BackUpFiles(input_notes_folder, input_file);
                }
            }
            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: {0}", ex.Message), ex.StackTrace);
            }
        }


        public static void CreateHtmlFile(List<dynamic> lst_result, string input_notes_folder, string output_notes_html_folder, string note_file_txt, string note_file_html, string cTakes_text, int note_id)
        {
            string output_html_path = System.IO.Path.Combine(output_notes_html_folder, note_file_html);
            string input_note_file = System.IO.Path.Combine(input_notes_folder, note_file_txt);

            try
            {
                FileInfo info = new FileInfo(input_note_file);

                string extension = info.Extension;
                string text = "";

                if (extension == ".txt")
                {
                    text = cTakes_text;//System.IO.File.ReadAllText(input_note_file);
                    System.IO.File.WriteAllText(output_html_path, TextToHtml(text, lst_result, note_id));
                }

                BackUpFiles(input_notes_folder, input_note_file);
            }

            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: {0}", ex.Message), ex.StackTrace);
            }
        }

        private static string TextToHtml(string text, List<dynamic> lst_result, int note_id)
        {
            text = addHighlight(text, lst_result, note_id);

            return text;
        }

        private static string addHighlight(string text, List<dynamic> lst_result, int note_id)
        {
            string tags_txt = "";
            string sub_txt = "";
            int next_start = 0;

            // text = text.Replace("&#13;", "♪");
            // text = text.Replace("&#10;", "♪");
            // text = text.Replace("\r\n", "&");
            // text = text.Replace("\n", "&");

            bool is_end_longer_text = false;

            foreach (var item in lst_result)
            {

                if (item != null)
                {

                    is_end_longer_text = item.pos_end > text.Length;

                    sub_txt = text.Substring(0, is_end_longer_text ? text.Length : item.pos_end);

                    tags_txt += sub_txt.Substring(next_start).Replace(item.text_descr,
                                        "<a id=\"id_nm\" onclick=\"linkClicker('" +
                                        item.pos_start +
                                        "');\" data-icd='" +
                                        item.icd10_code +
                                        "'data-snomed='" +
                                        item.snomed_code +
                                         "'data-start='" +
                                        item.pos_start +
                                         "'data-end='" +
                                        item.pos_end +
                                        "'data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"Dx not marked\" style=\"text-decoration: underline; font-weight:bold;" +
                                        (item.status != null && item.status == "Rejected" ? "color:red;" : "color:green;") +
                                        " cursor: pointer; user-select: none;-moz-user-select: none; -ms-user-select: none;\">" +
                                        item.text_descr +
                                        "</a>");

                    next_start = is_end_longer_text ? next_start : item.pos_end;

                    if (is_end_longer_text)
                        break;
                }
            }

            text = tags_txt + (is_end_longer_text ? "" : text.Substring(next_start));

            text = text.Replace("\r\n", "<br>");
            text = text.Replace("\n", "<br>");

            text = text.Replace("  ", " &nbsp;");
            text += "\r\n" + "<footer id='" + note_id + "'></footer> " + "\r\n" +
                     "<script type='text/javascript'>function linkClicker(val){  localStorage.setItem('clicked_dx', val); }</script>";

            return text;
        }


        private static string ExtractTextFromPdf(string path)
        {
            ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.LocationTextExtractionStrategy();

            using (PdfReader reader = new PdfReader(path))

            {
                StringBuilder text = new StringBuilder();

                for (int i = 1; i <= reader.NumberOfPages; i++)

                {
                    string thePage = PdfTextExtractor.GetTextFromPage(reader, i, its);

                    string[] theLines = thePage.Split('\n');

                    foreach (var theLine in theLines)

                    {
                        text.AppendLine(theLine);
                    }
                }

                return text.ToString();
            }

        }

        public static void BackUpFiles(string input_folder, string input_file)
        {
            string old_directory = input_folder + @"\old";
            string old_file_path = old_directory + @"\" + System.IO.Path.GetFileName(input_file);

            if (!Directory.Exists(old_directory))
                Directory.CreateDirectory(old_directory);

            // Ensure that the target does not exist.
            if (File.Exists(old_file_path))
                File.Delete(old_file_path);

            System.IO.File.Move(input_file, old_file_path);
        }

    }
}
