using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace WpfTest
{

    [TestFixture]
    public class SpiderTests
    {
        string startingSite = "http://www.stuff.co.nz";
        Spider s;

        [SetUp]
        public void setup()
        {
            s = new Spider();
        }

        //simple tests/spikes to help build app
        //names of tests: method being tested....given....then it
        //methods are arranged loosly as: Arrange, Act, Assert
        [Test]
        public void GetHtml_GivenAWebsite_ReturnsTheRawHtml()
        {
            string result = s.GetHtml(startingSite);

            Assert.IsNotNullOrEmpty(result);
            Console.WriteLine(result);
        }

        [Test]
        public void GetAllAbsoluteLinks_GivenHtml_ReturnsACollectionOfAllHttpAbsoluteLinks()
        {
            string html = s.GetHtml(startingSite);
            List<string> listOfAbsoluteLinks = s.GetAllAbsoluteLinks(html);

            CollectionAssert.IsNotEmpty(listOfAbsoluteLinks);
            foreach (var link in listOfAbsoluteLinks)
                Console.WriteLine(link);
        }

        [Test]
        public void GetExternalLinks_GivenAListOfLinks_ReturnsACollectionOfOnlyExternalLinks()
        {
            string html = s.GetHtml(startingSite);
            List<string> listOfAbsoluteLinks = s.GetAllAbsoluteLinks(html);
            List<string> listOfExternalLinks = s.GetExternalLinks(listOfAbsoluteLinks, startingSite);

            CollectionAssert.IsNotEmpty(listOfExternalLinks);
            foreach (var link in listOfExternalLinks)
                Console.WriteLine(link);
        }

        //first working spider with single jump
        [Test]
        public void GetExternalLinks_GivenAListOfExternalLinks_GetHtmlForFirstLink()
        {
            string html = s.GetHtml(startingSite);
            List<String> listOfLinks = s.GetAllAbsoluteLinks(html);
            List<String> listOfExternalLinks = s.GetExternalLinks(listOfLinks, startingSite);

            Console.WriteLine("Going to: " + listOfExternalLinks[0].ToString());
            html = s.GetHtml(listOfExternalLinks[0]);

            Assert.IsNotNullOrEmpty(html);
            Console.WriteLine(html);
        }


        //first working spider with multi jump
        //code refactored for WPF so runspider changed to yield return as it goes along
        //otherwise the ui would have to wait until the entire spider completed before displaying anything
        //and we don't want the spider to be controlling the ui....like it is here.. putting messages to the console :-)
        [Test]
        public void RunSpiderGetNext_GivenAStartingWebsiteAnd5Jumps_ReturnAListOfWebsitesVisitedWhichShouldBeUnique()
        {
            IEnumerable<WebPageInfo> listOfWebPages = s.RunSpiderGetNext(startingSite, 50);

            var listOfWebPagesVisited = new List<string>();
            foreach (var webPage in listOfWebPages)
            {
                listOfWebPagesVisited.Add(webPage.Uri);
                Console.WriteLine(webPage.Uri);
                Console.WriteLine(webPage.Messages);
            }

            CollectionAssert.Contains(listOfWebPagesVisited, startingSite);
            CollectionAssert.AllItemsAreUnique(listOfWebPagesVisited);
            Assert.AreEqual(5, listOfWebPagesVisited.Count());
        }

        [Test]
        public void RunspiderGetNext_WhenLinksFail_ReturnMessageToDisplayUPTREE()
        {
            IEnumerable<WebPageInfo> listOfWebPages = s.RunSpiderGetNext(startingSite, 50);

            var listOfWebPagesVisited = new List<string>();
            foreach (var webPage in listOfWebPages)
            {
                listOfWebPagesVisited.Add(webPage.Uri);
                Console.WriteLine(webPage.Uri);
                Console.WriteLine(webPage.Messages);
            }

            CollectionAssert.Contains(listOfWebPagesVisited, startingSite);
            CollectionAssert.AllItemsAreUnique(listOfWebPagesVisited);
        }

        [Test]
        public void RunSpiderGetNext_WhenUpTree_ShouldGoThoughSameBusinessLogicAndNotGoToGoogleOrBlackList()
        {
            
        }

        //edge cases
        //redirect or no links on a page
        [Test]
        public void RunSpiderGetNext_GivenAStartingWebsiteAnd5Jumps_ReturnAListOfWebsitesVisitedWhichShouldBeUniqueAndHandleCasesWhereAWebsiteIsARedirectByRevertingBackToLastWebsiteAndGoingToNextLink()
        {
            //trying to find a site that has a link in it to www.codecamp.co.nz which is a redirect
            startingSite = "http://alpha.mateerit.co.nz/links.htm";
            IEnumerable<WebPageInfo> listOfWebPages = s.RunSpiderGetNext(startingSite, 5);

            var listOfWebPagesVisited = new List<string>();
            foreach (var webPage in listOfWebPages)
            {
                listOfWebPagesVisited.Add(webPage.Uri);
                Console.WriteLine(webPage.Uri);
                Console.WriteLine(webPage.Messages);
            }

            CollectionAssert.Contains(listOfWebPagesVisited, startingSite);
            CollectionAssert.AllItemsAreUnique(listOfWebPagesVisited);
        }

        //links which are not too large
        [Test]
        public void RunSpiderGetNext_GivenAStartingWebsiteAnd20Jumps_ReturnAListOfVisitedWebsitesThatAreNotLongerThan50CharsSoWeAvoidDeepLinks()
        {
            IEnumerable<WebPageInfo> listOfWebPages = s.RunSpiderGetNext(startingSite, 20);

            var listOfWebPagesVisited = new List<string>();
            foreach (var webPage in listOfWebPages)
            {
                listOfWebPagesVisited.Add(webPage.Uri);
                Console.WriteLine(webPage.Uri);
                Console.WriteLine(webPage.Messages);
            }

            CollectionAssert.Contains(listOfWebPagesVisited, startingSite);
            CollectionAssert.AllItemsAreUnique(listOfWebPagesVisited);
        }

        //edge case 404
        [Test]
        public void RunSpiderGetNext_GivenAStartingWebsiteThatIsNotThere404_HandleItByThrowingAnException()
        {
            startingSite = "http://www.google.com/support/youtube/bin/static.py?p=watch&amp;page=start.cs&amp;hl=en_US ";
            IEnumerable<WebPageInfo> listOfWebPages = s.RunSpiderGetNext(startingSite, 100);

            var listOfWebPagesVisited = new List<string>();
            foreach (var webPage in listOfWebPages)
            {
                listOfWebPagesVisited.Add(webPage.Uri);
                Console.WriteLine(webPage.Uri);
                Console.WriteLine(webPage.Messages);
            }

            CollectionAssert.Contains(listOfWebPagesVisited, startingSite);
            CollectionAssert.AllItemsAreUnique(listOfWebPagesVisited);
        }

        //TODO assert exception
        [Test]
        public void RunSpiderGetNext_GivenAnInitialWebSiteWithNoLinks_HandleItByThrowingAnException()
        {
            startingSite = "http://www.davemateer.com";
            IEnumerable<WebPageInfo> listOfWebPages = s.RunSpiderGetNext(startingSite, 100);

            var listOfWebPagesVisited = new List<string>();
            foreach (var webPage in listOfWebPages)
            {
                listOfWebPagesVisited.Add(webPage.Uri);
                Console.WriteLine(webPage.Uri);
                Console.WriteLine(webPage.Messages);
            }

            CollectionAssert.Contains(listOfWebPagesVisited, startingSite);
            CollectionAssert.AllItemsAreUnique(listOfWebPagesVisited);
        }

        //500 error
        //TODO
        [Test]
        public void RunSpiderGetNext_GivenAWebSiteWithCrow500InernalServerError_HandleItByGoingToStuffCoNz()
        {
            startingSite = "http://www.canterburyquakelive.co.nz/";
            IEnumerable<WebPageInfo> listOfWebPages = s.RunSpiderGetNext(startingSite, 100);

            var listOfWebPagesVisited = new List<string>();
            foreach (var webPage in listOfWebPages)
            {
                listOfWebPagesVisited.Add(webPage.Uri);
                Console.WriteLine(webPage.Uri);
                Console.WriteLine(webPage.Messages);
            }

            CollectionAssert.Contains(listOfWebPagesVisited, startingSite);
            CollectionAssert.AllItemsAreUnique(listOfWebPagesVisited);
        }


        [Test]
        public void RunSpiderGetNext_GivenANonResponsiveWebsite_HandleItByTimingOutGracefuly()
        {
            
        }

        [Test]
        public void RunSpider_GivenCNNSite_LookForJustinBeaber()
        {
            startingSite = "http://www.cnn.com";
            IEnumerable<WebPageInfo> listOfWebPages = s.RunSpiderGetNext(startingSite, 100);

            var listOfWebPagesVisited = new List<string>();
            foreach (var webPage in listOfWebPages)
            {
                listOfWebPagesVisited.Add(webPage.Uri);
                Console.WriteLine(webPage.Uri);
                Console.WriteLine(webPage.Messages);
            }

            CollectionAssert.Contains(listOfWebPagesVisited, startingSite);
            CollectionAssert.AllItemsAreUnique(listOfWebPagesVisited);
        }


        //get next, yield return
        //returning object
       

        //WPF testing helpers
        [Test]
        public void GetHtml_GivenAWebsite_ReturnsTheRawHtmlAndRedersInWPFControl()
        {
            Spider s = new Spider();

            string result = s.GetHtml(startingSite);

            Assert.IsNotNullOrEmpty(result);
            Console.WriteLine(result);
        }



    }
}
