using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq;

namespace BulkyBookWeb.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Upsert(int? id)
        {

            ProductVM productVM = new ProductVM
            {
                product = new Product() { },
                CategoryList = unitOfWork.Category.GetAll()
                                                        .Select(
                                                            u => new SelectListItem
                                                            {
                                                                Text = u.Name,
                                                                Value = u.Id.ToString()
                                                            }
                                                         ),

                CoverTypeList = unitOfWork.CoverType.GetAll()
                                                        .Select(
                                                            u => new SelectListItem
                                                            {
                                                                Text = u.Name,
                                                                Value = u.Id.ToString()
                                                            }
                                                         )
            };


            if (id == null || id == 0)
            {
                //We want to create Product 

                //ViewBag.AnyRandomKeyName = AnyKeyValue;             
                //ViewData["AnyRandomKeyName "]= AnyKeyValue;


                return View(productVM);
            }
            else
            {
                //Update Product        
                productVM.product = unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
                return View(productVM);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                string wwwrootPath = webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwrootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    //Deleting old Image for Update
                    if (obj.product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwrootPath, obj.product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.product.ImageUrl = @"\images\products\" + fileName + extension;
                }
            }

            if (obj.product.Id == 0)
                unitOfWork.Product.Add(obj.product);
            else
                unitOfWork.Product.Update(obj.product);

            unitOfWork.Save();
            TempData["success"] = "Product Added Successfully";
            return RedirectToAction("Index");
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
            var productList = unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return Json(new { data = productList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var obj = unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);

            if (obj == null)
            {
                return Json(new { success = false, message = "Error While deleting" });
            }

            var oldImagePath = Path.Combine(webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            unitOfWork.Product.Remove(obj);
            unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successfully" });
        }
        #endregion

    }
}
