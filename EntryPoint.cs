using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Discarding_2._1
{
    public class EntryPoint
    {
        // All WPF applications should execute on a single-threaded apartment (STA) thread
        [STAThread]
        public static void Main()
        {
            try
            {
                var app = new Application();
                
                app.Run(new MainWindow());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception occur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
