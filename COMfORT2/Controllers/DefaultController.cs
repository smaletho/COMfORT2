using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

        public ActionResult ChooseDownloadBook()
        {
            ComfortModel cdb = new ComfortModel();
            var bookList = cdb.Books.Where(x => x.Published).OrderByDescending(x => x.ModifyDate).ToList();
            return View(bookList);
        }

        

        public ActionResult DownloadBook(int id)
        {
            // delete all the old stuff
            DeleteOldFiles();



            // grab all the stuff
            ComfortModel cdb = new ComfortModel();
            ViewController.BookModel model = new ViewController.BookModel(id);
            var theBook = cdb.Books.Where(x => x.BookId == id).FirstOrDefault();

            // TODO validate

            // loop through model.PageContent, and separate out all the XML pieces
            List<string> contentLs = new List<string>();
            foreach (var item in model.PageContent)
            {
                contentLs.Add(item.content);
                item.content = "";
            }


            // change the offline load to read two different files
            string dt = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");

            // make the name of the book safe for filenames
            string bookName = theBook.Name;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                bookName = bookName.Replace(c, '-');
            }


            string fullFileName = Server.MapPath("~/ZipDump/" + bookName + "_" + dt + ".zip");
            string fileName = bookName + "_" + dt + ".zip";

            string configFile1 = Server.MapPath("~/ZipDump/1config_" + dt + ".js");
            string configFile2 = Server.MapPath("~/ZipDump/2config_" + dt + ".js");
            string homeFile = Server.MapPath("~/ZipDump/index.html");

            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(Server.MapPath("~/Content/"), "/Content/");
                zip.AddDirectory(Server.MapPath("~/fonts/"), "/fonts/");

                // Add Index.html
                //  I need to strip down this file, and include the variables "config1.js" and config2.js"

                using (var tw = new StreamWriter(homeFile, true))
                {
                    string text = System.IO.File.ReadAllText(Server.MapPath("~/Views/View/Index.cshtml"));

                    text = text.Replace("~/", "./");
                    
                    string scriptText = "<script src=\"./Content/js/offline.js\"></script>";
                    scriptText += "<script src=\"./Content/js/config1.js\"></script>";
                    scriptText += "<script src=\"./Content/js/config2.js\"></script>";
                    text = text.Replace("<script src=\"./Content/js/offline.js\"></script>", scriptText);
                    
                    tw.Write(text);
                    zip.AddFile(homeFile).FileName = "index.html";
                    
                }

                using (var tw = new StreamWriter(configFile1, true))
                {
                    // create the config file
                    tw.Write("var offline_ConfigXml = $.parseXML(`");
                    tw.Write(model.ConfigXml.OuterXml);
                    tw.Write("`);");
                    
                    zip.AddFile(configFile1).FileName = "/Content/js/config1.js";
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

                    zip.AddFile(configFile2).FileName = "/Content/js/config2.js";
                }

                // map the images too

                // first get all the pages in the book
                var bookPages = cdb.BookPages.Where(x => x.BookId == theBook.BookId).Select(x => x.PageId).ToList();
                var pages = cdb.Pages.Where(x => bookPages.Contains(x.PageId)).ToList();
                foreach (var p in pages)
                {
                    // find all associated files with that page
                    var files = cdb.Files.Where(x => x.PageId == p.PageId).ToList();
                    int count = 1;
                    foreach (var f in files)
                    {
                        if (f.Content != null)
                        {
                            string fName = Server.MapPath("~/ZipDump/f_" + count.ToString() + "_" + dt);
                            string newFileName = "/Content/";
                            switch (f.FileType)
                            {
                                case FileType.Photo:
                                    switch (f.ContentType)
                                    {
                                        case "image/jpeg":
                                            fName += ".jpg";
                                            newFileName += "images/i_" + f.FileId.ToString() + ".jpg";
                                            break;
                                    }
                                    break;
                                case FileType.Video:
                                    break;
                            }
                            
                            // write the file to the stream
                            using (var tw = new StreamWriter(fName, true))
                            {
                                tw.BaseStream.Write(f.Content, 0, f.Content.Length);

                                zip.AddFile(fName).FileName = newFileName;
                            }
                        }
                    }
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
                string[] jpgList = Directory.GetFiles(sourceDir, "*.jpg");
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