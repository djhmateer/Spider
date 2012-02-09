﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace WpfTest
{
    public class Spider
    {
        public string GetHtml(string initialWebsite)
        {
            string rawHtml = "";
            //if not a successful request then revert to stuff
            try
            {
                WebRequest wr = WebRequest.Create(initialWebsite);
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                rawHtml = sr.ReadToEnd();
            }
            //eg 404
            catch (Exception ex)
            {
                Console.WriteLine("   PROBLEM with web request {0}", ex.Message);
                WebRequest wr = WebRequest.Create("http://www.stuff.co.nz");
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                rawHtml = sr.ReadToEnd();

            }
            return rawHtml;
        }

        public string GetFirstLink(string initialWebsite)
        {
            string rawHtml = GetHtml(initialWebsite);

            int x = rawHtml.IndexOf("<a href=");

            string y = rawHtml.Substring(x + 10);

            int z = y.IndexOf("\"");

            var a = y.Substring(0, z);
            return a;
        }

        public List<string> GetAllLinks(string message)
        {
            string anchorPattern = "<a[\\s]+[^>]*?href[\\s]?=[\\s\\\"\']+(?<href>.*?)[\\\"\\']+.*?>(?<fileName>[^<]+|.*?)?<\\/a>";
            MatchCollection matches = Regex.Matches(message, anchorPattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
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
            //to get rid of m.stuff.co.nz and i.stuff.co.nz links
            string mob = site.Replace("http://www.", "http://m.");
            string mobi = site.Replace("http://www.", "http://i.");

            List<string> listOfExternalLinks = new List<string>();
            if (listOfLinks != null)
            {
                foreach (var item in listOfLinks)
                {
                    //if (!item.StartsWith(site) && (!item.StartsWith(mob)) && (!item.StartsWith(mobi)))
                    if (!item.StartsWith(site))
                        listOfExternalLinks.Add(item);
                }
            }
            return listOfExternalLinks;
        }

        public List<String> RunSpider(string startingSite, int numberOfJumps)
        {
            string site = startingSite;
            var listOfSitesVisited = new List<String>();
            for (int i = 1; i <= numberOfJumps; i++)
            {
                Console.WriteLine("i is {0} Going to: {1} ", i, site);
                string html = GetHtml(site);

                //eg no links found, so when looping this site will already be in the list
                if (!listOfSitesVisited.Contains(site))
                    listOfSitesVisited.Add(site);

                List<String> listOfLinks = GetAllLinks(html);
                List<String> listOfExternalLinks = GetExternalLinks(listOfLinks, site);

                //eg a redirect can have no external links
                if (listOfExternalLinks.Count > 0)
                {
                    string siteToGoToNext = listOfExternalLinks[0].ToString();
                    bool keepGoing = true;
                    int j = 0;

                    while (keepGoing)
                    {
                        if (j < listOfExternalLinks.Count - 1)
                        {
                            if (siteToGoToNext.Length > 30)
                            {
                                Console.WriteLine("   length problem: siteToGoToNext: {0} ", siteToGoToNext);
                                j++;
                                siteToGoToNext = listOfExternalLinks[j].ToString();
                            }
                            else if (listOfSitesVisited.Contains(siteToGoToNext))
                            {
                                Console.WriteLine("   already visited problem: siteToGoToNext: {0} ", siteToGoToNext);
                                j++;
                                siteToGoToNext = listOfExternalLinks[j].ToString();
                            }
                            else
                            {
                                Console.WriteLine("   all good: siteToGoToNext: {0} ", siteToGoToNext);
                                siteToGoToNext = listOfExternalLinks[j].ToString();
                                keepGoing = false;
                            }
                        }
                        else
                        {
                            var countOfSites = listOfSitesVisited.Count;
                            var newsite = listOfSitesVisited[countOfSites - 2];
                            Console.WriteLine("      run out of suitable links on {0} problem reverting to {1}", site, newsite);
                            site = newsite;
                            keepGoing = false;
                        }
                    }

                    site = siteToGoToNext;
                }
                else
                {
                    Console.WriteLine("        NO LINKS FOUND on {0}", site);
                    var countOfSites = listOfSitesVisited.Count;
                    //eg if first site given doesn't have any links
                    try
                    {
                        site = listOfSitesVisited[countOfSites - 2];
                    }
                    catch
                    {
                        Console.WriteLine("      PROBLEM with first site not having any links");
                        site = "http://www.stuff.co.nz";
                    }
                }
            }

            return listOfSitesVisited;
        }

        List<string> listOfString = new List<string>();

        public string Start(string startingSite)
        {
            listOfString.Add("a");
            listOfString.Add("b");
            listOfString.Add("c");
            listOfString.Add("d");
            listOfString.Add("e");
            return "ok";
        }

        public IEnumerable<string> GetNext()
        {
            foreach (var item in listOfString)
            {
                yield return item;
            }
            
        }

        public IEnumerable<String> RunSpiderGetNext(string startingSite, int numberOfJumps)
        {
            string site = startingSite;
            var listOfSitesVisited = new List<String>();
            for (int i = 1; i <= numberOfJumps; i++)
            {
                Console.WriteLine("i is {0} Going to: {1} ", i, site);
                string html = GetHtml(site);

                //eg no links found, so when looping this site will already be in the list
                if (!listOfSitesVisited.Contains(site))
                    listOfSitesVisited.Add(site);

                List<String> listOfLinks = GetAllLinks(html);
                List<String> listOfExternalLinks = GetExternalLinks(listOfLinks, site);

                //eg a redirect can have no external links
                if (listOfExternalLinks.Count > 0)
                {
                    string siteToGoToNext = listOfExternalLinks[0].ToString();
                    bool keepGoing = true;
                    int j = 0;

                    while (keepGoing)
                    {
                        if (j < listOfExternalLinks.Count - 1)
                        {
                            if (siteToGoToNext.Length > 30)
                            {
                                Console.WriteLine("   length problem: siteToGoToNext: {0} ", siteToGoToNext);
                                j++;
                                siteToGoToNext = listOfExternalLinks[j].ToString();
                            }
                            else if (listOfSitesVisited.Contains(siteToGoToNext))
                            {
                                Console.WriteLine("   already visited problem: siteToGoToNext: {0} ", siteToGoToNext);
                                j++;
                                siteToGoToNext = listOfExternalLinks[j].ToString();
                            }
                            else
                            {
                                Console.WriteLine("   all good: siteToGoToNext: {0} ", siteToGoToNext);
                                siteToGoToNext = listOfExternalLinks[j].ToString();
                                yield return siteToGoToNext;
                                keepGoing = false;
                            }
                        }
                        else
                        {
                            var countOfSites = listOfSitesVisited.Count;
                            var newsite = listOfSitesVisited[countOfSites - 2];
                            Console.WriteLine("      run out of suitable links on {0} problem reverting to {1}", site, newsite);
                            site = newsite;
                            keepGoing = false;
                        }
                    }

                    site = siteToGoToNext;
                }
                else
                {
                    Console.WriteLine("        NO LINKS FOUND on {0}", site);
                    var countOfSites = listOfSitesVisited.Count;
                    //eg if first site given doesn't have any links
                    try
                    {
                        site = listOfSitesVisited[countOfSites - 2];
                    }
                    catch
                    {
                        Console.WriteLine("      PROBLEM with first site not having any links");
                        site = "http://www.stuff.co.nz";
                    }
                }
            }

            //return listOfSitesVisited;
        }
    }

    [TestFixture]
    public class SpiderTests
    {
        string startingSite = "http://www.stuff.co.nz";
        //string startingSite = "http://www.cnn.com";
        //string startingSite = "http://www.holidayhouses.co.nz";

        [Test]
        //events, tasks, reactive extensions.
        public void RunSpiderAndGetNext_GivenAStartingWebsite_ReturnEachSiteVisitedName()
        {
            Spider s = new Spider();

            var thing = s.RunSpiderGetNext(startingSite, 5);

            foreach (var item in thing)
            {
                Console.WriteLine(item);
            }
        }


        [Test]
        public void GetHtml_GivenAWebsite_ReturnsTheRawHtmlAndRedersInWPFControl()
        {
            Spider s = new Spider();

            string result = s.GetHtml(startingSite);

            Assert.IsNotNullOrEmpty(result);
            Console.WriteLine(result);
        }

        [Test]
        public void GetHtml_GivenAWebsite_ReturnsTheRawHtml()
        {
            Spider s = new Spider();

            string result = s.GetHtml(startingSite);

            Assert.IsNotNullOrEmpty(result);
            Console.WriteLine(result);
        }

        [Test]
        public void GetFirstLink_GivenAWebsite_ReturnsTheFirstLink()
        {
            Spider s = new Spider();

            string result = s.GetFirstLink(startingSite);

            Assert.IsNotNullOrEmpty(result);
            Console.WriteLine(result);
        }

        [Test]
        public void GetAllLinks_GivenHtml_ReturnsACollectionOfAllExternalLinks()
        {
            Spider s = new Spider();
            string html = s.GetHtml(startingSite);
            var result = s.GetAllLinks(html);

            CollectionAssert.IsNotEmpty(result);
            foreach (var item in result)
            {
                Console.WriteLine(item);
            }
        }

        [Test]
        public void GetExternalLinks_GivenAListOfLinks_ReturnsACollectionOfOnlyExternalLinksAndNotMobileVersionsOfSite()
        {
            Spider s = new Spider();
            string html = s.GetHtml(startingSite);
            List<String> listOfLinks = s.GetAllLinks(html);
            var result = s.GetExternalLinks(listOfLinks, startingSite);

            CollectionAssert.IsNotEmpty(result);
            foreach (var item in result)
            {
                Console.WriteLine(item);
            }
        }

        [Test]
        public void GetExternalLinks_GivenAListOfExternalLinks_GetHtmlForFirstLink()
        {
            Spider s = new Spider();
            string html = s.GetHtml(startingSite);
            List<String> listOfLinks = s.GetAllLinks(html);
            List<String> listOfExternalLinks = s.GetExternalLinks(listOfLinks, startingSite);

            Console.WriteLine("Going to: " + listOfExternalLinks[0].ToString());
            html = s.GetHtml(listOfExternalLinks[0]);

            Assert.IsNotNullOrEmpty(html);

            Console.WriteLine(html);
        }

        [Test]
        public void RunSpider_GivenAStartingWebsiteAnd5Jumps_ReturnAListOfWebsitesVisitedWhichShouldBeUnique()
        {
            Spider s = new Spider();
            var listOfSitesVisited = s.RunSpider(startingSite, 5);
            CollectionAssert.Contains(listOfSitesVisited, startingSite);

            CollectionAssert.AllItemsAreUnique(listOfSitesVisited);

            Assert.AreEqual(5, listOfSitesVisited.Count);
        }

        [Test]
        public void RunSpider_GivenAStartingWebsiteAnd20Jumps_ReturnAListOfWebsitesVisitedWhichShouldBeUniqueAndHandleCasesWhereAWebsiteIsARedirectByRevertingBackToLastWebsiteAndGoingToNextLink()
        {
            Spider s = new Spider();
            //startingSite = "http://www.giveway.govt.nz";
            var listOfSitesVisited = s.RunSpider(startingSite, 20);
            CollectionAssert.Contains(listOfSitesVisited, startingSite);

            CollectionAssert.AllItemsAreUnique(listOfSitesVisited);
        }

        [Test]
        public void RunSpider_GivenAStartingWebsiteAnd30Jumps_ReturnAListOfVisitedWebsitesThatAreNotLongerThan30CharsSoWeAvoidDeepLinks()
        {
            Spider s = new Spider();
            var listOfSitesVisited = s.RunSpider(startingSite, 100);
            CollectionAssert.Contains(listOfSitesVisited, startingSite);

            CollectionAssert.AllItemsAreUnique(listOfSitesVisited);
        }

        [Test]
        public void RunSpider_GivenAStartingWebsiteAnd100Jumps_ReturnAListOfVisitedWebsitesThatAreNotLongerThan30CharsSoWeAvoidDeepLinks()
        {
            Spider s = new Spider();
            startingSite = "http://www.bbc.co.uk";
            var listOfSitesVisited = s.RunSpider(startingSite, 100);
            CollectionAssert.Contains(listOfSitesVisited, startingSite);

            CollectionAssert.AllItemsAreUnique(listOfSitesVisited);

            foreach (var item in listOfSitesVisited)
            {
                Console.WriteLine(item);
            }
        }

        [Test]
        public void RunSpider_GivenAStartingWebsiteThatIsNotThere404_HandleItByGoingToStuffCoNz()
        {
            Spider s = new Spider();
            startingSite = "http://www.google.com/support/youtube/bin/static.py?p=watch&amp;page=start.cs&amp;hl=en_US ";
            var listOfSitesVisited = s.RunSpider(startingSite, 100);
            CollectionAssert.Contains(listOfSitesVisited, startingSite);

            CollectionAssert.AllItemsAreUnique(listOfSitesVisited);

            foreach (var item in listOfSitesVisited)
            {
                Console.WriteLine(item);
            }
        }

        [Test]
        public void RunSpider_GivenAWebSiteWithNoLinks_HandleItByGoingToStuffCoNz()
        {
            Spider s = new Spider();
            startingSite = "http://www.davemateer.com";
            var listOfSitesVisited = s.RunSpider(startingSite, 100);
            CollectionAssert.Contains(listOfSitesVisited, startingSite);

            CollectionAssert.AllItemsAreUnique(listOfSitesVisited);

            foreach (var item in listOfSitesVisited)
            {
                Console.WriteLine(item);
            }
        }

        [Test]
        public void RunSpider_GivenAWebSiteWithCrow500InernalServerError_HandleItByGoingToStuffCoNz()
        {
            Spider s = new Spider();
            startingSite = "http://www.canterburyquakelive.co.nz/";
            var listOfSitesVisited = s.RunSpider(startingSite, 100);
            CollectionAssert.Contains(listOfSitesVisited, startingSite);

            CollectionAssert.AllItemsAreUnique(listOfSitesVisited);

            foreach (var item in listOfSitesVisited)
            {
                Console.WriteLine(item);
            }
        }


        [Test]
        public void RunSpider_GivenAWebSiteWithMongolia_HandleItByGoingToStuffCoNz()
        {
            Spider s = new Spider();
            startingSite = "http://www.cnn.com";
            var listOfSitesVisited = s.RunSpider(startingSite, 100);
            CollectionAssert.Contains(listOfSitesVisited, startingSite);

            CollectionAssert.AllItemsAreUnique(listOfSitesVisited);

            foreach (var item in listOfSitesVisited)
            {
                Console.WriteLine(item);
            }
        }

    }
}