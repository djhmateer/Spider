using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace WpfTest
{
    public class Spider

    {
        bool wasLastGetHtmlAnError;
        List<String> listOfSitesVisited;

        public string GetHtml(string initialWebsite)
        {
            string rawHtml = "";
            try
            {
                var wr = WebRequest.Create(initialWebsite);
                var response = (HttpWebResponse)wr.GetResponse();
                var sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                rawHtml = sr.ReadToEnd();
            }
            //eg 404
            catch (Exception ex)
            {
                Console.WriteLine("   PROBLEM with web request {0}", ex.Message);

                //get last website visited..
                int countOfSites = listOfSitesVisited.Count;
                string lastGoodWebsite = listOfSitesVisited[countOfSites - 2];

                WebRequest wr = WebRequest.Create(lastGoodWebsite);

                var response = (HttpWebResponse)wr.GetResponse();
                var sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                rawHtml = sr.ReadToEnd();
                wasLastGetHtmlAnError = true;

                return rawHtml;
            }

            wasLastGetHtmlAnError = false;
            return rawHtml;
        }

        public List<string> GetAllAbsoluteLinks(string html)
        {
            string anchorPattern = "<a[\\s]+[^>]*?href[\\s]?=[\\s\\\"\']+(?<href>.*?)[\\\"\\']+.*?>(?<fileName>[^<]+|.*?)?<\\/a>";
            MatchCollection matches = Regex.Matches(html, anchorPattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
            if (matches.Count > 0)
            {
                List<string> listOfUris = new List<string>();
                foreach (Match m in matches)
                {
                    string uri = m.Groups[1].ToString();
                    if (uri.StartsWith("http://"))
                        listOfUris.Add(uri);
                }
                return listOfUris;
            }
            return null;
        }

        public List<string> GetExternalLinks(List<string> listOfLinks, string site)
        {
            List<string> listOfExternalLinks = new List<string>();
            if (listOfLinks != null)
            {
                foreach (var item in listOfLinks)
                {
                    //feature in here to ignore common sites like twitter, adobe, apple
                    if (!item.StartsWith(site))
                        listOfExternalLinks.Add(item);
                }
            }
            return listOfExternalLinks;
        }

        public IEnumerable<WebPageInfo> RunSpiderGetNext(string startingSite, int numberOfJumps)
        {
            string site = startingSite;
            listOfSitesVisited = new List<String>();
            string messages = "";
            int noLinksFoundCounter = 0;
            for (int i = 1; i <= numberOfJumps; i++)
            {
                messages += "i is " + i + " Going to: " + site + "\r\n";
                string html = GetHtml(site);

                //eg no links found, so when looping this site will already be in the list
                if (!listOfSitesVisited.Contains(site))
                    listOfSitesVisited.Add(site);

                List<String> listOfLinks = GetAllAbsoluteLinks(html);
                List<String> listOfExternalLinks = GetExternalLinks(listOfLinks, site);

                //pages eg redirects eg www.codecamp.co.nz can have no external links
                if (listOfExternalLinks.Count > 0)
                {
                    string siteToGoToNext = "";
                    //get random link of the page
                    if (wasLastGetHtmlAnError)
                    {
                        int numberOfLinks = listOfExternalLinks.Count();
                        Random r = new Random();
                        int randomNumber = r.Next(1, numberOfLinks);
                        siteToGoToNext = listOfExternalLinks[randomNumber].ToString();
                    }
                    else
                        siteToGoToNext = listOfExternalLinks[0].ToString();

                    WebPageInfo t = new WebPageInfo();
                    bool keepGoing = true;
                    int linkNumberOnPage = 0;

                    while (keepGoing)
                    {
                        if (linkNumberOnPage < listOfExternalLinks.Count - 1)
                        {
                            string[] blacklistStringArray = {"twitter", "mozilla", "firefox", "google", "youtube", "apple", "microsoft", "facebook", "itunes", "t.co", "fsf.org", "creativecommons.org", "adobe", "yahoo"};
                            bool onBlackList = blacklistStringArray.Any(s => siteToGoToNext.Contains(s));

                            if (siteToGoToNext.Length > 30)
                            {
                                messages += "   length problem: siteToGoToNext: " + siteToGoToNext + "\r\n";
                                linkNumberOnPage++;
                                siteToGoToNext = listOfExternalLinks[linkNumberOnPage].ToString();
                            }
                            else if (listOfSitesVisited.Contains(siteToGoToNext) || listOfSitesVisited.Contains(siteToGoToNext + "/"))
                            {
                                messages += "   already visited problem: siteToGoToNext: " + siteToGoToNext + "\r\n";
                                linkNumberOnPage++;
                                siteToGoToNext = listOfExternalLinks[linkNumberOnPage].ToString();
                            }
                            else if (onBlackList)
                            {
                                messages += "   link on blacklist problem: siteToGoToNext: " + siteToGoToNext + "\r\n";
                                linkNumberOnPage++;
                                siteToGoToNext = listOfExternalLinks[linkNumberOnPage].ToString();
                            }
                            else
                            {
                                messages += "   all good: siteToGoToNext: " + siteToGoToNext + "\r\n";
                                siteToGoToNext = listOfExternalLinks[linkNumberOnPage].ToString();
                                t.Uri = site;
                                t.Html = html;
                                t.Messages = messages;
                                t.SizeOfPageInBytes = html.Length;
                                noLinksFoundCounter = 0;
                                yield return t;
                                keepGoing = false;
                            }
                           
                        }
                        else
                        {
                            var countOfSites = listOfSitesVisited.Count;
                            if (countOfSites < 2)
                                throw new Exception("Run out of suitable links and can't go to previous sites!");

                            var newsite = listOfSitesVisited[countOfSites - 2];
                            messages += "     **UP TREE** run out of suitable links on " + site + " problem reverting to " + newsite + "\r\n";
                            site = newsite;

                            t.Uri = site;
                            t.Html = html;
                            t.Messages = messages;
                            t.SizeOfPageInBytes = html.Length;
                            yield return t;
                            keepGoing = false;
                        }
                    }

                    site = siteToGoToNext;
                    messages = "";
                }
                //no links on the page
                else
                {
                    //as I'm not returning anything if a failed jump, then hard to send back a message
                    //messages += "       NO LINKS FOUND on " + site;

                    var countOfSites = listOfSitesVisited.Count;
                    //using try catch to see if first site given doesn't have any links
                    try
                    {
                        //no links found so try previous site
                        if (noLinksFoundCounter == 0)
                            site = listOfSitesVisited[countOfSites - 2];
                        //no links found so try previous +1 site..hoping it exists..to do fix
                        else if (noLinksFoundCounter == 1)
                            site = listOfSitesVisited[countOfSites - 3];
                        //pick any site in list of visited and go to it.. to do fix as we'd need a random link
                        else
                        {
                            Random r = new Random();
                            int randomSiteNumber = r.Next(0, countOfSites - 3);
                            site = listOfSitesVisited[randomSiteNumber];
                        }

                        noLinksFoundCounter++;
                    }
                    catch
                    {
                        throw new Exception("No links - first or previous +1 site doesn't have any links!");
                    }
                }
            }
        }

    }

    public class WebPageInfo
    {
        public string Uri { get; set; }
        public string Html { get; set; }
        public string Messages { get; set; }
        public int SizeOfPageInBytes { get; set; }
    }
}
