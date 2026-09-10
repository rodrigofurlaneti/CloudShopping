using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace CloudShopping.Tests.E2E.Infrastructure;

public abstract class BrowserTest : IDisposable
{
    protected IWebDriver Browser { get; }
    protected WebDriverWait Wait { get; }
    private readonly Uri baseUrl;
    protected string Unique => "E2E-" + Guid.NewGuid().ToString("N")[..12];
    protected static string Required(string name) => Environment.GetEnvironmentVariable(name) is { Length: >0 } value
        ? value : throw new InvalidOperationException($"Configure {name} para o ambiente local de homologação. Consulte README.md.");

    protected BrowserTest()
    {
        baseUrl=new Uri(Required("E2E_BASE_URL"));
        if(!baseUrl.IsLoopback || baseUrl.Scheme is not ("http" or "https"))
            throw new InvalidOperationException("E2E_BASE_URL deve apontar para localhost nesta fase.");
        if(Required("E2E_ENVIRONMENT")!="Homologacao")
            throw new InvalidOperationException("E2E_ENVIRONMENT deve ser Homologacao; confira a conexão do backend antes de executar.");
        var options=new ChromeOptions();
        if(Environment.GetEnvironmentVariable("E2E_HEADLESS")!="false")options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1440,1000");
        options.AddArgument("--lang=pt-BR");
        if(Environment.GetEnvironmentVariable("E2E_CHROME_BINARY") is { Length: >0 } binary)options.BinaryLocation=binary;
        var driverPath=Environment.GetEnvironmentVariable("E2E_DRIVER_DIRECTORY");
        var service=string.IsNullOrWhiteSpace(driverPath)?ChromeDriverService.CreateDefaultService():ChromeDriverService.CreateDefaultService(driverPath);
        service.HideCommandPromptWindow=true;
        Browser=new ChromeDriver(service,options,TimeSpan.FromSeconds(60));
        Browser.Manage().Timeouts().ImplicitWait=TimeSpan.Zero;
        Browser.Manage().Timeouts().PageLoad=TimeSpan.FromSeconds(30);
        Wait=new WebDriverWait(Browser,TimeSpan.FromSeconds(20));
        Wait.IgnoreExceptionTypes(typeof(NoSuchElementException),typeof(StaleElementReferenceException));
    }
    protected void Open(string path)=>Browser.Navigate().GoToUrl(new Uri(baseUrl,path));
    protected IWebElement Visible(By selector)=>Wait.Until(d=>d.FindElements(selector).FirstOrDefault(e=>e.Displayed));
    protected void Click(By selector)=>Wait.Until(d=> {var e=d.FindElements(selector).FirstOrDefault(x=>x.Displayed&&x.Enabled);if(e==null)return false;e.Click();return true;});
    protected void Fill(By selector,string value){var input=Visible(selector);input.Clear();input.SendKeys(value);}
    protected static By Button(string text)=>By.XPath($"//button[normalize-space(.)={Literal(text)}]");
    protected static By Label(string text)=>By.XPath($"//label[starts-with(normalize-space(.),{Literal(text)})]//*[self::input or self::textarea or self::select]");
    protected static string Literal(string value)=>!value.Contains('\'')?"'"+value+"'":!value.Contains('"')?"\""+value+"\"":throw new ArgumentException("Unsupported selector literal");
    protected void Contains(string text)=>Wait.Until(d=>d.FindElement(By.TagName("body")).Text.Contains(text,StringComparison.Ordinal));
    protected void At(string path)=>Wait.Until(d=>new Uri(d.Url).AbsolutePath==path);
    protected void Reload()=>Browser.Navigate().Refresh();
    protected void AdminLogin()
    {
        Open("/admin/login");Fill(By.CssSelector("input[placeholder='Seu usuário']"),Required("E2E_ADMIN_USERNAME"));
        Fill(By.CssSelector("input[type='password']"),Required("E2E_ADMIN_PASSWORD"));
        Click(Button("Entrar no Backoffice"));At("/admin/dashboard");
    }
    protected string RegisterCustomer()
    {
        var email=Unique.ToLowerInvariant()+"@example.test";
        Open("/account");Click(Button("Quero criar uma conta"));
        Fill(By.Name("email"),email);Fill(By.Name("password"),"E2E-local-"+Guid.NewGuid().ToString("N")+"!aA1");
        Click(Button("Criar conta"));At("/cart");return email;
    }
    protected void AddProduct()
    {
        var id=Required("E2E_PRODUCT_ID");
        if(!int.TryParse(id,out var number)||number<=0)throw new InvalidOperationException("E2E_PRODUCT_ID deve ser um produto de teste com estoque >= 3.");
        Open("/product/"+id);Click(Button("Adicionar ao carrinho"));Contains("Produto adicionado.");
    }
    // Evidence is opt-in: rendered pages may contain homologation customer information.
    protected void Scenario(Action action)
    {
        try{action();}catch{
            if(Environment.GetEnvironmentVariable("E2E_CAPTURE_FAILURES")=="true")
            {try{var folder=Path.Combine(AppContext.BaseDirectory,"TestResults","screenshots");Directory.CreateDirectory(folder);
                ((ITakesScreenshot)Browser).GetScreenshot().SaveAsFile(Path.Combine(folder,Guid.NewGuid()+".png"));}catch{/* Preserve original failure. */}}
            throw;
        }
    }
    public void Dispose(){try{Browser.Quit();}finally{Browser.Dispose();}}
}
