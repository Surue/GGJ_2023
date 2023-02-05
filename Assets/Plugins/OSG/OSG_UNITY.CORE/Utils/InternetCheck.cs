// Old Skull Games
// Bernard Barthelemy
// Friday, March 8, 2019

using System.IO;
using System.Net;

namespace OSG
{
    public static class InternetCheck
    {
        public static bool OffLine()
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://google.com");
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    bool isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
                    if (isSuccess)
                    {
                        using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                        {
                            //We are limiting the array to 80 so we don't have
                            //to parse the entire html document feel free to 
                            //adjust (probably stay under 300)
                            char[] cs = new char[300];
                            reader.Read(cs, 0, cs.Length);
                            string content = new string(cs);
                            return !content.Contains("schema.org/WebPage");
                        }
                    }
                }
            }
            catch
            {

            }
            return true;
        }
    }
}