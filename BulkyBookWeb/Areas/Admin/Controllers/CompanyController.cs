using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;

namespace BulkyBookWeb.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

     
        public IActionResult Upsert(int? id)
        {

            Company company = new();
           

            if (id == null || id == 0)
            {
                //We want to create Product 
                return View(company);
            }
            else
            {
                //Update Product        
                company = unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
                return View(company);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {

                if (obj.Id == 0)
                {
                    unitOfWork.Company.Add(obj);

                    TempData["success"] = "Product Added Successfully";
                }
                else
                {
                    unitOfWork.Company.Update(obj);

                    TempData["success"] = "Company Updated Successfully";
                }
                 
                unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(obj);
        }

/*        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var categoryFromDb = unitOfWork.Category.GetFirstOrDefault(u=>u.Id==id);

            if (categoryFromDb == null)
                return NotFound();

            return View(categoryFromDb);
        }*/


        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = unitOfWork.Company.GetAll();
            return Json(new { data= companyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var obj = unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);

            if (obj == null)
            {
                return Json(new {success=false,message="Error While deleting" });
            }

            unitOfWork.Company.Remove(obj);
            unitOfWork.Save();
            return Json(new {success=true,message="Delete Successfully" });
        }
        #endregion

    }
}
