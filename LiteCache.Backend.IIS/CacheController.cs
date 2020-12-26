using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Text;
using System.Collections;

using LiteCache.Backend.Helpers;

namespace CloseTheMonth.Backend.Controllers
{
    [Route("cache")]
    public class CacheController : ControllerBase
    {
        private readonly ILogger _logger;

        public CacheController(ILogger<CacheController> logger)
        {
            this._logger = logger;
        }

        // POST cache/{key}
        [HttpPost("{key}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public ActionResult AddFile([FromRoute] string key, [FromBody]AddFileRequest request)
        {
            try
            {
                var path = ConfigHelper.GetAppSetting("CachePath");
                var filePath = System.IO.Path.Combine(path, key.Replace("___", "/"));

                this.CreateFolderForFile(filePath);

                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                System.IO.File.WriteAllBytes(filePath, request.content);

                return Ok();
            }
            catch (Exception ex)
            {
                return this.LogAndReturn500(ex);
            }
        }

        // GET cache/{key}
        [HttpGet("{key}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public ActionResult<byte[]> GetFile([FromRoute] string key) 
        {
            try
            {
                var path = ConfigHelper.GetAppSetting("CachePath");
                var filePath = System.IO.Path.Combine(path, key.Replace("___", "/"));

                if (System.IO.File.Exists(filePath))
                    return Ok(System.IO.File.ReadAllBytes(filePath));

                return Ok(new byte[0]);
            }
            catch (Exception ex)
            {
                return this.LogAndReturn500(ex);
            }
        }

        protected StatusCodeResult LogAndReturn500(Exception ex)
        {
            this.LogError(ex);

            return StatusCode(500);
        }

        protected void LogError(Exception ex)
        {
            var currentException = ex;

            while (currentException != null)
            {
                this._logger.LogError(currentException.Message + currentException.StackTrace + GetDataFromException(currentException));

                currentException = currentException.InnerException;
            }
        }

        private string GetDataFromException(Exception ex)
        {
            var str = new StringBuilder();

            foreach (DictionaryEntry data in ex.Data)
                str.AppendFormat(",{0}={1}", data.Key, data.Value);

            return str.ToString();
        }

        private void CreateFolderForFile(string filePath)
        {
            var file = new System.IO.FileInfo(filePath);

            if (!System.IO.Directory.Exists(file.Directory.FullName))
                System.IO.Directory.CreateDirectory(file.Directory.FullName);
        }

        public class AddFileRequest
        {
            public byte[] content { get; set; }
        }
    }
}
