using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestAPP.Models
{
    public class ToDoList
    {
        public long Id { get; set; }

        public List<ToDo> todoLists {get; set;}
    }
}