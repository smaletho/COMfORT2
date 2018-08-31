using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace COMfORT2.Controllers
{
    public class BuildController : Controller
    {
        // GET: Build
        public ActionResult Index(PageViewModel model)
        {
            return View("Index", model);
        }

        [HttpPost]
        public ActionResult ReadPage(PageViewModel model)
        {
            try
            {
                HttpPostedFileBase file = Request.Files[0];

                BinaryReader b = new BinaryReader(file.InputStream);
                byte[] binData = b.ReadBytes(file.ContentLength);

                string fileContentText = System.Text.Encoding.UTF8.GetString(binData);
                


                // TODO: validate this
                model.XmlContent = fileContentText;



                return RedirectToAction("Index", new { model });
            }
            catch
            {
                return Content("fail");
            }

        }

        public ActionResult TempDbRewrite()
        {
            // find all pages and change "position-x" to "left" and "position-y" to "top"
            ComfortModel cdb = new ComfortModel();
            foreach (var page in cdb.Pages.ToList())
            {
                string content = page.PageContent.Replace("position-x", "left");
                content = content.Replace("position-y", "top");
                page.PageContent = content;
            }

            cdb.SaveChanges();

            return Content("success");
        }

        public ActionResult ViewAssets(int id)
        {
            return View("ViewAssets", new AssetModel(id));
        }

        public ActionResult DeleteFile(int id)
        {
            int pageId = 0;

            ComfortModel cdb = new ComfortModel();
            var file = cdb.Files.Where(x => x.FileId == id).FirstOrDefault();
            pageId = file.PageId;
            cdb.Files.Remove(file);
            cdb.SaveChanges();

            return RedirectToAction("ViewAssets", new { id = pageId });
        }


        [HttpPost]
        public ActionResult UploadAsset(AssetModel model)
        {
            ComfortModel cdb = new ComfortModel();
            // if file is there...
            File f = new File();

            // Convert the httppostedfilebase to a byte[]
            MemoryStream target = new MemoryStream();
            model.UploadedFile.InputStream.CopyTo(target);
            f.Content = target.ToArray();
            
            f.ContentType = model.UploadedFile.ContentType;
            f.FileName = model.UploadedFile.FileName;

            // check if comatable/etc
            //  use ContentType ("image/jpeg" etc)
            f.FileType = FileType.Photo;

            f.PageId = 0;

            cdb.Files.Add(f);
            cdb.SaveChanges();
            return RedirectToAction("ViewAssets", new { id = 0 });
        }


        [HttpPost]
        public ActionResult SavePage(string xml, List<string> images, string email)
        {
            ComfortModel cdb = new ComfortModel();

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch (Exception e)
            {
                return Content("bad xml     " + e.InnerException.Message);
            }

            Page page = new Page(email);
            page.PageContent = "";
            cdb.Pages.Add(page);
            cdb.SaveChanges();

            // grab Type from page node

            // these are page elements
            foreach (XmlElement node in doc.ChildNodes)
            {
                // replace page id
                string newPageId = "p_" + page.PageId.ToString();
                node.SetAttribute("id", newPageId);

                // process the image nodes further
                //  update the page ids so they're all associated with that page
                foreach (XmlElement child in node.ChildNodes)
                {
                    if (child.Name.ToLower() == "image")
                    {
                        string fileId = child.Attributes["id"].Value;
                        try
                        {
                            int id = Convert.ToInt32(fileId.Split('_'));
                            var dbImage = cdb.Files.Where(x => x.FileId == id).FirstOrDefault();
                            if (dbImage != null)
                            {
                                dbImage.PageId = page.PageId;
                            }
                        }
                        catch
                        {

                        }
                        
                    }
                }
            }

            page.PageContent = doc.ToString();
            cdb.SaveChanges();
            

            return Content("success");
        }
    }

    public class AssetModel
    {
        public AssetModel() { }
        public AssetModel(int pageId)
        {
            ComfortModel cdb = new ComfortModel();
            this.FileList = cdb.Files.Where(x => x.PageId == pageId).ToList();
            this.PageId = pageId;
        }
        public List<File> FileList { get; set; }
        public int PageId { get; set; }

        [Required, FileExtensions(Extensions = "xml",
                     ErrorMessage = "Specify an XML file.")]
        public HttpPostedFileBase UploadedFile { get; set; }

    }

    public class PageViewModel
    {
        [Required, FileExtensions(Extensions = "xml",
                     ErrorMessage = "Specify an XML file.")]
        public HttpPostedFileBase File { get; set; }

        public string XmlContent { get; set; }
    }
}