using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;

namespace pGina.Plugin.Cbits
{
    public class UInfo
    {
        public string whyCannotLogin;
        public string uname;
        public string fullName;
        public string email;
        public string[] groups;

        public static UInfo parseResponse(string res)
        {
            UInfo u = new UInfo();
            pGinaCbitsResponse jres = JToken.Parse(res).ToObject<pGinaCbitsResponse>();

            // reason why could not login (empty = can login)
            u.whyCannotLogin = jres.message;
            u.uname = jres.username;
            if (u.uname == null)
            {
                throw new Exception("Bad response arrived: " + res);
            }
            u.fullName = jres.name;
            u.email = jres.email;
            u.groups = jres.groups.Split(';');
            if (u.groups.Length == 1 && u.groups[0].Contains(";"))
            {
                throw new Exception("Bad response arrived (groups wrong): " + res);
            }

            return u;
        }
    }
    public class pGinaCbitsResponse
    {
        public String message { get; set; }
        public String username { get; set; }
        public String name { get; set; }
        public String groups { get; set; }
        public String email { get; set; }
    }
}
