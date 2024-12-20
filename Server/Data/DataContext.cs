using Microsoft.EntityFrameworkCore;
using Server.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data;

public class DataContext : DbContext
{

    public DataContext()
    {
    }
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // اضافه کردن کانکشن استرینگ
        optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=ServerDatabase;Integrated Security=True;Trust Server Certificate=True");
    }

    DbSet<Entities.DataModel> DataModels { get; set; }


}
