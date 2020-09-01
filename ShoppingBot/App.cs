using System;
using System.Threading;

namespace ShoppingBot
{
    class App : Base
    {
        public static void Main()
        {
            App app = new App();

            app.SetUpWebDriver();

            app.AssignEnvVariables();

            app.Start(); 
        }          

        public void Start()
        {     
            while(true)
            {            
                LoadSite();

                ClickDesiredCategory();

                bool itemExists = ClickDesiredItem();

                if (itemExists)
                {
                    bool itemAvailable = CheckItemAvailability();                     
                
                    if (itemAvailable)
                    {
                        ClickAddItemToBasket();

                        ClickCheckoutNow();

                        EnterCardDetails();

                        //Add - ClickProcessPayment()

                        break;
                    }
                }

                Thread.Sleep(5000);
            }

            CleanUp();
        }       
    }
}
