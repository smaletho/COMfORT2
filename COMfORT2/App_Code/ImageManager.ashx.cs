

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace COMfORT2
{
    /// <summary>
    /// Summary description for ImageManager
    /// </summary>
    public class ImageManager : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string fileIdStr = "";
            if (context.Request.QueryString["id"] != null)
                fileIdStr = context.Request.QueryString["id"];
            else
                throw new ArgumentException("No parameter specified");

            int fileId = 0;
            try
            {
                fileId = Convert.ToInt32(fileIdStr.Split('_')[1]);
            }
            catch
            {
                throw new Exception("Failed to split image id");
            }

            ComfortModel cdb = new ComfortModel();
            var image = cdb.Files.Where(x => x.FileId == fileId).FirstOrDefault();

            context.Response.ContentType = image.ContentType;
            Stream strm = new MemoryStream(image.Content);
            byte[] buffer = new byte[4096];
            int byteSeq = strm.Read(buffer, 0, 4096);

            while (byteSeq > 0)
            {
                context.Response.OutputStream.Write(buffer, 0, byteSeq);
                byteSeq = strm.Read(buffer, 0, 4096);
            }
        }
        
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }

    //public class ImageRouteHandler : IRouteHandler
    //{
    //    public IHttpHandler GetHttpHandler(RequestContext requestContext)
    //    {
    //        // TODO: wtf is this
    //        return new ImageManager();
    //    }
    //}
}