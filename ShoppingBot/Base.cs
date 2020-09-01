using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Resources;
using System.Threading;

namespace ShoppingBot
{
    class Base
    {
        protected IWebDriver Driver { get; private set; }
        protected WebDriverWait Wait { get; private set; }

        private string siteURL;
        private string desiredItemName;
        private string desiredItemColour;
        private string desiredCategoryName;

        private string fullname;
        private string email;
        private string phone;
        private string address1;
        private string address2;
        private string city;
        private string postcode;

        private string cardNumber;
        private string cardSecurityCode;
        private string cardExpiryMonth;
        private string cardExpiryYear;

        private const string SELECTOR_ID_CATEGORIES = "nav-categories";
        private const string SELECTOR_CLASS_ITEMS_LIST = "turbolink_scroller";
        private const string SELECTOR_CLASS_ITEM = "inner-article";
        private const string SELECTOR_CSS_ADD_TO_BASKET_BTN = "#add-remove-buttons > input";
        private const string SELECTOR_CSS_CHECKOUT_BTN = "#cart > a.button.checkout";

        private const string SELECTOR_ID_FULLNAME_FIELD = "order_billing_name";
        private const string SELECTOR_ID_EMAIL_FIELD = "order_email";
        private const string SELECTOR_ID_PHONE_FIELD = "order_tel";
        private const string SELECTOR_NAME_ADDRESS1_FIELD = "order[billing_address]";
        private const string SELECTOR_NAME_ADDRESS2_FIELD = "order[billing_address_2]";
        private const string SELECTOR_ID_CITY_FIELD = "order_billing_city";
        private const string SELECTOR_ID_POSTCODE_FIELD = "order_billing_zip";
        private const string SELECTOR_CSS_TERMS_CHECKBOX = "#cart-cc > fieldset > p > label > div > ins";

        private const string SELECTOR_ID_CARD_NUMBER_FIELD = "cnb";        
        private const string SELECTOR_ID_CARD_SECURITY_CODE_FIELD = "vval";
        private const string SELECTOR_ID_CARD_EXPIRY_MONTH_FIELD = "credit_card_month";
        private const string SELECTOR_ID_CARD_EXPIRY_YEAR_FIELD = "credit_card_year";
        
        protected void SetUpWebDriver()
        {
            Driver = new ChromeDriver();
        }

        protected void CleanUp()
        {
            Driver.Quit();
        }

        protected void LoadSite()
        {  
            Driver.Navigate().GoToUrl(siteURL);                       
        }

        protected void AssignEnvVariables()
        {
            var rm = new ResourceManager("ShoppingBot.Environment", typeof(App).Assembly);

            siteURL = rm.GetString("siteURL");
            desiredItemName = rm.GetString("desiredItemName");
            desiredItemColour = rm.GetString("desiredItemColour");
            desiredCategoryName = rm.GetString("desiredCategoryName");

            fullname = rm.GetString("fullname");
            email = rm.GetString("email");
            phone = rm.GetString("phone");
            address1 = rm.GetString("address1");
            address2 = rm.GetString("address2");
            city = rm.GetString("city");
            postcode = rm.GetString("postcode");

            cardNumber = rm.GetString("cardNumber");
            cardSecurityCode = rm.GetString("cardSecurityCode");
            cardExpiryMonth = rm.GetString("cardExpiryMonth");
            cardExpiryYear = rm.GetString("cardExpiryYear");
        }

        protected void ClickDesiredCategory()
        {
            var categories = GetWebElements(By.Id(SELECTOR_ID_CATEGORIES), By.TagName("a"));

            var desiredCategoryIndex = -1;

            for (var i = 0; i < categories.Count; i++)
            {
                if (categories[i].Text == desiredCategoryName)
                {
                    desiredCategoryIndex = i;
                }
            }
            
            if (desiredCategoryIndex != -1)
            {
                var desiredCategoryBtn = categories[desiredCategoryIndex];
                ClickElement(desiredCategoryBtn);

                //Using static wait as the page effectively stays the same after changing category from "All" to "Jackets". 
                //The class name of the currently selected category does changes to "current", but that change will occur instantly, before the page animation has time to complete so that wont work as a dynamic wait element
                Thread.Sleep(2000);
            }
            else
            {                
                Console.WriteLine($"***Category not found***");
                throw new NoSuchElementException();
            }            
        }

