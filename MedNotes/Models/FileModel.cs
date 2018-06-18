using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using HtmlAgilityPack;

namespace MedNotes.Models
{
    public class FileModel
    {
        
        private int num_entries;
        private int note_id;
        private string file_name;
        private string file_date;

       
        public int Num_entries { get { return num_entries; } }
        public int Note_id { get { return note_id; } }
        public string File_Name { get { return file_name; } }
        public string File_date { get { return file_date; } }

        public static readonly string Get_HTML_File_SQL =
           @"
                 EXEC [Get_HTML_Note_File] @note_id
            ";

        public static FileModel FileNameDataReader(SqlDataReader dr)
        {
            return new FileModel
            {
                file_name = dr.IsDBNull(0) ? "" : dr.GetString(0)
            };
        }

        public static readonly string Get_Patient_Documents_SQL =
            @"
                 EXEC [dbo].[Get_Patient_Documents]  @patient_id
            ";

        public static FileModel PatientFiles_From_DataReader(SqlDataReader dr)
        {
            return new FileModel
            {
                file_name = dr.IsDBNull(0) ? "" : dr.GetString(0),
                file_date = dr.IsDBNull(1) ? "" : dr.GetString(1)
            };
        }

        public static List<FileModel> Get_PopUp_File_List(string search_val, List<FileModel> lstFiles)
        {
            string output_notes_html_folder = ConfigurationManager.AppSettings["output_notes_html_folder"];
            bool isValueExist = false;

            foreach (FileModel file_model in lstFiles)
            {
                if (!string.IsNullOrEmpty(file_model.File_Name))
                {
                    string html_file = output_notes_html_folder + "\\" + file_model.File_Name;
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.Load(html_file);

                    var bodyNodes = htmlDoc.DocumentNode.SelectNodes("//body");
                    var htmlNodes = bodyNodes == null ? htmlDoc.DocumentNode.Descendants() : bodyNodes;

                    if (htmlNodes != null)
                    {
                        int count_of_entries = 0;

                        foreach (var node in htmlNodes)
                        {
                            count_of_entries += CountEntries(node.InnerText.ToLower(), search_val);
                        }

                        if (count_of_entries > 0)
                        {
                            string note_id_attr = htmlDoc.DocumentNode
                                            .SelectNodes("//footer")
                                            .First()
                                            .Attributes["id"].Value;

                            int id_val = 0;
                            if (int.TryParse(note_id_attr, out id_val))
                            {
                                isValueExist = true;
                                file_model.num_entries = count_of_entries;
                                file_model.note_id = id_val;
                            }
                        }
                    }
                }

            }
            if (isValueExist)
                return lstFiles;
            else
                return new List<FileModel>();
        }

        private static int CountEntries(string text, string word)
        {
            int count = (text.Length - text.Replace(word, "").Length) / word.Length;
            return count;
        }

    }
}