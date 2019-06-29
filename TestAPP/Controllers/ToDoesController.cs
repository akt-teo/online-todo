using Microsoft.AspNet.Identity;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TestAPP.Models;

namespace TestAPP.Controllers
{
    public class ToDoesController : Controller
    {
       // private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ToDoes
        public ActionResult Index()
        {
            var pool = new PooledRedisClientManager(new string[] { "127.0.0.1" });

            string currentUserID = User.Identity.GetUserId();
            List<ToDoList> todoList;
            using (IRedisClient client = pool.GetClient())
            {
                List<string> keyValues = new List<string>();
                keyValues.Add(currentUserID);
                if(currentUserID == null)
                {
                    return View(new List<ToDo>());
                }
                todoList = client.GetValues<ToDoList>(keyValues);                
            }

            if (todoList.Count == 0 || todoList.SingleOrDefault().todoLists == null)
            {
                return View(new List<ToDo>());
            }
           
            return View(todoList.SingleOrDefault().todoLists);
        }

        // GET: ToDoes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ///
            var pool = new PooledRedisClientManager(new string[] { "127.0.0.1" });
            string currentUserID = User.Identity.GetUserId();
            List<ToDoList> todoList;
            using (IRedisClient client = pool.GetClient())
            {
                List<string> keyValues = new List<string>();
                keyValues.Add(currentUserID);
                if(currentUserID == null)
                {
                    return HttpNotFound();
                }
                todoList = client.GetValues<ToDoList>(keyValues);
            }

            ToDo todo = todoList.SingleOrDefault().todoLists.Find(x => x.Id == id);

            if (todo == null)
            {
                return HttpNotFound();
            }
            return View(todo);
        }

        // GET: ToDoes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ToDoes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Description,Done")] ToDo toDo)
        {
            if (ModelState.IsValid)
            {
                var pool = new PooledRedisClientManager(new string[] { "127.0.0.1" });

                using (IRedisClient client = pool.GetClient())
                {
                    List<ToDoList> usertodoFromDB;
                    //find if exisiting
                    List<string> keyValues = new List<string>();
                    keyValues.Add(User.Identity.GetUserId());
                    if (User.Identity.GetUserId() == null)
                    {
                        return HttpNotFound();
                    }
                    usertodoFromDB = client.GetValues<ToDoList>(keyValues);
                    //
                    if (usertodoFromDB.Count!=0 && usertodoFromDB != null)
                    {
                        int counter = usertodoFromDB.SingleOrDefault().todoLists.Count;
                        toDo.Id = counter + 1;
                        toDo.ChangedOn = DateTime.Now;
                        usertodoFromDB.SingleOrDefault().todoLists.Add(toDo);

                        client.Set(Convert.ToString(usertodoFromDB.SingleOrDefault().Id), usertodoFromDB.SingleOrDefault());
                    }
                    else
                    {
                        ToDoList list = new ToDoList();
                        toDo.Id = 1;
                        toDo.ChangedOn = DateTime.Now;
                        if (User.Identity.GetUserId() == null)
                        {
                            return HttpNotFound();
                        }
                        list.Id = Convert.ToInt64(User.Identity.GetUserId());
                        list.todoLists = new List<ToDo>();
                        list.todoLists.Add(toDo);

                        client.Set(Convert.ToString(list.Id), list);
                    }
                }

                return RedirectToAction("Index");
            }

            return View(toDo);
        }

        // GET: ToDoes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var pool = new PooledRedisClientManager(new string[] { "127.0.0.1" });
            string currentUserID = User.Identity.GetUserId();
            List<ToDoList> usertodoFromDB;
            using (IRedisClient client = pool.GetClient())
            {
                List<string> keyValues = new List<string>();
                keyValues.Add(currentUserID);
                if (currentUserID == null)
                {
                    return HttpNotFound();
                }
                usertodoFromDB = client.GetValues<ToDoList>(keyValues);
            }

            ToDo todo = usertodoFromDB.SingleOrDefault().todoLists.Find(x => x.Id == id);

            if (todo == null)
            {
                return HttpNotFound();
            }

            return View(todo);
        }

        // POST: ToDoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Description,Done")] ToDo toDo)
        {
            if (ModelState.IsValid)
            {
                var pool = new PooledRedisClientManager(new string[] { "127.0.0.1" });

                using (IRedisClient client = pool.GetClient())
                {
                    List<ToDoList> usertodoFromDB;
                    //find if exisiting
                    List<string> keyValues = new List<string>();
                    if (User.Identity.GetUserId() == null)
                    {
                        return HttpNotFound();
                    }
                    keyValues.Add(User.Identity.GetUserId());
                    usertodoFromDB = client.GetValues<ToDoList>(keyValues);
                    //
                    if (usertodoFromDB.Count != 0 && usertodoFromDB != null)
                    {                        
                        toDo.ChangedOn = DateTime.Now;

                        var itemIndex = usertodoFromDB.SingleOrDefault().todoLists.FindIndex(x => x.Id == toDo.Id);
                        var item = usertodoFromDB.SingleOrDefault().todoLists.ElementAt(itemIndex);
                        item.Description = toDo.Description;
                        item.Done = toDo.Done;
                        item.ChangedOn = DateTime.Now;                    

                        client.Set(Convert.ToString(usertodoFromDB.SingleOrDefault().Id), usertodoFromDB.SingleOrDefault());
                    }                    
                }
                return RedirectToAction("Index");
            }
            return View(toDo);
        }

        // GET: ToDoes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ///
            var pool = new PooledRedisClientManager(new string[] { "127.0.0.1" });
            string currentUserID = User.Identity.GetUserId();
            List<ToDoList> usertodoFromDB;
            using (IRedisClient client = pool.GetClient())
            {
                if (currentUserID == null)
                {
                    return HttpNotFound();
                }

                List<string> keyValues = new List<string>();
                keyValues.Add(currentUserID);
                usertodoFromDB = client.GetValues<ToDoList>(keyValues);
            }

            ToDo todo = usertodoFromDB.SingleOrDefault().todoLists.Find(x => x.Id == id);

            if (todo == null)
            {
                return HttpNotFound();
            }

            return View(todo);
        }

        // POST: ToDoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //if (ModelState.IsValid)
            {
                var pool = new PooledRedisClientManager(new string[] { "127.0.0.1" });

                using (IRedisClient client = pool.GetClient())
                {
                    List<ToDoList> usertodoFromDB;
                    //find if exisiting
                    List<string> keyValues = new List<string>();
                    if (User.Identity.GetUserId() == null)
                    {
                        return HttpNotFound();
                    }
                    keyValues.Add(User.Identity.GetUserId());
                    usertodoFromDB = client.GetValues<ToDoList>(keyValues);
                    //
                    if (usertodoFromDB.Count != 0 && usertodoFromDB != null)
                    {
                        var itemIndex = usertodoFromDB.SingleOrDefault().todoLists.FindIndex(x => x.Id == id);
                        usertodoFromDB.SingleOrDefault().todoLists.RemoveAt(itemIndex);
                        
                        client.Set(Convert.ToString(usertodoFromDB.SingleOrDefault().Id), usertodoFromDB.SingleOrDefault());
                    }
                }                
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
