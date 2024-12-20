using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Entities;

public class DataModel
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public float RamUsage { get; set; }
    public float CpuUsage { get; set; }
    public int Sum { get; set; }

}
