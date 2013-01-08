using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
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
        const string MachineKeyPurpose = "CookieTempDataProvider:{0}";
        const string Anonymous = "<anonymous>";

        public void SaveTempData(
            ControllerContext controllerContext,
            IDictionary<string, object> values)
        {
            byte[] bytes = SerializeWithBinaryFormatter(values);
            bytes = Compress(bytes);
            var value = Protect(bytes, controllerContext.HttpContext);
            IssueCookie(controllerContext, value);
        }

        public IDictionary<string, object> LoadTempData(
            ControllerContext controllerContext)
        {
            var value = GetCookieValue(controllerContext);
            var bytes = Unprotect(value, controllerContext.HttpContext);
            bytes = Decompress(bytes);
            return DeserializeWithBinaryFormatter(bytes);
        }

        string GetCookieValue(ControllerContext controllerContext)
        {
            if (controllerContext.HttpContext.Request.Cookies.AllKeys.Contains(CookieName))
            {
                HttpCookie c = controllerContext.HttpContext.Request.Cookies[CookieName];
                if (c != null)
                {
                    return c.Value;
                }
            }
            return null;
        }

        void IssueCookie(ControllerContext controllerContext, string value)
        {
            // if we don't have a value and there's no prior cookie then exit
            if (value == null && !controllerContext.HttpContext.Request.Cookies.AllKeys.Contains(CookieName)) return;

            HttpCookie c = new HttpCookie(CookieName, value)
            {
                // don't allow javascript access to the cookie
                HttpOnly = true,
                // set the path so other apps on the same server don't see the cookie
                Path = controllerContext.HttpContext.Request.ApplicationPath,
                // ideally we're always going over SSL, but be flexible for non-SSL apps
                Secure = controllerContext.HttpContext.Request.IsSecureConnection
            };

            if (value == null)
            {
                // if we have no data then issue an expired cookie to clear the cookie
                c.Expires = DateTime.Now.AddMonths(-1);
            }

            controllerContext.HttpContext.Response.Cookies.Add(c);
        }

        string GetAnonMachineKeyPurpose()
        {
            return String.Format(MachineKeyPurpose, Anonymous);
        }

        string GetMachineKeyPurpose(HttpContextBase ctx)
        {
            return String.Format(MachineKeyPurpose,
                ctx.User.Identity.IsAuthenticated ? ctx.User.Identity.Name : Anonymous);
        }

        string Protect(byte[] data, HttpContextBase ctx)
        {
            if (data == null || data.Length == 0) return null;

            var purpose = GetMachineKeyPurpose(ctx);
            var value = MachineKey.Protect(data, purpose);
            return Convert.ToBase64String(value);
        }

        byte[] Unprotect(string value, HttpContextBase ctx)
        {
            if (String.IsNullOrWhiteSpace(value)) return null;

            var purpose = GetMachineKeyPurpose(ctx);
            var bytes = Convert.FromBase64String(value);
            try
            {
                return MachineKey.Unprotect(bytes, purpose);
            }
            catch (CryptographicException)
            {
                return MachineKey.Unprotect(bytes, GetAnonMachineKeyPurpose());
            }
        }

        byte[] Compress(byte[] data)
        {
            if (data == null || data.Length == 0) return null;

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

        byte[] Decompress(byte[] data)
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
                    return result;
                }
            }
        }

        byte[] SerializeWithBinaryFormatter(IDictionary<string, object> data)
        {
            if (data == null || data.Keys.Count == 0) return null;

            var f = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                f.Serialize(ms, data);
                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        IDictionary<string, object> DeserializeWithBinaryFormatter(byte[] data)
        {
            if (data == null || data.Length == 0) return null;

            var f = new BinaryFormatter();
            using (var ms = new MemoryStream(data))
            {
                var obj = f.Deserialize(ms);
                return obj as IDictionary<string, object>;
            }
        }

        string SerializeWithJsonFormatter(IDictionary<string, object> data)
        {
            if (data == null || data.Keys.Count == 0) return null;

            var s = new JavaScriptSerializer();
            return s.Serialize(data);
        }

        IDictionary<string, object> DeserializeWithJsonFormatter(string data)
        {
            if (data == null || data.Length == 0) return null;

            var s = new JavaScriptSerializer();
            return s.Deserialize<IDictionary<string, object>>(data);
        }
    }
}
