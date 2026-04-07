using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Domain.Entities.LabEntities
{
    public class LabOrder
    {
        public int LabOrderID { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PlannedCompletionDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? SampleID { get; set; }

    }
}
