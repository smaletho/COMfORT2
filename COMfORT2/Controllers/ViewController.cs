using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace COMfORT2.Controllers
{
    public class ViewController : Controller
    {
        public ActionResult Index(int id)
        {
            ViewBag.BookId = id;
            return View();
        }

        // GET: View
        public ActionResult UploadFile()
        {
            return View(new IndexModel());
        }

        [HttpPost]
        public ActionResult ReadXml(IndexModel model)
        {
            HttpPostedFileBase file = Request.Files[0];

            BinaryReader b = new BinaryReader(file.InputStream);
            byte[] binData = b.ReadBytes(file.ContentLength);

            string fileContentText = System.Text.Encoding.UTF8.GetString(binData);

            XmlDocument xml = new XmlDocument();
            try
            {
                xml.LoadXml(fileContentText);
            }
            catch (Exception e)
            {
                return Content("bad xml     " + e.InnerException.Message);
            }

            ReadElement(xml.DocumentElement, 0, 0, 0, 0, 0, 10, 10, 10, 10);

            return Content("success");

            //return RedirectToAction("Index", new { id= })
        }


        [HttpPost]
        public JsonResult LoadBook(int id)
        {
            BookModel model = new BookModel(id);

            // model
            //  ConfigXml (document)
            //  PageContent (List<PageContentItem>)
            //      PageContentItem:
            //          "Module" (string)
            //          "Section" (string)
            //          "Chapter" (string)
            //          "Page" (string)
            //          "content" document
            //
            //
            //
            //
            
            return Json(new {
                ConfigXml = model.ConfigXml.OuterXml,
                PageContent = model.PageContent
            });
        }
        

        // initialize them all first so I can keep track of IDs as it recurses

        static void ReadElement(XmlElement element, int bookId, int moduleId, int sectionId, int chapterId, int pageId, int m_sortCount, int s_sortCount, int c_sortCount, int p_sortCount)
        {
            ComfortModel cdb = new ComfortModel();

            switch (element.Name)
            {
                case "book":
                    // Create book
                    Book book = new Book();
                    book.Create("tsmale@rktcreative.com");
                    book.Name = element.Attributes["name"].Value;
                    book.Published = true;
                    book.Version = element.Attributes["version"].Value;
                    cdb.Books.Add(book);
                    cdb.SaveChanges();

                    bookId = book.BookId;
                    m_sortCount = 10;
                    break;
                case "module":
                    Module module = new Module();
                    module.BookId = bookId;
                    module.FontColor = element.Attributes["fontcolor"].Value;
                    module.MainColor = element.Attributes["maincolor"].Value;
                    module.Name = element.Attributes["name"].Value;

                    module.SortOrder = m_sortCount;
                    m_sortCount += 10;

                    module.Theme = element.Attributes["theme"].Value;
                    cdb.Modules.Add(module);
                    cdb.SaveChanges();

                    moduleId = module.ModuleId;
                    s_sortCount = 10;
                    break;
                case "section":
                    Section section = new Section();
                    section.ModuleId = moduleId;
                    section.Name = element.Attributes["name"].Value;

                    section.SortOrder = s_sortCount;
                    s_sortCount += 10;

                    cdb.Sections.Add(section);
                    cdb.SaveChanges();

                    sectionId = section.SectionId;
                    c_sortCount = 10;
                    break;
                case "chapter":
                    Chapter chapter = new Chapter();
                    chapter.Name = element.Attributes["name"].Value;
                    chapter.SectionId = sectionId;

                    chapter.SortOrder = c_sortCount;
                    c_sortCount += 10;

                    cdb.Chapters.Add(chapter);
                    cdb.SaveChanges();

                    chapterId = chapter.ChapterId;
                    p_sortCount = 10;
                    break;
                case "page":
                    Page page = new Page();
                    page.Create("tsmale@rktcreative.com");
                    page.PageContent = element.OuterXml.Trim();
                    page.Type = element.Attributes["type"].Value;
                    cdb.Pages.Add(page);
                    cdb.SaveChanges();

                    BookPage bp = new BookPage();
                    bp.BookId = bookId;
                    bp.ChapterId = chapterId;
                    bp.PageId = page.PageId;
                    bp.SortOrder = p_sortCount;
                    cdb.BookPages.Add(bp);
                    cdb.SaveChanges();

                    pageId = bp.BookPageId;
                    break;
                default:
                    break;
            }

            if (element.HasChildNodes)
            {
                foreach (XmlNode child in element.ChildNodes)
                {
                    if (child is XmlElement)
                    {
                        ReadElement(child as XmlElement, bookId, moduleId, sectionId, chapterId, pageId, m_sortCount, s_sortCount, c_sortCount, p_sortCount);
                    }
                }
            }
        }





        public class IndexModel
        {
            [Required, FileExtensions(Extensions = "xml",
             ErrorMessage = "Specify an XML file.")]
            public HttpPostedFileBase File { get; set; }
        }

        public class BookModel
        {
            public BookModel() { }
            public BookModel(int id)
            {
                // Load the book. 
                //  I'll be passing 2 variables to the page:
                //      1. ConfigXml - outlines of the chapters, content, etc.
                //                      does not include *page* content
                //      2. PageContent - contains strictly the pages and what's on them (text, etc.)

                this.ConfigXml = new XmlDocument();
                this.PageContent = new List<PageContentItem>();

                ComfortModel cdb = new ComfortModel();
                var book = cdb.Books.Where(x => x.BookId == id).FirstOrDefault();

                var modules = cdb.Modules.Where(x => x.BookId == book.BookId).ToList();
                var moduleIds = modules.Select(x => x.ModuleId).ToList();

                var sections = cdb.Sections.Where(x => moduleIds.Contains(x.ModuleId)).ToList();
                var sectionIds = sections.Select(x => x.SectionId).ToList();

                var chapters = cdb.Chapters.Where(x => sectionIds.Contains(x.SectionId)).ToList();
                var chapterIds = chapters.Select(x => x.ChapterId).ToList();

                var bookPages = cdb.BookPages.Where(x => chapterIds.Contains(x.ChapterId)).ToList();
                var bookPageIds = bookPages.Select(x => x.PageId).ToList();

                var pages = cdb.Pages.Where(x => bookPageIds.Contains(x.PageId)).ToList();

                // configXml.DocumentElement --> book
                XmlElement root = this.ConfigXml.CreateElement("book");
                root.SetAttribute("name", book.Name);
                root.SetAttribute("version", book.Version);
                root.SetAttribute("author", book.CreatedBy);
                root.SetAttribute("createdate", book.CreateDate.ToString("YYYYMMdd"));
                root.SetAttribute("modifydate", book.ModifyDate.ToString("YYYYMMdd"));
                this.ConfigXml.AppendChild(root);

                int moduleCount = 1;
                int sectionCount = 1;
                int chapterCount = 1;


                foreach (var m in modules.OrderBy(x => x.SortOrder).ToList())
                {
                    XmlElement mod = this.ConfigXml.CreateElement("module");
                    mod.SetAttribute("name", m.Name);
                    mod.SetAttribute("maincolor", m.MainColor);
                    mod.SetAttribute("fontcolor", m.FontColor);
                    mod.SetAttribute("theme", m.Theme);
                    mod.SetAttribute("id", "m_" + moduleCount);
                    

                    root.AppendChild(mod);

                    foreach (var s in sections.Where(x => x.ModuleId == m.ModuleId).OrderBy(x => x.SortOrder).ToList())
                    {
                        XmlElement sec = this.ConfigXml.CreateElement("section");
                        sec.SetAttribute("name", s.Name);
                        sec.SetAttribute("id", "s_" + sectionCount);
                        

                        mod.AppendChild(sec);

                        foreach (var c in chapters.Where(x => x.SectionId == s.SectionId).OrderBy(x => x.SortOrder).ToList())
                        {
                            XmlElement cha = this.ConfigXml.CreateElement("chapter");
                            cha.SetAttribute("name", c.Name);
                            cha.SetAttribute("id", "c_" + chapterCount);
                            

                            sec.AppendChild(cha);

                            foreach (var p in bookPages.Where(x => x.ChapterId == c.ChapterId).OrderBy(x => x.SortOrder).ToList())
                            {
                                var foundPage = pages.Where(x => x.PageId == p.PageId).FirstOrDefault();

                                XmlElement bpa = this.ConfigXml.CreateElement("page");
                                bpa.SetAttribute("id", "p_" + p.PageId);
                                bpa.SetAttribute("type", foundPage.Type);
                                cha.AppendChild(bpa);

                                XmlDocument xml_page = new XmlDocument();
                                try
                                {

                                    PageContentItem newPage = new PageContentItem();
                                    newPage.Chapter = "c_" + chapterCount;




                                    newPage.content = foundPage.PageContent;


                                    newPage.Module = "m_" + moduleCount;
                                    newPage.Page = "p_" + p.BookPageId;
                                    newPage.Section = "s_" + sectionCount;

                                    this.PageContent.Add(newPage);
                                }
                                catch
                                {
                                        
                                }
                            }
                            chapterCount++;
                        }
                        sectionCount++;
                    }
                    moduleCount++;
                }
            }
            public XmlDocument ConfigXml { get; set; }
            public List<PageContentItem> PageContent { get; set; }
        } 


        public class PageContentItem
        {
            public string Module { get; set; }
            public string Section { get; set; }
            public string Chapter { get; set; }
            public string Page { get; set; }
            public string content { get; set; }
        }
    }
}