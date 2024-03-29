﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestAPP.Models
{
    public class ToDo
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public bool Done { get; set; }

        public virtual ApplicationUser User { get; set; }

        public DateTime ChangedOn { get; set; }
    }
}