using Server.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Repository;

public interface IDataRepository
{

    public Task<bool> AddDataToDataBase(Entities.DataModel dataModel);







}
