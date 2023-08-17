using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace Discarding_2._1.Db
{
    public class NamesContext : DbContext
    {
        public DbSet<Names> Names { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string relPath = "NamesList.db";
            string resPath = Path.Combine(exeDir, relPath);
            resPath = Path.GetFullPath(resPath);

            optionsBuilder.UseSqlite($"Data Source={resPath}");
        }
    }
}
