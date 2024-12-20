using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models;

public class ClientData
{

    public int Id { get; set; }
    public float CpuUsage { get; set; }
    public float RamUsage { get; set; }
    public int[] NumbersList { get; set; }



}
