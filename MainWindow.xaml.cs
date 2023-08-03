using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Discarding_2._1.Db;
using Microsoft.EntityFrameworkCore;
using System.Windows.Controls;
using System.Windows.Media;

namespace Discarding_2._1
{
    public partial class MainWindow : Window
    {
        public static NamesContext db = new NamesContext();
        Message chat = null;
        Names names = new Names();
        List<Position> reservList = new List<Position>();
        SpaceWindow spaceWindow;
        public MainWindow()
        {
            InitializeComponent();
            db.Database.EnsureCreated();
            db.Names.Load();
            DataContext = Position.positionsList;//db.Names.Local.ToObservableCollection();
            this.Closed += ClosingWindow;
        }
        private void LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                Position position = (Position)e.Row.DataContext;

                if (position.Comments != null)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Yellow);
                }
            }
            catch
            {
                Exception exeption = new Exception();
            }
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Position.positionsList = reservList;
            content.ItemsSource = Position.positionsList;
            content.Items.Refresh();
        }
        private void ClosingWindow(object sender, EventArgs e)
        {
            List<Names> namesList = db.Names.ToList();

            for (int i = 0; i < namesList.Count; i++)
            {
                List<string> name = namesList[i].VarietyOfNames.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                for (int j = 0; j < name.Count; j++)
                {
                    name[j] = " " + name[j] + " ";
                }

                name = name.Distinct().ToList();

                for (int j = 0; j < name.Count; j++)
                {
                    for (int z = i + 1; z < namesList.Count; z++)
                    {
                        if (namesList[z].VarietyOfNames.Contains(name[j]))
                        {
                            for (int k = 0; k < name.Count; k++)
                            {
                                if (!namesList[z].VarietyOfNames.Contains(name[k]))
                                {
                                    namesList[z].VarietyOfNames += name[k].TrimStart();
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < namesList.Count; i++)
            {
                bool flag = false;
                List<string> name = namesList[i].VarietyOfNames.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                for (int j = 0; j < name.Count; j++)
                {
                    name[j] = " " + name[j] + " ";
                }
                for (int j = 0; j < name.Count; j++)
                {
                    if (flag == true)
                    {
                        break;
                    }
                    for (int z = i + 1; z < namesList.Count; z++)
                    {
                        if (namesList[z].VarietyOfNames.Contains(name[j]))
                        {
                            namesList.RemoveAt(i);
                            i--;
                            flag = true;
                            break;
                        }
                    }
                }
            }
            if (namesList.Count > 0)
            {
                db.Names.ExecuteDelete();
                db.Names.AddRange(namesList);
            }
            if (spaceWindow != null)
            {
                spaceWindow.ClosingSpace(sender, e);
                spaceWindow.Close();
            }

            db.SaveChangesAsync();
        }
        private void Json_Click(object sender, RoutedEventArgs e)
        {
            if (content.Items.Count > 0) Position.positionsList.Clear();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
            openFileDialog.ShowDialog();
            if (openFileDialog.FileNames.Length == 0) return;

            string data = File.ReadAllText(openFileDialog.FileName);

            chat = JsonSerializer.Deserialize<Message>(data);

            string vendorCode = null;
            string name = null;
            decimal? amount = null;
            string units = null;
            string comment = null;

            Regex regexVendor = new Regex(@"^\s*(\w+\s*\S*\s*\S*\s*\S*\s*\S*)\:\s*$", RegexOptions.Multiline);
            Regex regexName = new Regex(@"^\s*(\w+\s*\S*\s*\S*\s*\S*)\s*\-", RegexOptions.Multiline);
            Regex regexAmount = new Regex(@"\-*\s*(\d+\.*\,*\d*)", RegexOptions.Multiline);
            Regex regexUnits = new Regex(@"\d\s*([А-Яа-яЁё]+)", RegexOptions.Multiline);
            Regex regexComment = new Regex(@"\(([А-Яа-яЁё]+)\)", RegexOptions.Multiline);

            for (int i = 0; i < chat.Messages.Count; i++)
            {
                vendorCode = null;
                string[] message = chat.Messages[i].Text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                for (int x = 0; x < message.Length; x++)
                {
                    Match matchVendor = regexVendor.Match(message[x]);
                    if (matchVendor.Success)
                    {
                        vendorCode = (matchVendor.Groups[1].Value).Trim().ToLower();
                        continue;
                    }
                    Match matchName = regexName.Match(message[x]);
                    if (matchName.Success)
                    {
                        name = matchName.Groups[1].Value.Trim().ToLower();
                    }
                    Match matchAmount = regexAmount.Match(message[x]);
                    if (matchAmount.Success)
                    {
                        amount = Convert.ToDecimal(matchAmount.Groups[1].Value.Replace(".", ","));
                        if (amount <= 0) amount = null;
                    }
                    Match matchUnits = regexUnits.Match(message[x]);
                    if (matchUnits.Success)
                    {
                        units = matchUnits.Groups[1].Value.ToLower();
                    }
                    Match matchComment = regexComment.Match(message[x]);
                    if (matchComment.Success)
                    {
                        comment = matchComment.Groups[1].Value;
                    }
                    if (name != null & amount != null & units == null & comment == null)
                    {
                        Position.positionsList.Add(Position.Default(vendorCode, name, amount, (DateTime.Parse(chat.Messages[i].Date)).ToString("G"), chat.Messages[i].From));
                        name = null;
                        amount = null;
                    }
                    else if (name != null & amount != null & units != null & comment == null)
                    {
                        Position.positionsList.Add(Position.WhithUnits(vendorCode, name, amount, units, (DateTime.Parse(chat.Messages[i].Date)).ToString("G"), chat.Messages[i].From));
                        name = null;
                        amount = null;
                        units = null;
                    }
                    else if (name != null & amount != null & units == null & comment != null)
                    {
                        Position.positionsList.Add(Position.WithComments(vendorCode, name, amount, comment, (DateTime.Parse(chat.Messages[i].Date)).ToString("G"), chat.Messages[i].From));
                        name = null;
                        amount = null;
                        comment = null;
                    }
                    else if (name != null & amount != null & units != null & comment != null)
                    {
                        Position.positionsList.Add(Position.WithUnitsAndComments(vendorCode, name, amount, units, comment, (DateTime.Parse(chat.Messages[i].Date)).ToString("G"), chat.Messages[i].From));
                        name = null;
                        amount = null;
                        units = null;
                        comment = null;
                    }
                    else
                    {
                        Position.positionsList.Add(Position.Error(message[x].ToLower(), (DateTime.Parse(chat.Messages[i].Date)).ToString("G"), chat.Messages[i].From));
                        name = null;
                        amount = null;
                        units = null;
                        comment = null;
                    }
                }
            }
            Position.positionsList.Sort();
            for (int i = 1; i < Position.positionsList.Count; i++)
            {
                Position.positionsList[i].Id = i;
            }

            content.ItemsSource = Position.positionsList;
            content.Items.Refresh();
        }
        private void First_Click(object sender, RoutedEventArgs e)
        {
            Position.positionsList.Sort();
            if (Position.positionsList == null) return;
            reservList = (List<Position>)Position.positionsList.Clone();
            for (int i = 0; i < Position.positionsList.Count - 1; i++)
            {
                if (Position.positionsList[i].VendorCode != null)
                {
                    if (Position.positionsList[i].VendorCode == Position.positionsList[i + 1].VendorCode &&
                        Position.positionsList[i].Name == Position.positionsList[i + 1].Name &&
                        Position.positionsList[i].Units == Position.positionsList[i + 1].Units &&
                        Position.positionsList[i].Comments == null &&
                        Position.positionsList[i + 1].Comments == null)
                    {
                        Position.positionsList[i].Amount += Position.positionsList[i + 1].Amount;
                        Position.positionsList[i].From = null;
                        Position.positionsList[i].Date = null;
                        Position.positionsList.RemoveAt(i + 1);
                        i--;
                    }
                }
            }
            //храним индексы позиций с совпадениями имен по контексту базы данных
            List<int> listIndex = new List<int>();
            //дублированый список совпадений из positionList
            List<Position> listPositions = new List<Position>();
            var parse = db.Names.Select(x => new { x.VarietyOfNames }).ToList();
            //ищу соответствие данных, идя по перечню строк в базе
            foreach (var line in parse)
            {
                //listOfNames = лист имён из одной строки в базе данных
                List<string> listOfNames = line.VarietyOfNames.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                for (int i = 0; i < listOfNames.Count; i++)
                {
                    listOfNames[i] = " " + listOfNames[i] + " ";
                }
                for (int i = 0; i < listOfNames.Count; i++)
                {
                    //positionList = список, который является содержимым программы.
                    //               каждый раз новые данные, с которыми работаем каждый месяц.
                    for (int j = 0; j < Position.positionsList.Count; j++)
                    {
                        // ...
                        string nameOfPosition = " " + Position.positionsList[j].Name.Replace(" ", "") + " ";
                        if (listOfNames[i] == nameOfPosition)
                        {
                            listIndex.Add(Position.positionsList.FindIndex(x => x.Equals(Position.positionsList[j])));
                        }
                    }
                }
                listIndex.Sort();
                for (int i = 0; i < listIndex.Count; i++)
                {
                    listPositions.Add(Position.positionsList[listIndex[i]]);
                }
                for (int i = 0; i < listPositions.Count; i++)
                {
                    for (int j = i + 1; j < listPositions.Count; j++)
                    {
                        //if (i == listPositions.Count) break;
                        if (listPositions[i].VendorCode == listPositions[j].VendorCode &&
                            listPositions[i].Units == listPositions[j].Units &&
                            listPositions[i].Comments == null &&
                            listPositions[j].Comments == null)
                        {
                            listPositions[i].Amount += listPositions[j].Amount;
                            int index = Position.positionsList.IndexOf(listPositions[j]);
                            Position.positionsList.RemoveAt(index);
                            listPositions.RemoveAt(j);
                            j--;
                            //if (j >= listPositions.Count) break;

                        }
                    }
                    //if (i + 1 == listPositions.Count) break;
                    //if (listPositions[i].VendorCode  == listPositions[i + 1].VendorCode &&
                    //   listPositions[i].Units        == listPositions[i + 1].Units &&
                    //   listPositions[i].Comments     == null &&
                    //   listPositions[i + 1].Comments == null)
                    //{
                    //    listPositions[i].Amount += listPositions[i + 1].Amount;
                    //    int index = positionsList.IndexOf(listPositions[i + 1]);
                    //    positionsList.RemoveAt(index);
                    //    listPositions.RemoveAt(i + 1);
                    //    if (i + 1 == listPositions.Count) break;
                    //    i--;
                    //}
                }
                listIndex.Clear();
                listPositions.Clear();
                ////positionsList.Sort();
                ////for (int i = 0; i < positionsList.Count; i++)
                ////{
                ////    positionsList[i].Id = i;
                ////}
            }
            content.ItemsSource = Position.positionsList;
            content.Items.Refresh();
        }
        private void Merge_Click(object sender, RoutedEventArgs e)
        {
            if (content.SelectedItems.Count == 0) return;
            reservList = (List<Position>)Position.positionsList.Clone();
            int? idOfNames = null;
            List<Position> miniList = content.SelectedItems.Cast<Position>().ToList();
            names.VarietyOfNames = "";
            for (int i = 0; i < miniList.Count; i++)
            {
                miniList[i].Name = " " + miniList[i].Name + " ";
                if (!names.VarietyOfNames.Contains(miniList[i].Name))
                {
                    names.VarietyOfNames += " " + miniList[i].Name.Replace(" ", "") + " ";
                }
            }
            List<string> namesList = names.VarietyOfNames.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
            for (int i = 0; i < namesList.Count; i++)
            {
                namesList[i] = " " + namesList[i] + " ";
            }
            for (int i = 0; i < namesList.Count; i++)
            {
                Names x = db.Names.FirstOrDefault(x => EF.Functions.Like(x.VarietyOfNames, $"%{namesList[i]}%"));
                if (x != null)
                {
                    idOfNames = x.Id;
                }
            }
            if (idOfNames == null)
            {
                db.Add(names);
                names = new Names();
                db.SaveChangesAsync();
            }
            else if (idOfNames != null)
            {
                for (int i = 0; i < namesList.Count; i++)
                {
                    if (db.Names.Find(idOfNames).VarietyOfNames.Contains(namesList[i]) == false)
                    {
                        namesList[i] = namesList[i].TrimStart();
                        db.Names.Find(idOfNames).VarietyOfNames += namesList[i];
                        db.SaveChangesAsync();
                    }
                }
            }

            miniList.Sort();

            int index = Position.positionsList.FindIndex(x => x.Equals(miniList[0]));
            for (int i = 0; i < miniList.Count; i++)
            {
                Position.positionsList.Remove(miniList[i]);
            }
            for (int i = 1; i < miniList.Count; i++)
            {
                miniList[0].Amount += miniList[i].Amount;
            }
            miniList[0].Comments = null;
            miniList[0].Date = null;
            miniList[0].From = null;
            miniList[0].Name = miniList[0].Name.Trim();
            Position.positionsList.Insert(index, miniList[0]);
            content.ItemsSource = Position.positionsList;
            content.Items.Refresh();
        }
        private void Data_Click(object sender, EventArgs e)
        {
            spaceWindow = new SpaceWindow();
            spaceWindow.Show();
        }
        private void Save_Click(object sender, EventArgs e)
        {
            if (Position.positionsList.Count > 0)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                if (File.Exists("positions.json"))
                {
                    File.Delete("positions.json");
                }
                using (FileStream fs = new FileStream("positions.json", FileMode.OpenOrCreate))
                {
                    JsonSerializer.Serialize(fs, Position.positionsList, options);
                    fs.Close();
                }

                MessageBoxResult result = MessageBox.Show("Данные сохранены", "Успех");
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("В программе нет данных", "Провал");
            }
        }
        private void Load_Click(object sender, EventArgs e)
        {
            using (FileStream fs = new FileStream("positions.json", FileMode.OpenOrCreate))
            {
                Position.positionsList = JsonSerializer.Deserialize<List<Position>>(fs);
                fs.Close();
                content.ItemsSource = Position.positionsList;
                content.Items.Refresh();
            }
        }
    }
}
