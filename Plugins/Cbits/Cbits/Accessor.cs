using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

using pGina.Shared.Types;
using log4net;
using System.Text;

using Newtonsoft.Json.Linq;

namespace pGina.Plugin.Cbits
{
    public class HttpAccessor
    {
        private static Dictionary<string, UInfo> resps = new Dictionary<string, UInfo>();
        private static ILog m_logger = LogManager.GetLogger("CbitsAccessor");

        static HttpAccessor()
        {
        }

        public static BooleanResult getResponse(UserInformation uinfo)
        {
            try
            {
                // BEGIN TLS1.2 hack
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                ServicePointManager.DefaultConnectionLimit = 9999;
                // END TLS1.2 hack

                WebRequest request = WebRequest.Create(Settings.resolveSettings());
                request.Method = "POST";
                request.Timeout = 5000;
                dynamic postObject = new JObject();
                postObject.username = uinfo.Username;
                postObject.password = uinfo.Password;
                string postData = postObject.ToString();
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/json";
                request.ContentLength = byteArray.Length;

                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
    
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            string responseFromServer = reader.ReadToEnd();
                            m_logger.InfoFormat("Response: {0}", responseFromServer);
                            UInfo proc_uinfo = UInfo.parseResponse(responseFromServer);
                            if (resps.ContainsKey(proc_uinfo.uname))
                            {
                                resps.Remove(proc_uinfo.uname);
                            }
                            resps.Add(proc_uinfo.uname, proc_uinfo);
                            uinfo.Username = proc_uinfo.uname;
                            uinfo.Fullname = proc_uinfo.fullName;
                        }
                    }
                }
                return new BooleanResult() { Success = true };
            }
            catch(WebException webx)
            {
                m_logger.ErrorFormat("Accessor.WebException: {0}", webx.Message);

                using (HttpWebResponse res = (HttpWebResponse)webx.Response)
                {
                    if (res != null)
                    {
                        using (StreamReader resReader = new StreamReader(res.GetResponseStream()))
                        {
                            string responseBody = resReader.ReadToEnd();
                            if (responseBody.Length > 0)
                            {
                                String message = ((JObject)JToken.Parse(responseBody)).Value<String>("message");
                                return new BooleanResult() { Success = false, Message = message };
                            }
                        }
                    }
                }

                return new BooleanResult() { Success = false, Message = webx.Message };
            }
            catch (Exception e)
            {
                // very bad scenario
                m_logger.ErrorFormat("Accessor.Exception: {0}", e.StackTrace);
                return new BooleanResult() { Success = false, Message = e.Message };
            }
            
        }

        public static BooleanResult getPwChangeResponse(String uname, String pwd, String old)
        {
            try
            {
                // Create a request using a URL that can receive a post. 
                WebRequest request = WebRequest.Create(Settings.resolveSettings());
                // Set the Method property of the request to POST.
                request.Method = "POST";
                request.Timeout = 2000;
                // Create POST data and convert it to a byte array.
                string postData = "{\"username\":\"" + uname + "\",\"password\":\"" + pwd + "\",\"old\":\"" + pwd + "\"}";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/json";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;

                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                // Get the response.
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        // Open the stream using a StreamReader for easy access.
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            // Read the content.
                            string responseFromServer = reader.ReadToEnd();
                            // Display the content.
                            m_logger.InfoFormat("PWDCHResponse: {0}", responseFromServer);
                            return new BooleanResult() { Success = true, Message = responseFromServer };
                        }
                    }
                }
            }
            catch (WebException webx)
            {
                m_logger.ErrorFormat("PWDCHAccessor.WebException: {0}", webx.Message);

                using (HttpWebResponse res = (HttpWebResponse)webx.Response)
                {
                    if (res != null)
                    {
                        using (StreamReader resReader = new StreamReader(res.GetResponseStream()))
                        {
                            string responseBody = resReader.ReadLine();
                            if (responseBody.Length > 0)
                            {
                                return new BooleanResult() { Success = false, Message = responseBody };
                            }
                        }
                    }
                }

                return new BooleanResult() { Success = false, Message = webx.Message };
            }
            catch (Exception e)
            {
                // very bad scenario
                m_logger.ErrorFormat("PWDCHAccessor.Exception: {0}", e.StackTrace);
                return new BooleanResult() { Success = false, Message = e.Message };
            }

        }

        public static UInfo getUserInfo(String uname)
        {
            if (! resps.ContainsKey(uname))
            {
                return null;
            }
            return resps[uname];
        }
    }
}
