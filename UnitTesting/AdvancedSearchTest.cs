using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RedditSharp.Things;
using RedditSharp;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Linq.Expressions;
using RedditSharp.Search;
namespace UnitTesting
{

    //q=subreddit:seattlewa+AND+((title:cat+AND+title:dog)+OR+(flair:sports+AND+NOT(+title:seahawks+)))&sort=new


    [TestClass]
    public class AdvancedSearchTest
    {
        [TestCategory("AdvancedSearch"),TestMethod]
        public void BoolPropertyTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => x.IsNsfw;
            string expected = "nsfw:1";

            ISearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        [TestCategory("AdvancedSearch")]
        public void NOT_BoolPropertyTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => !x.IsNsfw;
            string expected = "NOT(+nsfw:1+)";

            ISearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        [TestCategory("AdvancedSearch")]
        public void StringPropertyTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => x.Site == "google.com";
            string expected = "site:google.com";

            ISearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("AdvancedSearch")]
        public void Flipped_StringPropertyTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => "google.com" == x.Site;
            string expected = "site:google.com";

            ISearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        [TestCategory("AdvancedSearch")]
        public void Not_StringPropertyTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => x.Site != "google.com";
            string expected = "NOT(+site:google.com+)";

            ISearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("AdvancedSearch")]
        public void AndTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => x.IsNsfw && x.Site == "google.com";
            string expected = "(+nsfw:1+AND+site:google.com+)";

            ISearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }








        //[TestMethod]
        //public void ValueComparison_BoolPropertyTest()
        //{
        //    //Arrange
        //    Expression<Func<AdvancedSearchFilter, bool>>
        //        expression = x => !x.IsNsfw == true;
        //    string expected = "NOT(+nsfw:1+)";

        //    ISearchFormatter searchFormatter = new DefaultSearchFormatter();

        //    //Act
        //    string actual = searchFormatter.Format(expression);

        //    //Assert
        //    Assert.AreEqual(expected, actual);
        //}


    }
}
