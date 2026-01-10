using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using StudentManagement.Core.DTOs;

namespace StudentManagement.Core.Interfaces
{
    public interface IGradeService
    {
        Task<GradeResponse> UpdateGradeAsync(UpdateGradeRequest request);
    }
}

