using System;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace twitterSentiment
{
    public static class twitterClient
    {
        public class twitterPost
        {
            public string text;
            public string uid;

            public twitterPost(string u, string t)
            {
                uid = u;
                text = t;
            }
        }

        public static List<twitterPost> twitterNewsHashtag(string hashtag)
        {
            string twitterHtml = get("https://twitter.com/hashtag/" + hashtag + "?f=tweets&vertical=news&src=hash");
            if (twitterHtml == "") return new List<twitterPost>();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(twitterHtml);
            HtmlNode streamItems = document.GetElementbyId("stream-items-id");
            //Console.WriteLine(streamItems.ChildNodes[1].ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[1].ChildNodes[4].InnerText);
            List<twitterPost> posts = new List<twitterPost>();
            foreach (HtmlNode tweet in streamItems.ChildNodes)
            {
                if (tweet.Name == "#text") continue;
                string uid = WebUtility.HtmlDecode(tweet.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[1].ChildNodes[4].InnerText);
                string text = WebUtility.HtmlDecode(tweet.ChildNodes[1].ChildNodes[3].SelectNodes("div")[1].ChildNodes[1].InnerText);
                //tweet.ChildNodes[1].ChildNodes[3].SelectNodes("//div");
                //Console.WriteLine(text);
                posts.Add(new twitterPost(uid, text));
            }

            return posts;
        }

        public static string get(string uri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Main/Thread]: Web Exception: {0}", ex.Message);
                return "";
            }
        }
    }
}
