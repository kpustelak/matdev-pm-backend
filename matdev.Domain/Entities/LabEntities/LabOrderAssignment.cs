using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Domain.Entities.LabEntities
{
    public class LabOrderAssignment
    {
        public int LabOrderAssignmentID { get; set; }
        public int LabOrderID { get; set; }
        public LabOrder LabOrder { get; set; }
        public int ProjectID { get; set; }
        public Project Project { get; set; }
    }
}
