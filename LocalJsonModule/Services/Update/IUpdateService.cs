using LocalJsonModule.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalJsonModule.Services.Update
{
    public interface IUpdateService
    {
        Task<bool> UpdateAsync(Guid id, UpdateRequest request);
    }
}
