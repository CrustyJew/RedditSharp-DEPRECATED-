using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using RedditSharp.Search;
namespace UnitTesting
{

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

            IAdvancedSearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCategory("AdvancedSearch"), TestMethod]
        public void NOT_BoolPropertyTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => !x.IsNsfw;
            string expected = "NOT(+nsfw:1+)";

            IAdvancedSearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCategory("AdvancedSearch"), TestMethod]
        public void StringPropertyTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => x.Site == "google.com";
            string expected = "site:google.com";

            IAdvancedSearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCategory("AdvancedSearch"), TestMethod]
        public void Flipped_StringPropertyTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => "google.com" == x.Site;
            string expected = "site:google.com";

            IAdvancedSearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCategory("AdvancedSearch"), TestMethod]
        public void Not_StringPropertyTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => x.Site != "google.com";
            string expected = "NOT(+site:google.com+)";

            IAdvancedSearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCategory("AdvancedSearch"), TestMethod]
        public void AndAlsoTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => x.IsNsfw && x.Site == "google.com";
            string expected = "(+nsfw:1+AND+site:google.com+)";

            IAdvancedSearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCategory("AdvancedSearch"), TestMethod]
        public void TwoString_AndAlsoTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => x.Author=="AutoModerator" && x.Site == "google.com";
            string expected = "(+author:AutoModerator+AND+site:google.com+)";

            IAdvancedSearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCategory("AdvancedSearch"), TestMethod]
        public void TwoString_OrElseTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => x.Author == "AutoModerator" || x.Site == "google.com";
            string expected = "(+author:AutoModerator+OR+site:google.com+)";

            IAdvancedSearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCategory("AdvancedSearch"), TestMethod]
        public void NotOrElseTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => !(x.Author == "AutoModerator" || x.Site == "google.com");
            string expected = "NOT(+(+author:AutoModerator+OR+site:google.com+)+)";

            IAdvancedSearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCategory("AdvancedSearch"), TestMethod]
        public void AndNotOrElseTest()
        {
            //Arrange
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x =>  (x.Title != "trump") && !(x.Author == "AutoModerator" || x.Site == "google.com");
            string expected = "(+NOT(+title:trump+)+AND+NOT(+(+author:AutoModerator+OR+site:google.com+)+)+)";

            IAdvancedSearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCategory("AdvancedSearch"), TestMethod]
        public void StringVariablePropertyTest()
        {
            //Arrange
            string title = "meirl";
            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => x.Title == title;
            string expected = "title:meirl";

            IAdvancedSearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCategory("AdvancedSearch"), TestMethod]
        public void ClassVariablePropertyTest()
        {
            //Arrange
            Test t = new Test() { title = "meirl" };

            Expression<Func<AdvancedSearchFilter, bool>>
                expression = x => x.Title == t.title || x.Title == t.Title || x.Title == t.TitleMethod();
            string expected = "(+(+title:meirl+OR+title:meirl+)+OR+title:meirl+)";

            IAdvancedSearchFormatter searchFormatter = new DefaultSearchFormatter();

            //Act
            string actual = searchFormatter.Format(expression);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        //[TestMethod]
        //[TestCategory("AdvancedSearch")]
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

        private class Test
        {
            public string title { get; set; }
            public string Title => $"{title}";
            public string TitleMethod()
            {
                return "meirl";
            }
        }
    }
}
