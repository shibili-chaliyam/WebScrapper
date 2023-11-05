using System.Text;
using System.Text.RegularExpressions;

namespace WebScrapping;

class ListPages
{
    public string PageUrl = "";
    public string PageName = "";
    public bool isProcessed;
    public StringBuilder strContent = new StringBuilder();
}

class Program
{
    public static void FormatPageNameAndURL(ListPages page)
    {
        try
        {
            page.PageName = "\"" + page.PageName + "\"";
            page.PageUrl = "\"" + page.PageUrl + "\"";
        }
        catch
        {
            throw;
        }
    }

    static async void Main(string[] args)
    {
        try
        {
            string url;

            Console.WriteLine("Enter Base URL");
            //url = Console.ReadLine();
            //if (!url[url.Length - 1].Equals("/"))
            //{
            //	url = url + "/";
            //}
            url = "https://boxnovel.com/novel/monster-paradise-webnovel/chapter-1/";
            string keepInUrl = "https://boxnovel.com/novel/monster-paradise-webnovel/";
            //string pattern = url + @"(.*?)/""";
            string pattern = keepInUrl + @"(.*?)""";
            //string pattern = url + @"(.*?)/""";
            //string pattern = url + "([\\s\\S]*?)"+@"/""";
            //string patternOG = @"String Currentversionno = \""([\d.]+)\"";";
            //string pattern = url+@"([\*]+)\""";

            var split = url.Split('/');
            int splitLength = split.Length - 1;

            while (string.IsNullOrEmpty(split[splitLength]) || !char.IsLetter(split[splitLength][0]))
            {
                splitLength--;
            }
            string pageName = split[splitLength];
            string directory = pageName + "/";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            List<ListPages> lstPages = new List<ListPages>();
            lstPages.Add(new ListPages()
            {
                PageUrl = url,
                PageName = pageName + ".html",
                isProcessed = false
            });

            using (HttpClient client = new HttpClient())
            {
                // using (WebClient client = new WebClient())
                // {
                Console.WriteLine("Processing:");
                Console.WriteLine("-----------");
                int count = 1;

                while (lstPages.Count(c => c.isProcessed == false) > 0)
                {
                    var pageToProcess = lstPages.First(f => f.isProcessed == false);
                    pageToProcess.isProcessed = true;
                    Console.WriteLine("{0} - {1}", count++, pageToProcess.PageName);

                    pageToProcess.strContent.Append(await client.GetStringAsync(pageToProcess.PageUrl));

                    var collMathedUrls = Regex.Matches(pageToProcess.strContent.ToString(), pattern);

                    foreach (var collmathedUrl in collMathedUrls)
                    {
                        if (collmathedUrl == null)
                        {
                            continue;
                        }
                        string urlMatched = Convert.ToString(collmathedUrl)!;
                        urlMatched = urlMatched.Substring(0, urlMatched.Length - 1);

                        if (!lstPages.Exists(p => p.PageUrl.Equals(urlMatched)))
                        {
                            var splitMatched = urlMatched.Split('/');
                            int splitLengthMatched = splitMatched.Length - 1;

                            while (string.IsNullOrEmpty(splitMatched[splitLengthMatched]) || !char.IsLetter(splitMatched[splitLengthMatched][0]))
                            {
                                splitLengthMatched--;
                            }

                            lstPages.Add(new ListPages()
                            {
                                PageUrl = urlMatched,
                                PageName = splitMatched[splitLengthMatched] + ".html",
                                isProcessed = false
                            });
                        }
                    }
                }
            }

            lstPages.ForEach(ac => FormatPageNameAndURL(ac));
            foreach (var page in lstPages)
            {
                lstPages.ForEach(ac => ac.strContent.Replace(page.PageUrl, page.PageName));
            }
            foreach (var page in lstPages)
            {
                string fileWithDir = directory + page.PageName.Replace("\"", "");
                File.WriteAllText(fileWithDir, page.strContent.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error Occured: {0}", ex.Message);
        }
    }
}
