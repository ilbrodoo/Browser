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
        private bool isRefreshing = false;
        private Uri currentUrl;
        private WebBrowser currentWebBrowser;
        private Uri previousUrl;
        private Uri googleSearchUrl = new Uri("https://www.google.com");

        public MainWindow()
        {
            currentUrl = new Uri("https://www.google.com/");
            InitializeComponent();

            TabItem initialTab = (TabItem)tabControl.Items[0];
            currentWebBrowser = (WebBrowser)initialTab.Content;

            if (currentWebBrowser != null)
            {
                currentWebBrowser.Navigate(currentUrl);
            }
        }
        private async void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            string input = urlTextBox.Text;
            if (!string.IsNullOrEmpty(input))
            {
                // Rimuovi spazi vuoti iniziali e finali dall'input
                input = input.Trim();

                // Verifica se l'input è una URL assoluta valida
                Uri url;
                if (Uri.TryCreate(input, UriKind.Absolute, out url))
                {
                    previousUrl = currentUrl; // Salva l'URL corrente come URL precedente
                    currentUrl = url; // Aggiorna l'URL corrente
                    await LoadWebPageAsync(currentWebBrowser, url);
                }
                else
                {
                    // Controlla se l'input contiene spazi e non contiene "." o "www"
                    if (input.Contains(" ") || (!input.Contains(".") && !input.Contains("www")))
                    {
                        // E' una ricerca su Google
                        url = new Uri(googleSearchUrl, "search?q=" + Uri.EscapeDataString(input));
                    }
                    else
                    {
                        // Aggiungi "https://" se necessario
                        if (!input.StartsWith("http://") && !input.StartsWith("https://"))
                        {
                            // Controlla se l'input inizia con "www." e aggiungi "https://" se necessario
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

                    previousUrl = currentUrl; // Salva l'URL corrente come URL precedente
                    currentUrl = url; // Aggiorna l'URL corrente
                    await LoadWebPageAsync(currentWebBrowser, url);
                }
            }
        }


        private TaskCompletionSource<bool> pageLoadTcs;

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

        private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            // Segnalare che il caricamento della pagina è completo
            pageLoadTcs.SetResult(true);
        }

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            TabItem newTab = new TabItem();
            newTab.Header = "Scheda " + tabCounter;
            newTab.Content = CreateNewTabContent();
            tabControl.Items.Add(newTab);
            tabControl.SelectedItem = newTab;
            tabCounter++;
        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTabToGoogle();
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            Button closeButton = (Button)sender;
            TabItem tabItem = FindParentTabItem(closeButton);

            if (tabItem != null)
            {
                tabControl.Items.Remove(tabItem);

                if (tabControl.Items.Count == 0)
                {
                    AddNewTabToGoogle();
                }
            }
        }

        private int tabCounter = 1;

        private void AddNewTabToGoogle()
        {
            TabItem newTab = new TabItem();
            newTab.Header = "New Tab"; // Imposta l'intestazione iniziale
            WebBrowser newWebBrowser = new WebBrowser();
            newWebBrowser.Name = "webBrowser" + tabControl.Items.Count;

            // Imposta l'URL di Google
            newWebBrowser.Navigate("https://www.google.com");

            newTab.Content = newWebBrowser;
            tabControl.Items.Add(newTab);
            tabControl.SelectedItem = newTab;
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

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentWebBrowser != null)
            {
                // Esegui l'aggiornamento della pagina web
                currentWebBrowser.Refresh();
                isRefreshing = true;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            TabItem selectedTab = tabControl.SelectedItem as TabItem;
            WebBrowser webBrowser = selectedTab?.Content as WebBrowser;

            if (webBrowser != null)
            {
                if (previousUrl != null)
                {
                    webBrowser.Navigate(previousUrl); // Torna all'URL precedente
                }
                else
                {
                    // Se non c'è un URL precedente, carica una pagina vuota
                    webBrowser.Navigate("about:blank");
                }
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentWebBrowser.CanGoBack)
            {
                currentWebBrowser.GoBack();
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentWebBrowser.CanGoForward)
            {
                currentWebBrowser.GoForward();
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
