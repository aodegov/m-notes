using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace cTakesXMLParser
{
    class Parser
    {
        string input_xml_fileName;
        string output_parsed_file;
        List<dynamic> lst_SNOMED_Codes;
        List<dynamic> lst_result;
        string cTakesTextstring;

        public void ParseXML(string xml_file_name, string input_notes_folder, string cTakes_xml_folder, string parsed_service_folder, string output_notes_html_folder, string indications_file, int note_id)
        {
            try
            {
                ConsoleSpinner spin = new ConsoleSpinner();

                lst_SNOMED_Codes = new List<dynamic>();
                input_xml_fileName = Path.GetFileName(xml_file_name);
                output_parsed_file = Path.Combine(parsed_service_folder, Path.GetFileNameWithoutExtension(xml_file_name));

                Console.WriteLine("\r\nProcessing of the file: {0}.....\r\n", input_xml_fileName);

                if (!string.IsNullOrEmpty(xml_file_name))
                {
                    var excel_rows = GetExcelRows(indications_file)
                                 .Select(dataRow => new
                                 {
                                     code = dataRow["Code"].ToString(),
                                     name = dataRow["Name"].ToString().ToLower(),
                                     confidence = dataRow["Confidence"].ToString(),
                                     status = dataRow["Status"].ToString()
                                 });

                    var indicationNames = excel_rows.Select(n => n.name.ToLower()).ToList();

                    if (!Directory.Exists(parsed_service_folder))
                        Directory.CreateDirectory(parsed_service_folder);

                    StringBuilder sb_descr = new StringBuilder();
                    sb_descr.AppendLine(DateTime.Now.ToString());
                    sb_descr.AppendLine(string.Format("Parsing of file: {0}", input_xml_fileName));
                    sb_descr.AppendLine(new string('-', 80));
                    sb_descr.AppendLine("Descriptions");
                    sb_descr.AppendLine(new string('-', 80));
                    sb_descr.AppendLine();

                    XDocument document = XDocument.Load(xml_file_name);

                    cTakesTextstring = document.Descendants("uima.cas.Sofa")
                                    .Select(r => r.Attribute("sofaString").Value).FirstOrDefault();

                    if (!string.IsNullOrEmpty(cTakesTextstring))
                    {
                        var positions = document.Descendants("org.apache.ctakes.typesystem.type.textsem.DiseaseDisorderMention")
                                        .Select(r => new { begin = r.Attribute("begin").Value, end = r.Attribute("end").Value, arr_ref = r.Attribute("_ref_ontologyConceptArr").Value });

                        int descr_num = 1;
                        int cur_begin = -1;
                        int cur_end = -1;
                        int prev_begin = -1;
                        int prev_end = -1;

                        if (positions.Count() > 0)
                        {

                            var sentences = document.Descendants("org.apache.ctakes.typesystem.type.textspan.Sentence")
                                                .Select(r => new { begin = int.Parse(r.Attribute("begin").Value), end = int.Parse(r.Attribute("end").Value), num = r.Attribute("sentenceNumber").Value });

                            var treebankNode = document.Descendants("org.apache.ctakes.typesystem.type.syntax.TerminalTreebankNode")
                                               .Select(r => new { begin = int.Parse(r.Attribute("begin").Value), end = int.Parse(r.Attribute("end").Value), nodeValue = r.Attribute("nodeValue").Value.ToLower() });

                            var measurements = document.Descendants("org.apache.ctakes.typesystem.type.textsem.RomanNumeralAnnotation")
                                               .Select(r => new { begin = int.Parse(r.Attribute("begin").Value), end = int.Parse(r.Attribute("end").Value), _id = r.Attribute("_id").Value.ToLower() });

                            var semantics = document.Descendants("org.apache.ctakes.typesystem.type.textsem.SemanticArgument")
                                               .Select(r => new { begin = int.Parse(r.Attribute("begin").Value), end = int.Parse(r.Attribute("end").Value), _id = r.Attribute("_id").Value.ToLower() });


                            foreach (var result in positions)
                            {
                                spin.Turn();

                                cur_begin = int.Parse(result.begin);
                                cur_end = int.Parse(result.end);

                                var cur_sentence = sentences
                                                   .Where(e => e.begin <= cur_begin && e.end >= cur_end)
                                                   .Select(s => s);


                                var checkMeasurement = measurements
                                                                    .Where(e => e.begin == cur_begin && e.end == cur_end)
                                                                    .Select(e => e._id);

                                var checkSemantic = semantics
                                                             .Where(e => e.begin == cur_begin && e.end == cur_end)
                                                             .Select(e => e._id);

                                var cur_descr = treebankNode
                                                            .Where(e => e.begin >= cur_begin
                                                                    && e.end <= cur_end
                                                                    &&
                                                                    !(
                                                                        e.begin >= prev_begin && e.end <= prev_end)
                                                                     )
                                                            .Select(e => e.nodeValue + " ");

                                var indications = treebankNode
                                                              .Where(tb => cur_sentence.All(sent => (sent.begin <= tb.begin && sent.end >= tb.end))
                                                                     && indicationNames.Contains(tb.nodeValue))
                                                              .Select(e => new { ind_name = e.nodeValue, begin = e.begin, end = e.end });

                                if (cur_descr.Count() > 0 && checkSemantic.Count() == 0 && checkMeasurement.Count() == 0)
                                {
                                    string descr_val = "";
                                    StringBuilder help_descr = new StringBuilder();

                                    foreach (var d_itm in cur_descr)
                                    {
                                        descr_val += d_itm;
                                    }

                                    help_descr.AppendLine(string.Format("Begin: {0}; End: {1}", cur_begin, cur_end));
                                    help_descr.AppendLine("Codes: ");

                                    var cas_arr = document.Descendants("uima.cas.FSArray")
                                                        .Where(e => e.Attribute("_id").Value == result.arr_ref)
                                                        .Descendants("i")
                                                        .Select(e => e.Value);

                                    int code_num = 1;

                                    foreach (var id_itm in cas_arr)
                                    {
                                        spin.Turn();

                                        string currCode = document.Descendants("org.apache.ctakes.typesystem.type.refsem.UmlsConcept")
                                                        .Where(e => e.Attribute("_id").Value == id_itm)
                                                        .Select(e => e.Attribute("code").Value).FirstOrDefault();

                                        var curr_indication = indications.Count() > 0 
                                                             ?
                                                               excel_rows
                                                                         .Where(ex => indications.Any(ind => ex.name == ind.ind_name))
                                                             :
                                                                null
                                                             ;


                                        // fill dynamic list //
                                        lst_SNOMED_Codes.Add(new
                                        {
                                            snomed_code = currCode,
                                            text_descr = descr_val.Trim(),
                                            pos_start = cur_begin,
                                            pos_end = cur_end,
                                            confidence = curr_indication == null ? "-" : curr_indication.Select(ex => ex.confidence).FirstOrDefault(),
                                            status = curr_indication == null ? "-" : curr_indication.Select(ex => ex.status).FirstOrDefault()
                                        });


                                        string code_with_indications = curr_indication == null ? currCode : (currCode + curr_indication.Select(ex => " - " + ex.code + "(" + ex.name + ")").FirstOrDefault());

                                        help_descr.AppendLine(string.Format("\t{0}) {1}", code_num, code_with_indications));
                                        code_num++;
                                    }

                                    if (sb_descr.ToString().Contains(descr_val))
                                    {
                                        sb_descr.Replace(descr_val, descr_val + "\n" + help_descr.ToString());
                                    }
                                    else
                                    {
                                        sb_descr.AppendLine(string.Format("{0}. Description: {1}", descr_num, descr_val));
                                        sb_descr.AppendLine(help_descr.ToString());
                                        sb_descr.AppendLine();
                                        descr_num++;
                                    }

                                    prev_begin = cur_begin;
                                    prev_end = cur_end;
                                }
                            }

                            using (StreamWriter sw = new StreamWriter(output_parsed_file))
                            {
                                sw.Write(sb_descr.ToString());
                            }

                            FilesHelper.BackUpFiles(cTakes_xml_folder, xml_file_name);

                            string file_result_txt = Path.GetFileName(output_parsed_file);
                            string file_result_html = Path.GetFileNameWithoutExtension(output_parsed_file) + ".htm";
                            lst_result = SQLService.ExecuteCompareSQL(lst_SNOMED_Codes);
                            FilesHelper.CreateHtmlFile(lst_result, input_notes_folder, output_notes_html_folder, file_result_txt, file_result_html, cTakesTextstring, note_id);
                            SQLService.CreateNoteConditions(lst_result, file_result_html, note_id);
                        }
                        else
                            Console.WriteLine("cTakes text string is empty");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: {0}", ex.Message), ex.StackTrace);
            }

        }

        private static DataTable GetExcelDataTable(string file_path)
        {
            var file = new FileInfo(file_path);
            DataTable dataTable = null;

            using (FileStream fs = File.Open(file_path, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (file.Extension.Equals(".xls"))
                    reader = ExcelReaderFactory.CreateBinaryReader(fs);
                else if (file.Extension.Equals(".xlsx"))
                    reader = ExcelReaderFactory.CreateOpenXmlReader(fs);
                else
                    throw new Exception("Invalid FileName");

                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                };

                var dataSet = reader.AsDataSet(conf);
                dataTable = dataSet.Tables[0];
            }

            return dataTable;
        }

        private static IEnumerable<DataRow> GetExcelRows(string file_path)
        {
            return from DataRow row in GetExcelDataTable(file_path).Rows select row;
        }


    }
}
