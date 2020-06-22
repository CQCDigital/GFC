using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using SYE.Services;
using Xunit;

namespace SYE.Tests.Services
{
    public class PageServiceTests
    {
        //private string _dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Resources\\";
        //private string _fileName = "form-schema.json";
        //private string _location = "Test Location";


        //[Theory]
        //[InlineData(null, "Pos001")]
        //[InlineData("", "Pos001")]
        //[InlineData("Pos001", "Pos001")]
        //[InlineData("Pos101", "Pos101")]
        //[InlineData("Pos004", "Pos004")]
        //[InlineData("Pos005", "Pos005")]
        //[InlineData("Pos006", "Pos006")]
        //public void GetPageById_Should_Return_Correct_Page(string searchPageId, string expectedPageId)
        //{
        //    //arrange
        //    var sut = new PageService();
        //    //act            
        //    //TODO this will need to change to call the method to get from the cache            
        //    var result = sut.GetPageById(searchPageId, _dir + _fileName, "test location");

        //    //assert
        //    result.PageId.Should().Be(expectedPageId);
        //}

        //[Theory]
        //[InlineData(0, true)]
        //[InlineData(1, false)]
        //[InlineData(2, false)]
        //[InlineData(3, false)]
        //[InlineData(4, false)]
        //public void GetPageById_Should_Return_Correct_Next_Page(int pageIndex, bool getPageFromAnswerLogic)
        //{
        //    //arrange
        //    var sut = new PageService();
        //    var happyPathPageList = GenerateHappyPathPageList();
        //    //act            
        //    var thisPageId = happyPathPageList[pageIndex];
        //    var expectedPageId = happyPathPageList[pageIndex + 1];

        //    //TODO this will need to change to call the method to get from the cache
        //    var result = sut.GetPageById(thisPageId, _dir + _fileName, _location);

        //    string next;

        //    if (getPageFromAnswerLogic)
        //    {
        //        next = result.Questions.ToList()[0].AnswerLogic.ToList()[0].NextPageId;
        //    }
        //    else
        //    {
        //        next = result.NextPageId;
        //    }

        //    //assert
        //    next.Should().Be(expectedPageId);
        //}

        //[Theory]
        //[InlineData("Pos007")]
        //[InlineData("xxxxxxxxxxxxxxxxxxxxx")]
        //public void GetPageById_Should_Return_Null_Page(string pageId)
        //{
        //    //arrange
        //    var sut = new PageService();
        //    //act            
        //    //TODO this will need to change to call the method to get from the cache
        //    var result = sut.GetPageById(pageId, _dir + _fileName, "test location");

        //    //assert
        //    result.Should().Be(null);
        //}


        ///// <summary>
        ///// sets up a list of page ids representing the happy path
        ///// </summary>
        //private List<string> GenerateHappyPathPageList()
        //{
        //    var happyPath = new List<string>
        //        {"Pos001", "Pos101", "Pos004", "Pos005", "Pos006", "Pos007"};
        //    return happyPath;
        //}
 
    }
}