        protected bool ClickDesiredItem()
        {  
            var itemsList = GetWebElements(By.ClassName(SELECTOR_CLASS_ITEMS_LIST), By.TagName("article"));            

            int desiredItemIndex = -1;
            
            for (var i = 0; i < itemsList.Count; i++)
            {
                var itemName = GetWebElement(itemsList[i], By.ClassName(SELECTOR_CLASS_ITEM), By.TagName("h1")).Text;
                var itemColour = GetWebElement(itemsList[i], By.ClassName(SELECTOR_CLASS_ITEM), By.TagName("p")).Text;

                if (itemName.Contains(desiredItemName) && itemColour.Contains(desiredItemColour))
                {
                    desiredItemIndex = i;
                }
            }

            if(desiredItemIndex != -1)
            {
                ClickElement(itemsList[desiredItemIndex]);
                WaitUntilElementisDisplayed(By.CssSelector(SELECTOR_CSS_ADD_TO_BASKET_BTN));
                return true;
            }
            else
            {
                Console.WriteLine($"***Item not found***");
                return false;
            }
        }

        protected bool CheckItemAvailability()
        {
            try
            {
                var addToBasketBtn = Driver.FindElement(By.CssSelector(SELECTOR_CSS_ADD_TO_BASKET_BTN));                
                return true;

            }
            catch (NoSuchElementException)
            {
                Console.WriteLine($"***Item out of stock***");
                return false;
            }
        }

        protected void ClickAddItemToBasket()
        {    
            var addToBasketBtn = GetWebElement(By.CssSelector(SELECTOR_CSS_ADD_TO_BASKET_BTN));            
            ClickElement(addToBasketBtn);             
        }

        protected void ClickCheckoutNow()
        {
            var checkoutNowBtn = GetWebElement(By.CssSelector(SELECTOR_CSS_CHECKOUT_BTN));
            ClickElement(checkoutNowBtn);

            WaitUntilElementisDisplayed(By.Id(SELECTOR_ID_FULLNAME_FIELD));
        }

        protected void EnterCardDetails()
        {
            var fullnameField = Driver.FindElement(By.Id(SELECTOR_ID_FULLNAME_FIELD));
            var emailField = Driver.FindElement(By.Id(SELECTOR_ID_EMAIL_FIELD));
            var phoneField = Driver.FindElement(By.Id(SELECTOR_ID_PHONE_FIELD));
            var addressField1 = Driver.FindElement(By.Name(SELECTOR_NAME_ADDRESS1_FIELD));
            var addressField2 = Driver.FindElement(By.Name(SELECTOR_NAME_ADDRESS2_FIELD));
            var cityField = Driver.FindElement(By.Id(SELECTOR_ID_CITY_FIELD));
            var postcodeField = Driver.FindElement(By.Id(SELECTOR_ID_POSTCODE_FIELD));
            var termsCheckbox = Driver.FindElement(By.CssSelector(SELECTOR_CSS_TERMS_CHECKBOX));

            var cardNumberField = Driver.FindElement(By.Id(SELECTOR_ID_CARD_NUMBER_FIELD));
            var cardSecurityCodeField = Driver.FindElement(By.Id(SELECTOR_ID_CARD_SECURITY_CODE_FIELD));
            var cardExpiryMonthField = Driver.FindElement(By.Id(SELECTOR_ID_CARD_EXPIRY_MONTH_FIELD));
            var cardExpiryYearField = Driver.FindElement(By.Id(SELECTOR_ID_CARD_EXPIRY_YEAR_FIELD));            


            fullnameField.SendKeys(fullname);
            emailField.SendKeys(email);
            phoneField.SendKeys(phone);
            addressField1.SendKeys(address1);
            addressField2.SendKeys(address2);
            cityField.SendKeys(city);
            postcodeField.SendKeys(postcode);
            termsCheckbox.Click();

            cardNumberField.SendKeys(cardNumber);
            cardSecurityCodeField.SendKeys(cardSecurityCode);
            cardExpiryMonthField.SendKeys(cardExpiryMonth);
            cardExpiryYearField.SendKeys(cardExpiryYear);            

            Thread.Sleep(5000);
        }

