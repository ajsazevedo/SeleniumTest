using System;
using TechTalk.SpecFlow;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Linq;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using NUnit.Framework;

namespace Amazon.Steps
{
  [Binding]
  public class BookTitleCheckSteps
  {
    IWebDriver driver;
    string lowestPriceBookTitle = string.Empty;

    [Given(@"I navigate to “www\.amazon\.com”\.")]
    public void GivenINavigateToWww_Amazon_Com_()
    {
      //Can use other drivers...
      driver = new ChromeDriver
      {
        Url = "http://www.amazon.com"
      };
      driver.Navigate().GoToUrl(driver.Url);
    }

    [When(@"I select the option “Books” in the dropdown next to the search text input criteria\.")]
    public void WhenISelectTheOptionBooksInTheDropdownNextToTheSearchTextInputCriteria_()
    {
      var searchDropDown = driver.FindElement(By.Id("searchDropdownBox"));
      var selectElement = new SelectElement(searchDropDown);
      selectElement.SelectByText("Books");
    }

    [Then(@"I search for “Test automation”\.")]
    public void ThenISearchForTestAutomation_()
    {
      driver.FindElement(By.Id("twotabsearchtextbox")).SendKeys("Test automation");
      driver.FindElement(By.ClassName("nav-input")).Click();
    }

    [Then(@"I select the cheapest book of the page without using any sorting method available\.")]
    public void ThenISelectTheCheapestBookOfThePageWithoutUsingAnySortingMethodAvailable_()
    {
      var bookList = driver.FindElements(By.XPath("//*[contains(@class, 's-result-item')]"));

      int count = 0;
      decimal lowestPrice = 0;
      IWebElement lowestPriceBook = null;

      foreach (IWebElement w in bookList)
      {
        IWebElement bookPrice = w.FindElement(By.ClassName("a-price-whole"));
        IWebElement bookPriceFraction = w.FindElement(By.ClassName("a-price-fraction"));
        
        var p = $"{bookPrice.Text}.{bookPriceFraction.Text}";
        var price = GetMoney(p);

        if (count == 0)
        {
          lowestPrice = price;
          lowestPriceBook = w.FindElement(By.CssSelector("a.a-link-normal.a-text-normal"));
        }
        else if (price < lowestPrice)
        {
          lowestPrice = price;
          lowestPriceBook = w.FindElement(By.CssSelector("a.a-link-normal.a-text-normal")); //w.FindElement(By.XPath("//*[contains(@class, 'a-link-normal a-text-normal')]"));
        }
        count++;
      }

      if (lowestPriceBook != null)
      {
        lowestPriceBookTitle = lowestPriceBook.Text;
        var bookUrl = lowestPriceBook.GetAttribute("href");
        driver.Navigate().GoToUrl(bookUrl);
      }
      else
        throw new Exception("Book not found!");     
    }

    [When(@"I reach the detailed book page, I check if the name in the header is the same name of the book that I select previously\.")]
    public void WhenIReachTheDetailedBookPageICheckIfTheNameInTheHeaderIsTheSameNameOfTheBookThatISelectPreviously_(/*string previousName*/)
    {
      Assert.That(lowestPriceBookTitle == driver.FindElement(By.XPath("//*[contains(@id, 'roductTitle')]")).Text);
      driver.Quit();
    }

    private decimal GetMoney(string money)
    {
      decimal decval;
      bool convt = decimal.TryParse(money, NumberStyles.Currency,
        CultureInfo.CurrentCulture.NumberFormat, out decval);
      if (convt)
        return decval;
      else
        throw new Exception("Value not correct");
    }
  }
}
