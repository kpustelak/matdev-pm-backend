using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Domain.Entities.Lab
{
    public class LabOrderAssigment
    {
        public int LabOrderAssigmentID { get; set; }
        public int LabOrderID { get; set; }
        public LabOrder LabOrder { get; set; }
        public int ProjectID { get; set; }
        public Project Project { get; set; }
    }
}
