using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace matdev.Domain.Entities.LookupEntities
{
    public class IssueType
    {
        public int IssueTypeID { get; set; }
        public string Name { get; set; }

    }
}
