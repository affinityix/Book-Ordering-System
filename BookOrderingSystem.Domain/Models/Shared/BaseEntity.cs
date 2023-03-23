using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookOrderingSystem.Domain.Models.Shared
{
    public class BaseEntity
    {
        public DateTime CreatedAt { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
