using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.ElasticSearch
{

    public class IncidentReport
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime ReportedOn { get; set; }
        public Person ReportedBy { get; set; }
    }
    public class Person
    {
        public int Id { get; set; }
        public int Age { get; set; }
        public DateTime RecordDate { get; set; }

        public string City { get; set; }
        public string Country { get; set; }

        public string Name { get; set; }
        public string FamilyName { get; set; }

    }
}
