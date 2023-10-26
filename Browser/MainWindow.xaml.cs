        using System;
        using System.Threading.Tasks;
        using System.Windows;
        using System.Windows.Controls;
        using System.Windows.Navigation;

        namespace Browser
        {
            public partial class MainWindow : Window
            {
                public MainWindow()
                {
                    InitializeComponent();

                    TabItem initialTab = (TabItem)tabControl.Items[0];
                    WebBrowser webBrowser = (WebBrowser)initialTab.Content;

                    if (webBrowser != null)
                    {
                        webBrowser.Navigate("https://www.google.com/");
                    }
                }

                private async void NavigateButton_Click(object sender, RoutedEventArgs e)
                {
                    string input = urlTextBox.Text;
                    if (!string.IsNullOrEmpty(input))
                    {
                        Uri url;
                        // Prova a creare un Uri dall'input
                        if (Uri.TryCreate(input, UriKind.Absolute, out url))
                        {
                            // Se l'operazione è riuscita, naviga all'URL
                            await LoadWebPageAsync(url.ToString());
                        }
                        else
                        {
                            // Se l'operazione è fallita, esegui una ricerca su Google
                            url = new Uri("https://www.google.com/search?q=" + Uri.EscapeDataString(input));
                            await LoadWebPageAsync(url.ToString());
                        }
                    }
                }

                private TaskCompletionSource<bool> pageLoadTcs;

                private async Task LoadWebPageAsync(string url)
                {
                    pageLoadTcs = new TaskCompletionSource<bool>();

                    webBrowser.LoadCompleted += WebBrowser_LoadCompleted;
                    webBrowser.Navigate(new Uri(url));

                    // Attendere che la pagina sia completamente caricata
                    await pageLoadTcs.Task;

                    webBrowser.LoadCompleted -= WebBrowser_LoadCompleted;
                }

                private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
                {
                    // Segnalare che il caricamento della pagina è completo
                    pageLoadTcs.SetResult(true);
                }

                private int tabCounter = 1;

                private void NewTabButton_Click(object sender, RoutedEventArgs e)
                {
                    TabItem newTab = new TabItem();
                    newTab.Header = "Scheda " + tabCounter;
                    newTab.Content = CreateNewTabContent();
                    tabControl.Items.Add(newTab);
                    tabControl.SelectedItem = newTab;
                    tabCounter++;
                }
        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            Button closeButton = (Button)sender;
            TabItem tabItem = (TabItem)tabControl.ItemContainerGenerator.ItemFromContainer(closeButton.Parent);
            tabControl.Items.Remove(tabItem);
        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            // Crea un nuovo TabItem
            TabItem newTab = new TabItem();
            newTab.Header = "New Tab"; // Imposta l'intestazione iniziale
            WebBrowser newWebBrowser = new WebBrowser();
            newWebBrowser.Name = "webBrowser" + tabControl.Items.Count;
            newTab.Content = newWebBrowser;

            // Aggiungi il nuovo TabItem alla fine del TabControl
            tabControl.Items.Add(newTab);

            // Passa alla nuova scheda
            tabControl.SelectedItem = newTab;
        }




        private void StopButton_Click(object sender, RoutedEventArgs e)
                {
                    TabItem selectedTab = tabControl.SelectedItem as TabItem;

                    if (selectedTab != null)
                    {
                        WebBrowser webBrowser = selectedTab.Content as WebBrowser;
                        if (webBrowser != null)
                        {
                            // Interrompi il caricamento della pagina web impostando la Source su null
                            webBrowser.Source = null;
                        }
                    }
                }


                private WebBrowser CreateNewTabContent()
                {
                    WebBrowser newWebBrowser = new WebBrowser();
                    newWebBrowser.Name = "webBrowser" + tabCounter;
                    newWebBrowser.Navigate("about:blank");
                    return newWebBrowser;
                }
            }
        }
