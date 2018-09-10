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

        // when selecting a page for editing, you'll go here.
        //  grab the info from the db and create a PageViewModel
        //  send that to Index to load the page.
        public ActionResult ViewPage(int id)
        {
            PageViewModel model = new PageViewModel();
            ComfortModel cdb = new ComfortModel();
            if (id == 0)
            {
                model.PageId = 0;
                model.PageName = "New Page";
                model.XmlContent = "<page type=\"content\"></page>";
            }
            else
            {
                var page = cdb.Pages.Where(x => x.PageId == id).FirstOrDefault();
                
                model.PageId = id;
                model.XmlContent = page.PageContent;
                model.PageName = page.Title;
            }
            

            return View("Index", model);
        }



        public ActionResult ListBooks()
        {
            // TODO: remove this
            AddTitlesToPages();

            ComfortModel cdb = new ComfortModel();
            return View(cdb.Books.ToList());
        }

        public ActionResult ListPages(int id)
        {
            PageListModel model = new PageListModel(id);
            
            
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ReadPage(PageViewModel model)
        {
            // TODO: validate the XML in the model
            return View("Index", model);











            //try
            //{
            //    HttpPostedFileBase file = Request.Files[0];

            //    BinaryReader b = new BinaryReader(file.InputStream);
            //    byte[] binData = b.ReadBytes(file.ContentLength);

            //    string fileContentText = System.Text.Encoding.UTF8.GetString(binData);
                


            //    // TODO: validate this
            //    model.XmlContent = fileContentText;


            //    return View("Index", model);
            //}
            //catch
            //{
            //    return Content("fail");
            //}

        }


        public ActionResult ViewEditor(int id)
        {
            ComfortModel cdb = new ComfortModel();

            PageViewModel model = new PageViewModel
            {
                PageId = id
            };

            var page = cdb.Pages.Where(x => x.PageId == id).FirstOrDefault();
            if (page != null)
            {
                model.PageName = page.Title;
                string oldContent = page.PageContent;
                model.XmlContent = oldContent.Replace("href=\"javascript:void(0)\"", "href=\"#\"");
            }
            else
            {
                model.PageName = "New Page";
                model.XmlContent = "<page type=\"content\"></page>";
            }
            return View(model);
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
            if (f.ContentType.Contains("image"))
                f.FileType = FileType.Photo;
            else if (f.ContentType.Contains("video"))
                f.FileType = FileType.Video;

            f.PageId = model.PageId;

            cdb.Files.Add(f);
            cdb.SaveChanges();
            return RedirectToAction("ViewAssets", new { id = model.PageId });
        }


        [HttpPost]
        public ActionResult SavePage(string xml, 
            List<string> images, 
            string email, 
            int pageId,
            string name)
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

            var page = cdb.Pages.Where(x => x.PageId == pageId).FirstOrDefault();
            if (page == null)
            {
                page = new Page(email);
                cdb.Pages.Add(page);
            }
            else
            {
                page.Modify(email);
            }

            page.Title = name;

            // TODO: this
            page.Type = "content";
            page.PageContent = "";
            
            cdb.SaveChanges();
            

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
                        string fileId = child.Attributes["source"].Value;
                        try
                        {
                            int id = Convert.ToInt32(fileId.Split('_')[1]);
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

                    if (child.Name.ToLower() == "text")
                    {
                        foreach(var el in child.ChildNodes)
                        {
                            try
                            {
                                XmlElement e = (XmlElement)el;
                                if (e.Name.ToLower() == "a")
                                {
                                    e.SetAttribute("href", "javascript:void(0)");
                                }
                            }
                            catch { }
                        }
                    }
                }
            }

            page.PageContent = doc.OuterXml;
            cdb.SaveChanges();
            
            

            return Content("success");
        }



        public void AddTitlesToPages()
        {
            ComfortModel cdb = new ComfortModel();
            foreach(var page in cdb.Pages.ToList())
            {
                if (string.IsNullOrEmpty(page.Title) || page.Title == "")
                {
                    page.Title = "New Page";
                }
            }
            cdb.SaveChanges();
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
        //[Required, FileExtensions(Extensions = "xml",
        //             ErrorMessage = "Specify an XML file.")]
        //public HttpPostedFileBase File { get; set; }

        [AllowHtml]
        public string XmlContent { get; set; }
        public int PageId { get; set; }
        public string PageName { get; set; }
    }

    public class PageListModel
    {
        public PageListModel() { }
        public PageListModel(int bookId)
        {
            this.PageList = new List<Page>();
            this.BookPages = new List<BookPage>();

            //List<Page> pageList = new List<Page>();
            ComfortModel cdb = new ComfortModel();
            if (bookId == 0)
            {
                // must find all pages not associated with a book

                // get all pages
                var pages = cdb.Pages.Select(x => x.PageId).ToList();

                // get all BookPages that contain these pages
                var usedBookPages = cdb.BookPages.Where(x => pages.Contains(x.PageId)).Select(x => x.PageId).ToList();

                // find all pages that are not in the previous list
                this.PageList = cdb.Pages.Where(x => !usedBookPages.Contains(x.PageId)).ToList();

                this.Modules = new List<Module>();
                this.Sections = new List<Section>();
                this.Chapters = new List<Chapter>();
                this.GetBook = new Book();
            }
            else
            {
                this.BookPages = cdb.BookPages.Where(x => x.BookId == bookId).OrderBy(x => x.SortOrder).ToList();
                List<int> bpId = this.BookPages.Select(x => x.PageId).ToList();
                this.PageList = cdb.Pages.Where(x => bpId.Contains(x.PageId)).ToList();

                this.GetBook = cdb.Books.Where(x => x.BookId == bookId).FirstOrDefault();

                this.Modules = cdb.Modules.Where(x => x.BookId == bookId).ToList();
                var moduleIdList = this.Modules.Select(x => x.ModuleId).ToList();

                this.Sections = cdb.Sections.Where(x => moduleIdList.Contains(x.ModuleId)).ToList();
                var sectionIdList = this.Sections.Select(x => x.SectionId).ToList();

                this.Chapters = cdb.Chapters.Where(x => sectionIdList.Contains(x.SectionId)).ToList();
            }
        }
        public List<Page> PageList { get; set; }
        public List<BookPage> BookPages { get; set; }
        public Book GetBook { get; set; }
        public List<Section> Sections { get; set; }
        public List<Module> Modules { get; set; }
        public List<Chapter> Chapters { get; set; }
    }
}