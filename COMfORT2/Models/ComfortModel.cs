namespace COMfORT2
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.Linq;


    public class ComfortModel : DbContext
    {
        // Your context has been configured to use a 'ComfortModel' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'COMfORT2.Models.ComfortModel' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'ComfortModel' 
        // connection string in the application configuration file.
        public ComfortModel()
            : base("name=ComfortModel")
        {
        }

        
        public virtual DbSet<Book> Books { get; set; }
        public virtual DbSet<Module> Modules { get; set; }
        public virtual DbSet<Section> Sections { get; set; }
        public virtual DbSet<Chapter> Chapters { get; set; }
        public virtual DbSet<Page> Pages { get; set; }
        public virtual DbSet<BookPage> BookPages { get; set; }
        public virtual DbSet<File> Files { get; set; }
    }

    public class File
    {
        public int FileId { get; set; }
        [StringLength(255)]
        public string FileName { get; set; }
        [StringLength(100)]
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
        public FileType FileType { get; set; }

        public int PageId { get; set; }
    }

    public class Book
    {
        public void Modify(string user)
        {
            this.ModifiedBy = user;
            this.ModifyDate = DateTime.Now;
        }
        public void Create(string user)
        {
            this.ModifiedBy = user;
            this.ModifyDate = DateTime.Now;
            this.CreatedBy = user;
            this.CreateDate = DateTime.Now;
        }
        public int BookId { get; set; }
        
        public bool Published { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifyDate { get; set; }

        public string Name { get; set; }
        public string Version { get; set; }
    }

    public class Module
    {
        public int ModuleId { get; set; }
        
        public int SortOrder { get; set; }
        public string Name { get; set; }
        public string Theme { get; set; }

        public int BookId { get; set; }
        //public virtual Book GetBook { get; set; }
    }

    public class Section
    {
        public int SectionId { get; set; }
        
        public int SortOrder { get; set; }
        public string Name { get; set; }

        public int ModuleId { get; set; }
        //public virtual Module GetModule { get; set; }
        
    }

    public class Chapter
    {
        public int ChapterId { get; set; }

        public string Name { get; set; }
        public int SortOrder { get; set; }

        public int SectionId { get; set; }
        //public virtual Section GetSection { get; set; }

        
    }

    
    public class Page
    {
        public Page() { }
        public Page(string user)
        {
            this.CreateDate = DateTime.Now;
            this.CreatedBy = user;
            this.ModifiedBy = user;
            this.ModifyDate = DateTime.Now;
        }
        public Page(string user, string content)
        {
            this.CreateDate = DateTime.Now;
            this.CreatedBy = user;
            this.ModifiedBy = user;
            this.ModifyDate = DateTime.Now;

            this.PageContent = content;
        }
        public void Modify(string user)
        {
            this.ModifiedBy = user;
            this.ModifyDate = DateTime.Now;
        }

        public void Create(string user)
        {
            this.ModifiedBy = user;
            this.ModifyDate = DateTime.Now;
            this.CreatedBy = user;
            this.CreateDate = DateTime.Now;
        }
        public int PageId { get; set; }
        public string PageContent { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifyDate { get; set; }

        public string Type { get; set; }
    }

    public class BookPage
    {
        public int BookPageId { get; set; }
        
        public int SortOrder { get; set; }

        public int ChapterId { get; set; }
        //public virtual Chapter GetChapter { get; set; }

        public int BookId { get; set; }
        //public virtual Book GetBook { get; set; }

        public int PageId { get; set; }
        //public virtual Page GetPage { get; set; }
    }
}