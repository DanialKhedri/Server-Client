using Server.Data;
using Server.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Repository;

public class DataRepository : IDataRepository
{


    #region Ctor

    private readonly DataContext _dataContext;

    public DataRepository(DataContext dataContext)
    {
        this._dataContext = dataContext;
    }

    #endregion

    public async Task<bool> AddDataToDataBase(DataModel dataModel)
    {

        if (dataModel == null)
            return false;


        try
        {
            await _dataContext.AddAsync(dataModel);
            await _dataContext.SaveChangesAsync();

            return true;

        }
        catch
        {

            return false;

        }

    }


}
