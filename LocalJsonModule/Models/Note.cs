using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalJsonModule.Models
{
    public class Note
    {
        public Guid id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int ownerId { get; set; }
        public DateTime date_of_create { get; set; }
    }
}
