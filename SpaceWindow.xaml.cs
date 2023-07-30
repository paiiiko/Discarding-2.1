using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Discarding_2._1.Db;
using Microsoft.EntityFrameworkCore;

namespace Discarding_2._1
{
    public partial class SpaceWindow : Window
    {
        public SpaceWindow()
        {
            InitializeComponent();
            MainWindow.db.Names.Load();
            DataContext = MainWindow.db.Names.Local.ToObservableCollection();
            nameSpace.ItemsSource = MainWindow.db.Names.Local.ToObservableCollection();
            this.Closed += ClosingSpace;
        }
        public void ClosingSpace(object sender, EventArgs e)
        {
            MainWindow.db.SaveChangesAsync();
            List<Names> namesList = MainWindow.db.Names.ToList();
            for (int i = 0; i < namesList.Count; i++)
            {
                List<string> distinctList = namesList[i].VarietyOfNames.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                distinctList = distinctList.Distinct().ToList();
                namesList[i].VarietyOfNames = String.Join(" ", distinctList.Select(x => x.ToString()));
            }

            for (int i = 0; i < namesList.Count; i++)
            {
                if (!namesList[i].VarietyOfNames.StartsWith(' '))
                {
                    namesList[i].VarietyOfNames = " " + namesList[i].VarietyOfNames;
                }
                if (!namesList[i].VarietyOfNames.EndsWith(' '))
                {
                    namesList[i].VarietyOfNames = namesList[i].VarietyOfNames + " ";
                }
            }
            MainWindow.db.SaveChangesAsync();
        }
    }
}
