using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
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
    private TextBox _usernameInput;
    private TextBox _passwordInput;
    private TextBox _xpathInput;

    // Buttons
    private Button _startSessionBtn;
    private Button _refreshElementsBtn;
    private Button _loginBtn;
    private Button _truckBtn;
    private Button _openBtn;
    private Button _containerManagementBtn; // Renamed button2 for clarity
    private Button _showcaseSequenceBtn; // New button

    public WebDriverNavigator()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        Utils.WebDriverExtensions.LogAction = (message, color) => Log(message, color);

        this.Text = "POC - Automação CTOS - Selenium + FlaUI WebDriver";
        this.Size = new Size(1000, 800);
        this.Padding = new Padding(10); // Add padding to the form

        // --- Top Panel for Session, Refresh, XPath ---
        var topPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true, // Adjust height automatically
            WrapContents = false, // Keep controls in a single row
            Padding = new Padding(0, 0, 0, 10) // Add padding below
        };

        _startSessionBtn = new Button { Text = "Iniciar Sessão", Size = new Size(120, 30), Margin = new Padding(5) };
        _startSessionBtn.Click += StartSession_Click;

        _refreshElementsBtn = new Button { Text = "Atualizar Elementos", Size = new Size(120, 30), Margin = new Padding(5) };
        _refreshElementsBtn.Click += RefreshElements_Click;

        var xpathLabel = new Label { Text = "XPath:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(10, 8, 0, 0) }; // Align vertically
        _xpathInput = new TextBox { Text = "//*", Width = 200, Margin = new Padding(5) }; // Increased width

        topPanel.Controls.AddRange(new Control[] { _startSessionBtn, _refreshElementsBtn, xpathLabel, _xpathInput });

        // --- Middle Panel for Actions and Login ---
        var middlePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true, // Allow wrapping if needed
            Padding = new Padding(0, 0, 0, 10)
        };

        _loginBtn = new Button { Text = "Login", Size = new Size(80, 30), Margin = new Padding(5) };
        _loginBtn.Click += Login_Click;

        _truckBtn = new Button { Text = "Truck Transaction", Size = new Size(120, 30), Margin = new Padding(5) };
        _truckBtn.Click += gate_Click;

        _openBtn = new Button { Text = "Conteiner Size", Size = new Size(140, 30), Margin = new Padding(5) };
        _openBtn.Click += Open_Click;

        _containerManagementBtn = new Button { Text = "Conteiner Management", Size = new Size(200, 30), Margin = new Padding(5) };
        _containerManagementBtn.Click += (s, e) => { OpenMenu("Management", "Management[CI002]"); };

        // --- New Showcase Button ---
        _showcaseSequenceBtn = new Button { Text = "Showcase Sequence", Size = new Size(150, 30), Margin = new Padding(5) };
        _showcaseSequenceBtn.Click += ShowcaseSequence_Click; // Assign new handler

        middlePanel.Controls.AddRange(new Control[] { _loginBtn, _truckBtn, _openBtn, _containerManagementBtn, _showcaseSequenceBtn });

        // --- Right Panel for Credentials ---
        var credentialsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown, // Stack vertically
            Dock = DockStyle.Right, // Align to the right
            AutoSize = true,
            Width = 200, // Fixed width for alignment
            Padding = new Padding(10, 0, 0, 0) // Add left padding
        };

        var usernameLabel = new Label { Text = "Usuário:", AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
        _usernameInput = new TextBox { Width = 180, Margin = new Padding(0, 0, 0, 5) }; // Adjust width

        var passwordLabel = new Label { Text = "Senha:", AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
        _passwordInput = new TextBox { Width = 180, UseSystemPasswordChar = true, Margin = new Padding(0, 0, 0, 5) };

        credentialsPanel.Controls.AddRange(new Control[] { usernameLabel, _usernameInput, passwordLabel, _passwordInput });

        // --- Log Panel ---
        _logTextBox = new RichTextBox
        {
            Dock = DockStyle.Bottom,
            Height = 300,
            Multiline = true,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            ReadOnly = true
        };

        // --- Element Tree ---
        _elementTree = new TreeView
        {
            Dock = DockStyle.Fill, // Fill remaining space
            BorderStyle = BorderStyle.FixedSingle,
            ShowNodeToolTips = true
        };

        // --- Session Info Label (Optional, can be placed elsewhere or removed) ---
        _sessionInfo = new Label { Text = "Sessão não iniciada", Dock = DockStyle.Bottom, TextAlign = ContentAlignment.MiddleLeft, Height = 20 };


        // --- Add Panels to Form ---
        // Order matters for docking: Bottom and Right first, then Top, then Fill
        this.Controls.Add(_sessionInfo); // Add session info label first at the bottom
        this.Controls.Add(_logTextBox);
        this.Controls.Add(credentialsPanel); // Add right panel before top/fill
        this.Controls.Add(middlePanel);      // Add middle panel
        this.Controls.Add(topPanel);         // Add top panel
        this.Controls.Add(_elementTree);     // Add tree view to fill remaining space
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
        Log("Iniciando nova sessão...");
        try
        {
            var opt = FlaUIDriverOptions.ForApp("C:\\Users\\Beltzac\\Desktop\\DIS\\CM.CTOS.WinUIAdmin.exe");
            _driver = new WindowsDriver(new Uri("http://192.168.56.56:4723/"), opt);

            _sessionActive = true;
            _sessionInfo.Text = $"ID da Sessão: {_sessionId}";
            Log("Sessão iniciada com sucesso", Color.Green);

        }
        catch (Exception ex)
        {
            _sessionInfo.Text = $"Erro: {ex.Message}";
            Log($"Falha ao iniciar sessão: {ex.Message}");
        }
    }


    private void BuildElementTree(string xpath)
    {

        try
        {
            _elementTree.Nodes.Clear();

            Log("Construindo árvore de elementos com XPath: " + xpath);

            var elements = _driver.FindElements(By.XPath(xpath));

            Log($"Encontrado {elements.Count} elementos");

            foreach (var element in elements)
            {
                try
                {
                    var location = element.Location;
                    var size = element.Size;
                    var enabled = element.Enabled;
                    var name = element.GetDomAttribute("Name");
                    var tipo = element.GetDomAttribute("ControlType");

                    var node = new TreeNode($"{name ?? "Sem nome"} ({tipo}) - {element.GetAttribute("Value") ?? element.Text ?? "Sem valor"}")
                    {
                        Tag = element,
                    };

                    // Add action buttons as child nodes
                    var clickNode = new TreeNode("Clicar") { Tag = new { Action = "click", Element = element } };
                    var setValueNode = new TreeNode("Definir Valor") { Tag = new { Action = "setValue", Element = element } };
                    var detailsNode = new TreeNode("Detalhes") { Tag = new { Action = "showDetails", Element = element, Loaded = false } }; // Add details node

                    node.Nodes.Add(clickNode);
                    node.Nodes.Add(setValueNode);
                    node.Nodes.Add(detailsNode); // Add details node to parent

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
                            var value = Microsoft.VisualBasic.Interaction.InputBox("Digite o valor:", "Definir Valor", "");
                            if (!string.IsNullOrEmpty(value))
                            {
                                element.SendKeys(value);
                                e.Node.BackColor = Color.LightYellow; // Indicate value was set
                            }
                        }
                        else if (action == "showDetails" && !(bool)actionInfo.Loaded) // Check if details need loading
                        {
                            e.Node.Nodes.Clear(); // Clear placeholder if any
                            try
                            {
                                //https://github.com/FlaUI/FlaUI.WebDriver
                                //https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-automation-element-propids
                                // Fetch details on demand
                                var location = element.Location;
                                var size = element.Size;
                                var enabled = element.Enabled;
                                var displayed = element.Displayed;
                                var selected = element.Selected;
                                var tipo = element.TagName;
                                var valueAttr = element.GetAttribute("Value");
                                var name = element.GetDomAttribute("Name");
                                var tipo2 = element.GetDomAttribute("ControlType");
                                var text = element.Text;

                                e.Node.Nodes.Add(new TreeNode($"ID: {element}"));
                                e.Node.Nodes.Add(new TreeNode($"Name: {name}"));
                                e.Node.Nodes.Add(new TreeNode($"Valor: {valueAttr ?? text ?? "Nenhum"}"));
                                e.Node.Nodes.Add(new TreeNode($"Tipo: {tipo}"));
                                e.Node.Nodes.Add(new TreeNode($"Tipo 2: {tipo2}"));
                                e.Node.Nodes.Add(new TreeNode($"Posição: ({location.X}, {location.Y})"));
                                e.Node.Nodes.Add(new TreeNode($"Tamanho: {size.Width}x{size.Height}"));
                                e.Node.Nodes.Add(new TreeNode($"Habilitado: {enabled}"));
                                e.Node.Nodes.Add(new TreeNode($"Visível: {displayed}"));
                                e.Node.Nodes.Add(new TreeNode($"Selecionado: {selected}"));

                                // Mark as loaded
                                e.Node.Tag = new { Action = "showDetails", Element = element, Loaded = true };
                                e.Node.Expand(); // Expand to show details
                            }
                            catch (Exception detailEx)
                            {
                                e.Node.Nodes.Add(new TreeNode($"Erro ao carregar detalhes: {detailEx.Message}"));
                                Log($"ERRO: Falha ao obter detalhes do elemento: {detailEx.Message}", Color.Red);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"ERRO: Erro ao executar ação '{action}': {ex.Message}", Color.Red);
                        e.Node.BackColor = Color.LightCoral; // Indicate error
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _elementTree.Nodes.Add(new TreeNode($"Erro ao carregar elementos: {ex.Message}"));
        }
    }

    private async void RefreshElements_Click(object sender, EventArgs e)
    {
        try
        {
            var xpath = string.IsNullOrWhiteSpace(_xpathInput.Text) ? "//*" : _xpathInput.Text;
            BuildElementTree(xpath);
            Log($"Elementos atualizados com sucesso (XPath: {xpath})", Color.Green);
        }
        catch (Exception ex)
        {
            Log($"Erro ao atualizar elementos: {ex.Message}", Color.Red);
        }
    }

    private void Open_Click(object sender, EventArgs e)
    {
        OpenMenu("Size", "Size[RM022]"); //Conteiner Size
        //BuildElementTree("//*[@Name='Row 1']"); // funciona
        //BuildElementTree("//*[@Name='Data Panel']/*"); // funciona
        //BuildElementTree("//*[@Name='Data Panel']/*[@Value='40']"); // nao funciona
        //BuildElementTree("//*[@Value.Value='40']"); // nao funciona
        //BuildElementTree("//*[Value.Value='40']"); // nao funciona
        //BuildElementTree("//*[@ControlType='ListItem']"); // nao funciona
        //BuildElementTree("//*[contains(@ControlType,'ListItem')]"); // nao funciona
        //BuildElementTree("//*[contains(@Name,'Row')]"); // Nao funciona
        ParseDataPanel();


    }

    private List<Dictionary<string, string>> ParseDataPanel()
    {
        var tableData = new List<Dictionary<string, string>>();
        try
        {
            // Get headers
            var headers = _driver.FindElements(By.XPath("//*[@Name='Header Panel']/*"), 10)
                                .Select(h => h.Text)
                                .ToList();

            // Get rows
            var rows = _driver.FindElements(By.XPath("//*[@Name='Data Panel']/*"), 10);

            Log("==== DATA PANEL CONTENTS ====", Color.Blue);

            // Log headers
            Log($"Headers: {string.Join(";", headers)}", Color.DarkBlue);

            foreach (var row in rows)
            {
                try
                {
                    var cells = row.Text.Split(";");
                    var rowData = new Dictionary<string, string>();

                    for (int i = 0; i < Math.Min(headers.Count, cells.Length); i++)
                    {
                        rowData[headers[i]] = cells[i].Trim();
                    }

                    tableData.Add(rowData);
                    Log($"Row: {string.Join(";", rowData.Values)}", Color.DarkBlue);
                }
                catch (Exception ex)
                {
                    Log($"Error parsing row: {ex.Message}", Color.Red);
                }
            }
            Log("============================", Color.Blue);
        }
        catch (Exception ex)
        {
            Log($"Error finding Data Panel: {ex.Message}", Color.Red);
        }
        return tableData;
    }

    private void OpenMenu(string textoPesquisa, string nomeJanela)
    {
        try
        {
            Log("Abrindo menu: " + textoPesquisa);

            _driver.SwitchTo().ActiveElement().SendKeys(OpenQA.Selenium.Keys.Control + "o");

            new Actions(_driver)
                .SendKeys(textoPesquisa)
                .SendKeys(OpenQA.Selenium.Keys.ArrowDown)
                .SendKeys(OpenQA.Selenium.Keys.Enter)
                .SendKeys(OpenQA.Selenium.Keys.Enter)
                .SendKeys(OpenQA.Selenium.Keys.Enter)
                .Perform();

            Log("Aguardando menu abrir: " + textoPesquisa);

            new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                .Until(d => _driver.Title.ToUpper().Contains(textoPesquisa.ToUpper()));

            if (!_driver.Title.ToUpper().Contains(textoPesquisa.ToUpper()))
                throw new Exception($"Janela '{_driver.Title}' diferente da janela esperada '{nomeJanela}'");

            Log("Aberta: " + _driver.Title);

            Log("Operação Abrir concluída com sucesso", Color.Green);

        }
        catch (Exception ex)
        {
            Log($"ERRO: Erro ao enviar Ctrl+O: {ex.Message}", Color.Red);
        }
    }

    private async void gate_Click(object sender, EventArgs e)
    {
        OpenMenu("Truck Transaction", "Truck Transaction[ER025]");
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
            userNameField.SendKeys(_usernameInput.Text);
            Log("Usuário inserido", Color.Blue);

            // Wait for and interact with password field
            passwordField.WaitForElement(_driver);
            passwordField.SendKeys(_passwordInput.Text);
            Log("Senha inserida", Color.Blue);

            // Wait for and click OK button
            okButton.WaitForElement(_driver);
            okButton.Click();
            Log("Botão OK clicado", Color.Blue);

            // Verifica sucesso do login

            // Esperar nova tela
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                .Until(d => _driver.WindowHandles.Count > 0);

            // Mudar para a nova tela
            _driver.SwitchTo().Window(_driver.WindowHandles.First());

            Log("Aberta: " + _driver.Title);

            var janelaEsperada = "CTOS DIS";
            if (_driver.Title != janelaEsperada)
                throw new Exception($"Janela '{_driver.Title}' diferente da janela esperada '{janelaEsperada}'");

            Log("Login realizado com sucesso", Color.Green);

        }
        catch (Exception ex)
        {
            Log($"ERRO: Falha no login: {ex.Message}", Color.Red);
        }
    }
// Method to attempt starting a session
private bool TryStartSession()
{
    Log("Attempting to start session...", Color.Blue);
    try
    {
        var opt = FlaUIDriverOptions.ForApp("C:\\Users\\Beltzac\\Desktop\\DIS\\CM.CTOS.WinUIAdmin.exe");
        _driver = new WindowsDriver(new Uri("http://192.168.56.56:4723/"), opt);
        _sessionActive = true;
        // _sessionInfo might be null if InitializeComponents hasn't fully run or if called early
        if (_sessionInfo != null) _sessionInfo.Text = $"ID da Sessão: {_sessionId}";
        Log("Session started successfully.", Color.Green);
        return true;
    }
    catch (Exception ex)
    {
        if (_sessionInfo != null) _sessionInfo.Text = $"Error: {ex.Message}";
        Log($"Failed to start session: {ex.Message}", Color.Red);
        _sessionActive = false;
        return false;
    }
}

// Method to attempt login
private bool TryLogin()
{
    Log("Attempting to login...", Color.Blue);
    try
    {
        var win = _driver.WindowHandles;
        _driver.SwitchTo().Window(win.First());

        var userNameField = _driver.FindElement(By.Id("txtUserName"), 10);
        var passwordField = _driver.FindElement(By.Id("txtPassword"), 10);
        var okButton = _driver.FindElement(By.Id("btnOK"), 10);

        userNameField.WaitForElement(_driver);
        userNameField.Click();
        userNameField.SendKeys(_usernameInput.Text);
        Log("Username entered.", Color.Blue);

        passwordField.WaitForElement(_driver);
        passwordField.SendKeys(_passwordInput.Text);
        Log("Password entered.", Color.Blue);

        okButton.WaitForElement(_driver);
        okButton.Click();
        Log("OK button clicked.", Color.Blue);

        new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
            .Until(d => _driver.WindowHandles.Count > 0); // Basic check, might need refinement

        _driver.SwitchTo().Window(_driver.WindowHandles.First());
        Log("Switched to window: " + _driver.Title, Color.Blue);

        var expectedWindowTitle = "CTOS DIS";
        if (!_driver.Title.Contains(expectedWindowTitle)) // Use Contains for flexibility
        {
             throw new Exception($"Expected window title '{expectedWindowTitle}' not found. Actual: '{_driver.Title}'");
        }

        Log("Login successful.", Color.Green);
        return true;
    }
    catch (Exception ex)
    {
        Log($"ERROR: Login failed: {ex.Message}", Color.Red);
        return false;
    }
}

// Method to attempt opening a menu
private bool TryOpenMenu(string menuSearchText, string expectedWindowTitlePart)
{
    Log($"Attempting to open menu: {menuSearchText}", Color.Blue);
    try
    {
        _driver.SwitchTo().ActiveElement().SendKeys(OpenQA.Selenium.Keys.Control + "o");

        new Actions(_driver)
            .SendKeys(menuSearchText)
            .SendKeys(OpenQA.Selenium.Keys.ArrowDown)
            .SendKeys(OpenQA.Selenium.Keys.Enter)
            .SendKeys(OpenQA.Selenium.Keys.Enter)
            .SendKeys(OpenQA.Selenium.Keys.Enter)
            .Perform();

        Log($"Waiting for menu '{menuSearchText}' to open...", Color.Blue);

        // Wait for the window title to contain the expected part
        bool titleMatches = new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
            .Until(d => d.Title.ToUpper().Contains(expectedWindowTitlePart.ToUpper()));

        if (!titleMatches)
        {
            throw new Exception($"Expected window title part '{expectedWindowTitlePart}' not found. Actual title: '{_driver.Title}'");
        }

        Log($"Menu opened successfully: {_driver.Title}", Color.Green);
        return true;
    }
    catch (Exception ex)
    {
        Log($"ERROR: Failed to open menu '{menuSearchText}': {ex.Message}", Color.Red);
        return false;
    }
}

// Method to attempt parsing the data panel
private bool TryParseDataPanel()
{
    Log("Attempting to parse Data Panel...", Color.Blue);
    var tableData = new List<Dictionary<string, string>>();
    try
    {
        var headers = _driver.FindElements(By.XPath("//*[@Name='Header Panel']/*"), 10)
                              .Select(h => h.Text)
                              .ToList();

        var rows = _driver.FindElements(By.XPath("//*[@Name='Data Panel']/*"), 10);

        if (!headers.Any() || !rows.Any())
        {
             Log("Warning: Headers or rows not found in Data Panel.", Color.Orange);
             // Decide if this is a failure or just empty data
             // return false; // Uncomment if empty panel is considered a failure
        }

        Log("==== DATA PANEL CONTENTS ====", Color.Blue);
        Log($"Headers: {string.Join(";", headers)}", Color.DarkBlue);

        foreach (var row in rows)
        {
            try
            {
                var cells = row.Text.Split(';'); // Assuming semicolon delimiter
                var rowData = new Dictionary<string, string>();

                for (int i = 0; i < Math.Min(headers.Count, cells.Length); i++)
                {
                    rowData[headers[i]] = cells[i].Trim();
                }
                tableData.Add(rowData);
                Log($"Row: {string.Join(";", rowData.Values)}", Color.DarkBlue);
            }
            catch (Exception ex)
            {
                // Log row-specific error but continue processing other rows
                Log($"Error parsing row: {ex.Message}", Color.Red);
            }
        }
        Log("============================", Color.Blue);
        Log("Data Panel parsed successfully.", Color.Green);
        return true; // Return true even if some rows had errors, as long as the panel itself was accessed
    }
    catch (Exception ex)
    {
        Log($"ERROR: Failed to find or parse Data Panel: {ex.Message}", Color.Red);
        return false; // Return false if the panel couldn't be accessed at all
    }
}



// --- New Event Handler for Showcase Button ---
private async void ShowcaseSequence_Click(object sender, EventArgs e)
{
    Log("Starting showcase sequence...", Color.Magenta);

    // Step 1: Start Session
    Log("Step 1: Starting Session...", Color.Cyan);
    if (!TryStartSession())
    {
        Log("Step 1 FAILED. Aborting sequence.", Color.Red);
        return;
    }
    Log("Step 1 SUCCESS.", Color.Green);

    // Step 2: Login
    Log("Step 2: Performing Login...", Color.Cyan);
    if (!TryLogin())
    {
        Log("Step 2 FAILED. Aborting sequence.", Color.Red);
        return;
    }
    Log("Step 2 SUCCESS.", Color.Green);

    // Step 3: Open Menu
    Log("Step 3: Opening Menu 'Size'...", Color.Cyan);
    if (!TryOpenMenu("Size", "Size[RM022]")) // Pass expected title part
    {
        Log("Step 3 FAILED. Aborting sequence.", Color.Red);
        return;
    }
    Log("Step 3 SUCCESS.", Color.Green);

    // Step 4: Parse Data Panel
    Log("Step 4: Parsing Data Panel...", Color.Cyan);
    if (!TryParseDataPanel())
    {
        Log("Step 4 FAILED. Sequence incomplete.", Color.Red);
        // Decide if you want to return or just log failure
        // return;
    }
    else
    {
        Log("Step 4 SUCCESS.", Color.Green);
    }

    Log("Showcase sequence finished.", Color.Magenta);
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