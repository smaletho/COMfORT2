using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Mail;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;

namespace COMfORT2.Controllers
{
    public class DefaultController : Controller
    {
        // GET: Default
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult ZipTest()
        {
            return View();
        }

        public ActionResult DoZip()
        {
            // delete all the old stuff
            DeleteOldFiles();



            // grab all the stuff
            ViewController.BookModel model = new ViewController.BookModel(1);

            // loop through model.PageContent, and separate out all the XML pieces
            List<string> contentLs = new List<string>();
            foreach (var item in model.PageContent)
            {
                contentLs.Add(item.content);
                item.content = "";
            }


            // change the offline load to read two different files
            string dt = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");


            string fullFileName = Server.MapPath("~/ZipDump/comfort_" + dt + ".zip");
            string fileName = "comfort_" + dt + ".zip";

            string configFile1 = Server.MapPath("~/ZipDump/1config_" + dt + ".js");
            string configFile2 = Server.MapPath("~/ZipDump/2config_" + dt + ".js");
            string homeFile = Server.MapPath("~/ZipDump/index.html");

            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(Server.MapPath("~/Content"));

                // Add Index.html
                //  I need to strip down this file, and include the variables "config1.js" and config2.js"

                using (var tw = new StreamWriter(homeFile, true))
                {
                    string text = System.IO.File.ReadAllText(Server.MapPath("~/Views/View/Index.cshtml"));

                    // replace <link href="~/Content/css/base.css" rel="stylesheet" />
                    //  with <link href="~/css/base.css" rel="stylesheet" />
                    text = text.Replace("<link href=\"~/Content/css/base.css\" rel=\"stylesheet\" />", "<link href=\"./css/base.css\" rel=\"stylesheet\" />");

                    // replace <link href="~/Content/css/pageContent.css" rel="stylesheet" />
                    //  with <link href="~/css/pageContent.css" rel="stylesheet" />
                    text = text.Replace("<link href=\"~/Content/css/pageContent.css\" rel=\"stylesheet\" />", "<link href=\"./css/pageContent.css\" rel=\"stylesheet\" />");

                    // replace <script src="~/Content/js/jquery-3.3.1.min.js"></script>
                    //  with <script src="~/js/jquery-3.3.1.min.js"></script>
                    text = text.Replace("<script src=\"~/Content/js/jquery-3.3.1.min.js\"></script>", "<script src=\"./js/jquery-3.3.1.min.js\"></script>");

                    // replace <script src="~/Content/js/navigation.js"></script>
                    //  with <script src="~/js/navigation.js"></script>
                    text = text.Replace("<script src=\"~/Content/js/navigation.js\"></script>", "<script src=\"./js/navigation.js\"></script>");

                    // replace <script src="~/Content/js/base.js"></script>
                    //  with <script src="~/js/base.js"></script>
                    text = text.Replace("<script src=\"~/Content/js/base.js\"></script>", "<script src=\"./js/base.js\"></script>");

                    // replace <script src="~/Content/js/validate.js"></script>
                    //  with <script src="~/js/validate.js"></script>
                    text = text.Replace("<script src=\"~/Content/js/validate.js\"></script>", "<script src=\"./js/validate.js\"></script>");

                    // replace <script src="~/Content/js/loading.js"></script>
                    //  with <script src="~/js/loading.js"></script>
                    text = text.Replace("<script src=\"~/Content/js/loading.js\"></script>", "<script src=\"./js/loading.js\"></script>");

                    // replace <script src="~/Content/js/shared.js"></script>
                    //  with <script src="~/js/shared.js"></script>
                    text = text.Replace("<script src=\"~/Content/js/shared.js\"></script>", "<script src=\"./js/shared.js\"></script>");

                    // replace <script src="~/Content/js/userTrack.js"></script>
                    //  with <script src="~/js/userTrack.js"></script>
                    text = text.Replace("<script src=\"~/Content/js/userTrack.js\"></script>", "<script src=\"./js/userTrack.js\"></script>");

                    // ---- Special case: replace offline.js with both config files (created next)
                    // replace <script src="~/Content/js/offline.js"></script>
                    //  with <script src="~/js/offline.js"></script><script src="~/js/config1.js"></script><script src="~/js/config2.js"></script>


                    text = text.Replace("<script src=\"~/Content/js/offline.js\"></script>", "<script src=\"./js/offline.js\"></script><script src=\"./js/config1.js\"></script><script src=\"./js/config2.js\"></script>");


                    // replace loading image
                    text = text.Replace("<img src=\"~/Content/images/loading.gif\" />", "<img src=\"./images/loading.gif\" />");
                    
                    tw.Write(text);
                    zip.AddFile(homeFile).FileName = "index.html";
                    
                }

                using (var tw = new StreamWriter(configFile1, true))
                {
                    // create the config file
                    tw.Write("var offline_ConfigXml = $.parseXML(`");
                    tw.Write(model.ConfigXml.OuterXml);
                    tw.Write("`);");
                    
                    zip.AddFile(configFile1).FileName = "/js/config1.js";
                }

                using (var tw = new StreamWriter(configFile2, true))
                {
                    // create the config file
                    tw.Write("var offline_PageContents = `");
                    
                    var jsonSerialiser = new JavaScriptSerializer();
                    var json = jsonSerialiser.Serialize(model.PageContent);

                    tw.Write(json);
                    tw.Write("`;");

                    tw.WriteLine(Environment.NewLine);
                    tw.WriteLine("var offline_PageGuts = [];");

                    tw.WriteLine(Environment.NewLine);
                    tw.WriteLine(Environment.NewLine);
                    tw.WriteLine("function loadGuts() {");

                    foreach (var page in contentLs)
                    {
                        tw.WriteLine("offline_PageGuts.push(`" + page + "`);");
                    }

                    tw.WriteLine("}");

                    zip.AddFile(configFile2).FileName = "/js/config2.js";
                }
                


                


                zip.Save(fullFileName);

                return File(fullFileName, "application/zip", fileName);
            }



        }


        public void DeleteOldFiles()
        {
            string sourceDir = Server.MapPath("~/ZipDump/");

            try
            {
                string[] zipList = Directory.GetFiles(sourceDir, "*.zip");
                string[] jsList = Directory.GetFiles(sourceDir, "*.js");
                string[] htmlList = Directory.GetFiles(sourceDir, "*.html");

                // Delete source files
                foreach (string f in zipList)
                {
                    System.IO.File.Delete(f);
                }
                foreach (string f in jsList)
                {
                    System.IO.File.Delete(f);
                }
                foreach (string f in htmlList)
                {
                    System.IO.File.Delete(f);
                }
            }
            catch (DirectoryNotFoundException dirNotFound)
            {
                Console.WriteLine(dirNotFound.Message);
            }
        }
    }
}