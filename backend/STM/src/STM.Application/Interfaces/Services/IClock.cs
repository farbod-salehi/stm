using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Application.Interfaces.Services
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }

}
