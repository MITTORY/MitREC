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

namespace MitREC.Windows
{
    /// <summary>
    /// Логика взаимодействия для adminPanel.xaml
    /// </summary>
    public partial class adminPanel : Window
    {
        public adminPanel()
        {
            InitializeComponent();

            ApplicationContext db = new ApplicationContext();
            List<User> users = db.Users.ToList();

            profileList.ItemsSource = users;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }


        private void deleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (profileList.SelectedItem != null)
            {
                User selectedUser = (User)profileList.SelectedItem;

                using (ApplicationContext db = new ApplicationContext())
                {
                    db.Users.Attach(selectedUser);
                    db.Users.Remove(selectedUser);

                    db.SaveChanges();
                }

                RefreshListView();
            }
        }

        private void RefreshListView()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                List<User> users = db.Users.ToList();
                profileList.ItemsSource = users;
            }
        }

        private void returnAuth_Click(object sender, RoutedEventArgs e)
        {
            Start start = new Start();
            start.Show();
            Close();
        }
    }
}