        public void ClickElement(IWebElement element)
        {
            GetWaitForXSeconds().Until(d =>
            {
                try
                {
                    element.Click();
                    return true;
                }
                catch (NoSuchElementException e)
                {
                    Console.WriteLine($"CATCH ERROR in ClickElement() - {e.Message}");
                    return false;
                }
                catch (ElementNotInteractableException e)
                {
                    Console.WriteLine($"CATCH ERROR in ClickElement() - {e.Message}");
                    return false;
                }
                catch (StaleElementReferenceException e)
                {
                    Console.WriteLine($"CATCH ERROR in ClickElement() - {e.Message}");
                    return false;
                }
            });
        }

        public IWebElement GetWebElement(By selector)
        {
            IWebElement element = null;
            GetWaitForXSeconds().Until(d =>
            {
                try
                {
                    element = d.FindElement(selector);
                    return true;
                }
                catch (NoSuchElementException e)
                {
                    Console.WriteLine($"CATCH ERROR in GetWebElement() - {e.Message}");
                    return false;
                }
                catch (ElementNotInteractableException e)
                {
                    Console.WriteLine($"CATCH ERROR in GetWebElement() - {e.Message}");
                    return false;
                }
                catch (StaleElementReferenceException e)
                {
                    Console.WriteLine($"CATCH ERROR in GetWebElement() - {e.Message}");
                    return false;
                }
            });

            return element;
        }

        //For this version of GetWebElement(), driver is replaced by baseElement
        public IWebElement GetWebElement(IWebElement baseElement, By selector, By selector2)
        {
            IWebElement element = null;
            GetWaitForXSeconds().Until(d =>
            {
                try
                {
                    element = baseElement.FindElement(selector).FindElement(selector2);
                    return true;
                }
                catch (NoSuchElementException e)
                {
                    Console.WriteLine($"CATCH ERROR in GetWebElement() - {e.Message}");
                    return false;
                }
                catch (ElementNotInteractableException e)
                {
                    Console.WriteLine($"CATCH ERROR in GetWebElement() - {e.Message}");
                    return false;
                }
                catch (StaleElementReferenceException e)
                {
                    Console.WriteLine($"CATCH ERROR in GetWebElement() - {e.Message}");
                    return false;
                }
            });

            return element;
        }

        public ReadOnlyCollection<IWebElement> GetWebElements(By selector, By selector2)
        {
            ReadOnlyCollection<IWebElement> elements = null;

            GetWaitForXSeconds().Until(d =>
            {
                try
                {
                    elements = d.FindElement(selector).FindElements(selector2);
                    return true;
                }
                catch (NoSuchElementException e)
                {
                    Console.WriteLine($"CATCH ERROR in GetWebElement() - {e.Message}");
                    return false;
                }
                catch (ElementNotInteractableException e)
                {
                    Console.WriteLine($"CATCH ERROR in GetWebElement() - {e.Message}");
                    return false;
                }
                catch (StaleElementReferenceException e)
                {
                    Console.WriteLine($"CATCH ERROR in GetWebElement() - {e.Message}");
                    return false;
                }
            });

            return elements;
        }

        public void WaitUntilElementisDisplayed(By selector)
        {
            GetWaitForXSeconds().Until(d =>
            {
                try
                {
                    var element = GetWebElement(selector);
                    return element.Displayed;
                }
                catch (NoSuchElementException)
                {                    
                    return false;
                }
                catch (ElementNotInteractableException)
                {                    
                    return false;
                }
                catch (StaleElementReferenceException)
                {                    
                    return false;
                }
            });
        }

        public WebDriverWait GetWaitForXSeconds(int timeout = 10)
        {
            Wait = new WebDriverWait(Driver, new TimeSpan(0, 0, timeout));
            return Wait;
        }
    }
}
