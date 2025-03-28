using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

public class WebDriverNavigator : Form
{
    // Session management
    private IWebDriver _driver;
    private string _sessionId;
    private bool _sessionActive = false;

    // UI Controls
    private RichTextBox _logTextBox;
    private TreeView _elementTree;
    private Label _sessionInfo;

    // Buttons
    private Button _startSessionBtn;
    private Button _refreshElementsBtn;
    private Button _loginBtn;
    private Button _truckBtn;
    private Button _openBtn;

    public WebDriverNavigator()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        Utils.WebDriverExtensions.LogAction = (message, color) => Log(message, color);

        this.Text = "WebDriver GUI Navigator";
        this.Size = new Size(1000, 800);

        // Main controls panel
        var controlsPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            Padding = new Padding(10)
        };

        // Session controls
        _startSessionBtn = new Button
        {
            Text = "Start Session",
            Location = new Point(10, 10),
            Size = new Size(120, 30)
        };
        _startSessionBtn.Click += StartSession_Click;

        _refreshElementsBtn = new Button
        {
            Text = "Refresh Elements",
            Location = new Point(140, 10),
            Size = new Size(120, 30)
        };
        _refreshElementsBtn.Click += RefreshElements_Click;


        // Action buttons
        // Action buttons row 1
        _loginBtn = new Button
        {
            Text = "Login",
            Location = new Point(490, 10),
            Size = new Size(80, 30)
        };
        _loginBtn.Click += Login_Click;

        _truckBtn = new Button
        {
            Text = "Truck",
            Location = new Point(580, 10),
            Size = new Size(80, 30)
        };
        _truckBtn.Click += gate_Click;

        _openBtn = new Button
        {
            Text = "Open",
            Location = new Point(670, 10),
            Size = new Size(80, 30)
        };
        _openBtn.Click += Open_Click;

        // Additional buttons (2 rows of 6)
        var button2 = new Button { Text = "Conteiner Management", Location = new Point(10, 50), Size = new Size(100, 30) };
        button2.Click += (s, e) => { OpenMenu("Management"); };


        // Log panel
        // Log panel
        _logTextBox = new RichTextBox
        {
            Dock = DockStyle.Bottom,
            Height = 150,
            Multiline = true,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            ReadOnly = true
        };
        // Session info
        _sessionInfo = new Label
        {
            Dock = DockStyle.Top,
            Text = "Session not started",
            Padding = new Padding(10),
            Height = 40,
            BackColor = SystemColors.ControlLight
        };

        // Element tree
        _elementTree = new TreeView
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            ShowNodeToolTips = true
        };

        // Add controls to panels
        controlsPanel.Controls.AddRange(new Control[] {
            _startSessionBtn, _refreshElementsBtn, _loginBtn, _truckBtn, _openBtn,
            button2
        });

        this.Controls.AddRange(new Control[] {
            _elementTree, _sessionInfo, controlsPanel, _logTextBox
        });

    }

    public class FlaUIDriverOptions : AppiumOptions
    {
        public static FlaUIDriverOptions ForApp(string path)
        {
            var options = new FlaUIDriverOptions()
            {
                PlatformName = "windows",
                AutomationName = "flaui",
                App = path
            };
            return options;
        }

    }


    private async void StartSession_Click(object sender, EventArgs e)
    {
        Log("Starting new session...");
        try
        {
            var opt = FlaUIDriverOptions.ForApp("C:\\Users\\Beltzac\\Desktop\\DIS\\CM.CTOS.WinUIAdmin.exe");
            _driver = new WindowsDriver(new Uri("http://192.168.56.56:4723/"), opt);





            _sessionActive = true;

            _sessionInfo.Text = $"Session ID: {_sessionId}";
            _startSessionBtn.Enabled = false;
            _refreshElementsBtn.Enabled = true;
            Log("Session started successfully", Color.Green);

        }
        catch (Exception ex)
        {
            _sessionInfo.Text = $"Error: {ex.Message}";
            Log($"Session start failed: {ex.Message}");
        }
    }


    private async Task BuildElementTree()
    {

        try
        {
            _elementTree.Nodes.Clear();
            var elements = _driver.FindElements(By.XPath("//*"));

            foreach (var element in elements)
            {
                try
                {
                    var location = element.Location;
                    var size = element.Size;
                    var enabled = element.Enabled;
                    var name = element.GetDomAttribute("Name");
                    var tagName = element.TagName;

                    var node = new TreeNode($"{name ?? "Unnamed"} ({tagName}) - {element.GetAttribute("Value") ?? element.Text ?? "No value"}")
                    {
                        Tag = element,
                        ToolTipText = $"ID: {element}\n" +
                                      $"Value: {element.GetAttribute("Value") ?? element.Text ?? "None"}\n" +
                                      $"Position: ({location.X}, {location.Y})\n" +
                                      $"Size: {size.Width}x{size.Height}\n" +
                                      $"Enabled: {enabled}\n" +
                                      $"Visible: {element.Displayed}\n" +
                                      $"Selected: {element.Selected}"
                    };

                    // Add action buttons as child nodes
                    var clickNode = new TreeNode("Click") { Tag = new { Action = "click", Element = element } };
                    var setValueNode = new TreeNode("Set Value") { Tag = new { Action = "setValue", Element = element } };

                    node.Nodes.Add(clickNode);
                    node.Nodes.Add(setValueNode);

                    _elementTree.Nodes.Add(node);
                }
                catch (StaleElementReferenceException)
                {
                    continue;
                }
            }

            // Add event handler for node clicks
            _elementTree.NodeMouseClick += async (sender, e) =>
            {
                if (e.Node?.Tag is { } tag && tag.GetType().GetProperty("Action") != null)
                {
                    dynamic actionInfo = tag;
                    var element = (IWebElement)actionInfo.Element;
                    var action = (string)actionInfo.Action;

                    try
                    {
                        if (action == "click")
                        {
                            element.Click();
                            e.Node.BackColor = Color.LightGreen;
                        }
                        else if (action == "setValue")
                        {
                            var value = Microsoft.VisualBasic.Interaction.InputBox("Enter value:", "Set Value", "");
                            if (!string.IsNullOrEmpty(value))
                            {
                                element.SendKeys(value);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"ERROR: Error performing action: {ex.Message}", Color.Red);
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _elementTree.Nodes.Add(new TreeNode($"Error loading elements: {ex.Message}"));
        }
    }

    private async void RefreshElements_Click(object sender, EventArgs e)
    {
        await BuildElementTree();
        Log("Elements refreshed successfully", Color.Green);
    }

    private void Open_Click(object sender, EventArgs e)
    {
        OpenMenu("Conteiner Size", "Size");
    }

    private void OpenMenu(string menu, string titulo = null)
    {
        try
        {
            titulo ??= menu;

            Log("Abrindo menu: " + menu);

            _driver.SwitchTo().ActiveElement().SendKeys(OpenQA.Selenium.Keys.Control + "o");

            new Actions(_driver)
                .SendKeys(menu)
                .SendKeys(OpenQA.Selenium.Keys.ArrowDown)
                .SendKeys(OpenQA.Selenium.Keys.Enter)
                .SendKeys(OpenQA.Selenium.Keys.Enter)
                .SendKeys(OpenQA.Selenium.Keys.Enter)
                .Perform();

            Log("Aguardando menu abrir: " + menu);

            new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                .Until(d => _driver.Title.ToUpper().Contains(menu.ToUpper()));

            if (!_driver.Title.ToUpper().Contains(menu.ToUpper()))
                throw new Exception($"Janela {_driver.Title} diferente da janela esperada {titulo}");

            Log("Oppened: " + _driver.Title);


            Log("Open operation completed successfully", Color.Green);

        }
        catch (Exception ex)
        {
            Log($"ERROR: Error sending Ctrl+O: {ex.Message}", Color.Red);
        }
    }

    private async void gate_Click(object sender, EventArgs e)
    {
        OpenMenu("Truck Transaction");
    }

    private void Login_Click(object sender, EventArgs e)
    {
        try
        {
            var win = _driver.WindowHandles;
            _driver.SwitchTo().Window(win.First());

            // Find elements with timeout
            var userNameField = _driver.FindElement(By.Id("txtUserName"), 10);
            var passwordField = _driver.FindElement(By.Id("txtPassword"), 10);
            var okButton = _driver.FindElement(By.Id("btnOK"), 10);

            // Wait for and interact with username field
            userNameField.WaitForElement(_driver);
            userNameField.Click();
            var username = "ahoy.abeltzac";
            userNameField.SendKeys(username);
            Log("Entered username", Color.Blue);

            // Wait for and interact with password field
            passwordField.WaitForElement(_driver);
            passwordField.SendKeys("Hunt93cexx33");
            Log("Entered password", Color.Blue);

            // Wait for and click OK button
            okButton.WaitForElement(_driver);
            okButton.Click();
            Log("Clicked OK button", Color.Blue);

            // Verify login success

            // Esperar nova tela
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                .Until(d => _driver.WindowHandles.Count > 0);

            // Mudar para a nova tela
            _driver.SwitchTo().Window(_driver.WindowHandles.First());

            Log("Oppened: " + _driver.Title);

            var janelaEsperada = "CTOS DIS";
            if (_driver.Title != janelaEsperada)
                throw new Exception($"Janela {_driver.Title} diferente da janela esperada {janelaEsperada}");

            Log("Login successful", Color.Green);

        }
        catch (Exception ex)
        {
            Log($"ERROR: Login failed: {ex.Message}", Color.Red);
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new WebDriverNavigator());
    }

    private void Log(string message, Color? color = null)
    {
        if (_logTextBox != null)
        {
            _logTextBox.SelectionStart = _logTextBox.TextLength;
            if (color != null)
            {
                _logTextBox.SelectionColor = color.Value;
            }
            _logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            _logTextBox.SelectionColor = _logTextBox.ForeColor;
            _logTextBox.ScrollToCaret();
        }
    }
}