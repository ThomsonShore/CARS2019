using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CARS2019.Models
{
    public class CARS2019
    {
    }

    public class Lists
    {
        public string ProblemID { get; set; }
        public string ProblemDescription { get; set; }

    }

    public partial class CARSProblemList
    {
        public int ProblemID { get; set; }
        public string ProblemDescription { get; set; }
    }

    public class CARSJobDetails
    {
        public string job_id { get; set; }
        public string cust_name { get; set; }
        public string Title { get; set; }
        public string csr_name { get; set; }
        public string cust_key { get; set; }

    }
}