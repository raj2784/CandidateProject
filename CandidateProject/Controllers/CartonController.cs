﻿using CandidateProject.EntityModels;
using CandidateProject.ViewModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.WebSockets;

namespace CandidateProject.Controllers
{
    public class CartonController : Controller
    {
        private readonly CartonContext db = new CartonContext();

        // GET: Carton
        public ActionResult Index()
        {
            var cartons = db.Cartons
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber,
                    CartonTotalItem = c.CartonTotalItem,

                })
                .ToList();

            return View(cartons);
        }

        // GET: Carton/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // GET: Carton/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Carton/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CartonNumber")] Carton carton)
        {
            if (ModelState.IsValid)
            {
                db.Cartons.Add(carton);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(carton);
        }

        // GET: Carton/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // POST: Carton/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CartonNumber")] CartonViewModel cartonViewModel)
        {
            if (ModelState.IsValid)
            {
                var carton = db.Cartons.Find(cartonViewModel.Id);
                carton.CartonNumber = cartonViewModel.CartonNumber;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cartonViewModel);
        }

        // GET: Carton/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Carton carton = db.Cartons.Find(id);
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // POST: Carton/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            try
            {
                var cartonDetail = db.CartonDetails.Where(x => x.CartonId == id).ToList();
                if (cartonDetail != null)
                {
                    foreach (var item in cartonDetail)
                    {
                        db.CartonDetails.Remove(item);
                        db.SaveChanges();
                    }
                }
                var carton = db.Cartons.Where(c => c.Id == id).FirstOrDefault();
                db.Cartons.Remove(carton);
                db.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
            return RedirectToAction("Index");
        }

        public ActionResult RemoveAllEquipmentFromCarton(int? id)
        {
            try
            {
                var cartonDetail = db.CartonDetails.Where(x => x.CartonId == id).ToList();
                if (cartonDetail != null)
                {
                    foreach (var item in cartonDetail)
                    {
                        db.CartonDetails.Remove(item);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            var carton = db.Cartons.Where(c => c.Id == id).FirstOrDefault();

            return RedirectToAction("ViewCartonEquipment", new { id = carton.Id });

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult AddEquipment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonDetailsViewModel()
                {
                    CartonNumber = c.CartonNumber,
                    CartonId = c.Id
                })
                .FirstOrDefault();

            if (carton == null)
            {
                return HttpNotFound();
            }

            var equipment = db.Equipments
                .Where(e => !db.CartonDetails
                .Select(cd => cd.EquipmentId).Contains(e.Id))
                .Select(e => new EquipmentViewModel()
                {
                    Id = e.Id,
                    ModelType = e.ModelType.TypeName,
                    SerialNumber = e.SerialNumber
                })
                .ToList();

            carton.Equipment = equipment;
            return View(carton);
        }

        public ActionResult AddEquipmentToCarton([Bind(Include = "CartonId,EquipmentId")] AddEquipmentViewModel addEquipmentViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var carton = db.Cartons
                        .Include(c => c.CartonDetails)
                        .Where(c => c.Id == addEquipmentViewModel.CartonId)
                        .FirstOrDefault();
                    if (carton == null)
                    {
                        return HttpNotFound();
                    }

                    var equipment = db.Equipments
                        .Where(e => e.Id == addEquipmentViewModel.EquipmentId)
                        .FirstOrDefault();
                    if (equipment == null)
                    {
                        return HttpNotFound();
                    }
                    var cartonItem = db.CartonDetails.Where(c => c.CartonId == addEquipmentViewModel.CartonId).Count();

                    if (cartonItem + 1 <= 10)
                    {
                        carton.CartonTotalItem = cartonItem + 1;

                        var detail = new CartonDetail()
                        {
                            Carton = carton,
                            Equipment = equipment,
                        };

                        carton.CartonDetails.Add(detail);
                        db.SaveChanges();
                    }
                    else
                    {
                        return RedirectToAction("AddEquipment", new { id = addEquipmentViewModel.CartonId });

                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return RedirectToAction("AddEquipment", new { id = addEquipmentViewModel.CartonId });
        }
           

        public ActionResult ViewCartonEquipment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonDetailsViewModel()
                {
                    CartonNumber = c.CartonNumber,
                    CartonId = c.Id,
                    Equipment = c.CartonDetails
                      .Select(cd => new EquipmentViewModel()
                      {
                          Id = cd.EquipmentId,
                          ModelType = cd.Equipment.ModelType.TypeName,
                          SerialNumber = cd.Equipment.SerialNumber
                      })
                })
                .FirstOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);

        }
        public ActionResult RemoveEquipmentOnCarton(int? equipmentId,
                                    RemoveEquipmentViewModel removeEquipmentViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var ct = db.Cartons.Where(cc => cc.Id == removeEquipmentViewModel.CartonId).FirstOrDefault();

                    var caronDetailsByEquipmentId = db.CartonDetails.Include(cd => cd.Equipment)
                                                    .Where(cd => cd.EquipmentId == equipmentId).FirstOrDefault();

                    if (caronDetailsByEquipmentId != null)
                    {
                        db.CartonDetails.Remove(caronDetailsByEquipmentId);
                        db.SaveChanges();

                        var cartonItem = db.CartonDetails.Where(c => c.CartonId == removeEquipmentViewModel.CartonId).Count();

                        ct.CartonTotalItem = cartonItem;
                        db.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                return RedirectToAction("ViewCartonEquipment", new { id = removeEquipmentViewModel.CartonId });

            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        }

    }
}
