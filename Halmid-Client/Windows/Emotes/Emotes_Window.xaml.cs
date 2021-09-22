using Halmid_Client.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Halmid_Client.Windows.Emotes
{
    /// <summary>
    /// Logika interakcji dla klasy Emotes_Window.xaml
    /// </summary>
    public partial class Emotes_Window : Page
    {
        private class Emojis
        {
            public string Path { get; set; }
            public string Unicode { get; set; }
        }
        List<Emojis> emoji = new List<Emojis>();

        public Emotes_Window()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEmojis();
            EmojiList.ItemsSource = emoji;
        }
        private void Emoji_Select(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext;
            int index = EmojiList.Items.IndexOf(item);
            MainWindow win = (MainWindow)Window.GetWindow(this);
            if(win.msg.Opacity < 1) {
                win.msg.Focus();
            }
            win.msg.Text += EmojiData.codes[index];  
        }
        private void LoadEmojis()
        {
            for(int i = 0;i<EmojiData.urls.Length;i++)
            {
                Emojis emo = new Emojis();
                emo.Path = EmojiData.urls[i];
                emo.Unicode = EmojiData.codes[i];
                emoji.Add(emo);
            }
        }
    }
}
