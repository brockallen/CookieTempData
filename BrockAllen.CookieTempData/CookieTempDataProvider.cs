using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace BrockAllen.CookieTempData
{
    public class CookieTempDataProvider : ITempDataProvider
    {
        const string CookieName = "TempData";

        public IDictionary<string, object> LoadTempData(
            ControllerContext controllerContext)
        {
            var value = GetCookieValue(controllerContext);
            var bytes = Unprotect(value);
            value = Decompress(bytes);
            return Deserialize(value);
        }

        public void SaveTempData(
            ControllerContext controllerContext,
            IDictionary<string, object> values)
        {
            string value = Serialize(values);
            var bytes = Compress(value);
            value = Protect(bytes);
            IssueCookie(controllerContext, value);
        }

        string GetCookieValue(ControllerContext controllerContext)
        {
            HttpCookie c = controllerContext.HttpContext.Request.Cookies[CookieName];
            if (c != null)
            {
                return c.Value;
            }
            return null;
        }

        void IssueCookie(ControllerContext controllerContext, string value)
        {
            HttpCookie c = new HttpCookie(CookieName, value)
            {
                HttpOnly = true,
                Path = controllerContext.HttpContext.Request.ApplicationPath,
                Secure = controllerContext.HttpContext.Request.IsSecureConnection
            };
            if (value == null)
            {
                c.Expires = DateTime.Now.AddMonths(-1);
            }
            if (value != null || controllerContext.HttpContext.Request.Cookies[CookieName] != null)
            {
                controllerContext.HttpContext.Response.Cookies.Add(c);
            }
        }
        
        string Protect(byte[] data)
        {
            if (data == null || data.Length == 0) return null;
            
            return MachineKey.Encode(data, MachineKeyProtection.All);
        }

        byte[] Unprotect(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return null;
            
            return MachineKey.Decode(value, MachineKeyProtection.All);
        }

        byte[] Compress(string value)
        {
            if (value == null) return null;

            var data = Encoding.UTF8.GetBytes(value);
            using (var input = new MemoryStream(data))
            {
                using (var output = new MemoryStream())
                {
                    using (Stream cs = new DeflateStream(output, CompressionMode.Compress))
                    {
                        input.CopyTo(cs);
                    }

                    return output.ToArray();
                }
            }
        }

        string Decompress(byte[] data)
        {
            if (data == null || data.Length == 0) return null;
            
            using (var input = new MemoryStream(data))
            {
                using (var output = new MemoryStream())
                {
                    using (Stream cs = new DeflateStream(input, CompressionMode.Decompress))
                    {
                        cs.CopyTo(output);
                    }
                    
                    var result = output.ToArray();
                    return Encoding.UTF8.GetString(result);
                }
            }
        }

        string Serialize(IDictionary<string, object> data)
        {
            if (data == null || data.Keys.Count == 0) return null;

            JavaScriptSerializer ser = new JavaScriptSerializer();
            return ser.Serialize(data);
        }

        IDictionary<string, object> Deserialize(string data)
        {
            if (String.IsNullOrWhiteSpace(data)) return null;

            JavaScriptSerializer ser = new JavaScriptSerializer();
            return ser.Deserialize<IDictionary<string, object>>(data);
        }
    }
}
