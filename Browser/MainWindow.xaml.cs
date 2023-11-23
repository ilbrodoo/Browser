using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Browser
{
    public partial class MainWindow : Window
    {
        private Uri googleSearchUrl = new Uri("https://www.google.com");
        private TaskCompletionSource<bool> pageLoadTcs;
        private bool isRefreshing = false;
        private Uri currentUrl;
        private WebBrowser currentWebBrowser;
        private Uri previousUrl;

        public MainWindow()
        {
            currentUrl = new Uri("https://www.google.com/");
            InitializeComponent();

            
            AddNewTab("https://www.google.com/");


            if (currentWebBrowser != null)
            {
                currentWebBrowser.Navigate(currentUrl);
            }
        }

        // Gestisce il click sul pulsante "Naviga"
        private async void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            string input = urlTextBox.Text;
            if (!string.IsNullOrEmpty(input))
            {
                input = input.Trim();

                Uri url;
                if (Uri.TryCreate(input, UriKind.Absolute, out url))
                {
                    previousUrl = currentUrl;
                    currentUrl = url;
                    await LoadWebPageAsync(currentWebBrowser, url);
                }
                else
                {
                    if (input.Contains(" ") || (!input.Contains(".") && !input.Contains("www")))
                    {
                        url = new Uri(googleSearchUrl, "search?q=" + Uri.EscapeDataString(input));
                    }
                    else
                    {
                        if (!input.StartsWith("http://") && !input.StartsWith("https://"))
                        {
                            if (!input.StartsWith("www."))
                            {
                                input = "www." + input;
                            }

                            url = new Uri("https://" + input);
                        }
                        else
                        {
                            url = new Uri(input);
                        }
                    }

                    previousUrl = currentUrl;
                    currentUrl = url;

                    // Ottieni la scheda attualmente selezionata
                    TabItem selectedTab = tabControl.SelectedItem as TabItem;

                    if (selectedTab != null)
                    {
                        WebBrowser selectedWebBrowser = selectedTab.Content as WebBrowser;

                        if (selectedWebBrowser != null)
                        {
                            await LoadWebPageAsync(selectedWebBrowser, url);
                        }
                    }
                }
            }
        }

        // Carica una pagina web in modo asincrono
        private async Task LoadWebPageAsync(WebBrowser webBrowser, Uri uri)
        {
            webBrowser.Navigating += (s, e) =>
            {
                dynamic activeX = webBrowser.GetType().GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(webBrowser);
                activeX.Silent = true;
            };

            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    webBrowser.Navigate(uri);
                });
            });
        }

        // Gestisce l'evento di completamento del caricamento della pagina
        private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            
            pageLoadTcs.SetResult(true);
        }

        // Aggiunge una nuova scheda con l'URL specificato
        private void AddNewTab(string initialUrl)
        {
            TabItem newTab = new TabItem();
            WebBrowser newWebBrowser = new WebBrowser();
            newWebBrowser.Name = "webBrowser" + tabControl.Items.Count;

            newWebBrowser.LoadCompleted += (sender, e) =>
            {
                if (newWebBrowser.Source != null)
                {
                    string host = newWebBrowser.Source.Host;
                    if (host.StartsWith("www."))
                    {
                        host = host.Substring(4);
                    }

                   
                    host = host.Replace(".com", "");

                    newTab.Header = host;
                }
            };

                Uri initialUri = new Uri(initialUrl);
            newWebBrowser.Navigate(initialUri);

            newTab.Content = newWebBrowser;
            tabControl.Items.Add(newTab);
            tabControl.SelectedItem = newTab;
        }

       
        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab("https://www.google.com");
        }

        private TabItem FindParentTabItem(DependencyObject child)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while (parent != null && !(parent is TabItem))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as TabItem;
        }

        // Gestisce il click sul pulsante "Aggiorna"
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentWebBrowser != null)
            {
               
                currentWebBrowser.Refresh();
                isRefreshing = true;
            }
        }

        // Gestisce il click sul pulsante di chiusura di una scheda
        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            Button closeButton = (Button)sender;
            TabItem tabItem = FindParentTabItem(closeButton);

            if (tabItem != null)
            {
                tabControl.Items.Remove(tabItem);

                if (tabControl.Items.Count == 0)
                {
                    AddNewTab("https://www.google.com");
                }
            }
        }

        // Gestisce il click sul pulsante "Stop"
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            TabItem selectedTab = tabControl.SelectedItem as TabItem;
            WebBrowser webBrowser = selectedTab?.Content as WebBrowser;

            if (webBrowser != null)
            {
                if (previousUrl != null)
                {
                    webBrowser.Navigate(previousUrl); 
                }
                else
                {
                    
                    webBrowser.Navigate("about:blank");
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem selectedTab = tabControl.SelectedItem as TabItem;
            if (selectedTab != null)    
            {
                currentWebBrowser = (WebBrowser)selectedTab.Content;
            }
        }

        // Gestisce il click sul pulsante "Indietro"
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentWebBrowser.CanGoBack)
            {
                currentWebBrowser.GoBack();
            }
        }

        // Gestisce il click sul pulsante "Avanti"
        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentWebBrowser.CanGoForward)
            {
                currentWebBrowser.GoForward();
            }
        }
    }
}
