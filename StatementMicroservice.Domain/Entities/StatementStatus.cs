﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatementMicroservice.Domain.Entities
{
    public enum StatementStatus
    {
        Pending = 0,
        Completed = 1,
        Rejected = 2
    }
}
