using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace cTakesXMLParser
{
    class Program
    {
        static void Main(string[] args)
        {

            string input_notes_folder = ConfigurationManager.AppSettings["input_notes_folder"];
            string cTakes_xml_folder = ConfigurationManager.AppSettings["cTakes_xml_folder"];
            string cTakes_bin_folder = ConfigurationManager.AppSettings["cTakes_bin_folder"];
            string parsed_service_folder = ConfigurationManager.AppSettings["parsed_service_folder"];
            string output_notes_html_folder = ConfigurationManager.AppSettings["output_notes_html_folder"];
            string bypass_umls = ConfigurationManager.AppSettings["bypass_umls"];

            string new_file_txt = string.Empty;
            string input_file_name = string.Empty;
            string input_file_path = string.Empty;

            Dictionary<string, int> input_dict_files;
            Dictionary<string, int> converted_dict_files;

            string indications_file = ConfigurationManager.AppSettings["indications_file"];
            try
            {
                if (!string.IsNullOrEmpty(bypass_umls))
                    RunProcess(bypass_umls, "umlsmock.exe", false);

                input_dict_files = SQLService.GetInputFileNames();
                converted_dict_files = new Dictionary<string, int>();

                if (input_dict_files != null && input_dict_files.Count > 0)
                {
                    string[] input_file_entries = Directory.GetFiles(input_notes_folder);

                    if (input_file_entries.Length > 0)
                    {
                        Console.WriteLine(new string('-', 70));
                        Console.WriteLine("*** FILES CONVERSION ***");
                        Console.WriteLine(new string('-', 70));
                        Console.WriteLine();
                        Console.WriteLine("Start the file conversion process\r\n\r\nPlease wait.........");

                        foreach (string input_file in input_file_entries)
                        {
                            input_file_name = Path.GetFileName(input_file);

                            if (input_dict_files.ContainsKey(input_file_name))
                            {
                                new_file_txt = Path.GetFileNameWithoutExtension(input_file) + ".txt";
                                input_file_path = Path.Combine(input_notes_folder, new_file_txt);

                                FilesHelper.ConvertToTxt(input_file, input_notes_folder, input_file_path);
                                converted_dict_files.Add(new_file_txt, input_dict_files[input_file_name]);
                            }
                        }

                        Console.WriteLine("Finish the file conversion process");
                        Console.WriteLine();
                    }

                    Console.WriteLine(new string('-', 70));
                    Console.WriteLine("*** cTAKES PROCESSING ***");
                    Console.WriteLine(new string('-', 70));
                    Console.WriteLine();
                    RunProcess(cTakes_bin_folder, "runctakesCPE_CLI.bat", true);

                    Parser parser = new Parser();
                    string[] cTakes_file_entries = Directory.GetFiles(cTakes_xml_folder);
                    string file_without_ext = string.Empty;
                    int note_id = 0;

                    if (cTakes_file_entries.Length > 0)
                    {

                        Console.WriteLine("Start the cTakes .xml files processing\r\n\r\nPlease wait.........");

                        foreach (string xml_file in cTakes_file_entries)
                        {
                            file_without_ext = Path.GetFileNameWithoutExtension(xml_file);
                            if (converted_dict_files.ContainsKey(file_without_ext))
                            {
                                note_id = converted_dict_files[file_without_ext];
                                parser.ParseXML(xml_file, input_notes_folder, cTakes_xml_folder, parsed_service_folder, output_notes_html_folder, indications_file, note_id);
                            }
                        }

                        Console.WriteLine("Finishing the cTakes .xml files processing");
                        Console.WriteLine();
                        Console.WriteLine(new string('-', 70));
                        Console.WriteLine();

                        PrintFinalMessage("THE PROCESS HAS BEEN FINISHED SUCCESSFULLY........", bypass_umls);
                    }

                    else
                    {
                        PrintFinalMessage("NO OUTPUT HTML FILES........", bypass_umls);
                    }
                }
                else
                {
                    PrintFinalMessage("NO FILES TO READ.", bypass_umls);
                }
            }
            catch (Exception ex)
            {
                LogHelper.SaveLogInfo(string.Format("Exception: {0}", ex.Message), ex.StackTrace);
                PrintFinalMessage(ex.Message, bypass_umls);
            }

             Console.ReadKey();

        }

        private static void PrintFinalMessage(string reason, string bypass_umls)
        {
            if (!string.IsNullOrEmpty(bypass_umls))
                StopDummyServer("umlsmock");
            Console.WriteLine(reason);
            Console.WriteLine("PRESS ANY KEY TO CLOSE THE PROGRAM.........");
        }
    

        private static void RunProcess(string folder, string filename, bool isExitProcess )
        {
            Process proc = null;
            try
            {
                proc = new Process();
                proc.StartInfo.WorkingDirectory = folder;
                proc.StartInfo.FileName = filename;//;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                if (isExitProcess)
                {
                    proc.WaitForExit();
                    int exit_code = proc.ExitCode;
                    proc.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
                LogHelper.SaveLogInfo(string.Format("Exception: {0}", ex.Message), ex.StackTrace);
            }
        }

        private static void StopDummyServer(string processName)
        {
            Process[] arr_p = Process.GetProcessesByName(processName);

            if (arr_p.Length > 0)
            {
                arr_p[0].Kill();
                arr_p[0].Close();
                arr_p[0].Dispose();
            }
        }


    }
}
