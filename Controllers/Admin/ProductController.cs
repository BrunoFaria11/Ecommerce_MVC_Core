﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Ecommerce_MVC_Core.Data;
using Ecommerce_MVC_Core.Models.Admin;
using Ecommerce_MVC_Core.Repository;
using Ecommerce_MVC_Core.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce_MVC_Core.Controllers.Admin
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHostingEnvironment _hosingEnv;
        public ProductController(
            IUnitOfWork unitOfWork,
            IHostingEnvironment hosingEnv
        )
        {
            _hosingEnv = hosingEnv;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string search)
        {
            //Get List of All Products
            var allProducts = _unitOfWork.Repository<Product>().GetAllInclude(x=>x.Category);

            return View(allProducts);
        }

        public IActionResult GetById(int id)
        {

            //Get Product By ID
            var productByID = _unitOfWork.Repository<Product>().GetById(id);

            return View(productByID);
        }

        [HttpGet]
        public IActionResult AddEditProduct(int id)
        {
            try
            {
                ProductViewModel product = new ProductViewModel();
                if (id > 0)
                {

                    var productByID = _unitOfWork.Repository<Product>().GetById(id);
                    product.Id = productByID.Id;
                    product.Name = productByID.Name;
                    product.Description = productByID.Description;
                    product.Price = productByID.Price;
                    product.CategoryId = productByID.CategoryId;
                    product.Discount = productByID.Discount; 
                    product.ImagePath = productByID.ImagePath;
                    product.ImagePath2 = productByID.ImagePath2;
                    product.ImagePath3 = productByID.ImagePath3;




                }

                return PartialView("~/Views/Product/_AddEditProduct.cshtml", product);

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult AddEditProduct(int id, ProductViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return Json("Error");
            //}

            if (id > 0)
            {
                Product product = _unitOfWork.Repository<Product>().GetById(id);
                if (product != null)
                {
                    product.Name = model.Name;
                    product.Description = model.Description;
                    product.Price = model.Price;
                    product.CategoryId = model.CategoryId;
                    product.Discount = model.Discount;
                    product.Active = model.Active;
                    _unitOfWork.Repository<Product>().Update(product);           
                }
                var result = new { Result = "Produto Inserido com sucesso", Id = product.Id };
                return Json(result);
            }
            else
            {
                Product product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    Discount = model.Discount,
                    AddedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    Active = true
                };

                var addProduct = _unitOfWork.Repository<Product>().Insert(product);

                string FilePath = Path.Combine(_hosingEnv.WebRootPath, $"css\\Images\\Products\\Products_{addProduct.Id}");
                System.IO.Directory.CreateDirectory(FilePath);

                var result = new { Result = "Produto Inserido com sucesso", Id = addProduct.Id };
                return Json(result);
            }

        }


        public string UploadImages()
        {
            string result = string.Empty;
            try
            {
                long size = 0;
                var file = Request.Form.Files;
                var getLastProduct = new Product();

                var idProduct = ContentDispositionHeaderValue.Parse(file[0].ContentDisposition).Name.Trim('"');


                if (idProduct != "" && idProduct != "0")
                {
                    getLastProduct = _unitOfWork.Repository<Product>().GetById(Convert.ToInt32(idProduct));
                }
                string FilePath = Path.Combine(_hosingEnv.WebRootPath, $"css\\Images\\Products\\Products_{getLastProduct.Id}");

                bool exists = System.IO.Directory.Exists(FilePath);

                if (!exists)
                    System.IO.Directory.CreateDirectory(FilePath);



                if (getLastProduct != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var filename = $"ProductImage_{getLastProduct.Id }_{ i + 1}";
                        var file_ = Path.Combine(FilePath, filename + ".jpg");

                        size += file[i].Length;

                        using (FileStream fs = System.IO.File.Create(file_))
                        {
                            file[i].CopyTo(fs);
                            fs.Flush();
                        }

                        switch (i)
                        {
                            case 0:
                                getLastProduct.ImagePath = $"/css/Images/Products/Products_{getLastProduct.Id}/{filename}.jpg";
                                break;
                            case 1:
                                getLastProduct.ImagePath2 = $"/css/Images/Products/Products_{getLastProduct.Id}/{filename}.jpg";
                                break;
                            case 2:
                                getLastProduct.ImagePath3 = $"/css/Images/Products/Products_{getLastProduct.Id}/{filename}.jpg";
                                break;
                            default:
                                break;
                        }

                    }
                    _unitOfWork.Repository<Product>().Update(getLastProduct);

                }
            }

            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public IActionResult DeleteProduct(int id)
        {

            //Delete product
            var productByID = _unitOfWork.Repository<Product>().GetById(id);
            _unitOfWork.Repository<Product>().Delete(productByID);

            return RedirectToAction(nameof(Index));
        }

        // [HttpGet]
        // public async Task<IActionResult> AddEditProduct(int id)
        // {
        //     ProductViewModel model = new ProductViewModel();
        //     model.BrandList = _unitOfWork.Repository<Brand>().GetAll().Select(x => new SelectListItem
        //     {
        //         Text = x.Name,
        //         Value = x.Id.ToString()
        //     }).ToList();

        //     model.CategoryList = _unitOfWork.Repository<Category>().GetAll().Select(x => new SelectListItem
        //     {
        //         Text = x.Name,
        //         Value = x.Id.ToString()
        //     }).ToList();

        //     model.UnitList = _unitOfWork.Repository<Unit>().GetAll().Select(x => new SelectListItem
        //     {
        //         Text = x.Name,
        //         Value = x.Id.ToString()
        //     }).ToList();

        //     if (id > 0)
        //     {
        //         Product product =await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        //         model.Name = product.Name;
        //         // model.Code = product.Code;
        //         // model.Tag = product.Tag;
        //         // model.CategoryId = product.CategoryId;
        //         // model.BrandId = product.BrandId;
        //         // model.UnitId = product.UnitId;
        //         model.Description = product.Description;
        //         model.Price = product.Price;
        //         // model.Discount = product.Discount;
        //     }

        //     return PartialView("_AddEditProduct", model);
        // }

        // [HttpPost]
        // public async Task<IActionResult> AddEditProduct(int id, ProductViewModel model)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         ModelState.AddModelError("","Something Wrong");
        //         return View("_AddEditProduct", model);
        //     }
        //     if (id>0)
        //     {
        //         Product product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        //         if (product!=null)
        //         {
        //             product.Name = model.Name;
        //             // product.Code = model.Code;
        //             // product.Tag = model.Tag;
        //             // product.CategoryId = model.CategoryId;
        //             // product.BrandId = model.BrandId;
        //             // product.UnitId = model.UnitId;
        //             product.Description = model.Description;
        //             product.Price = model.Price;
        //             // product.Discount = model.Discount;

        //             product.ModifiedDate = DateTime.Now;
        //             await _unitOfWork.Repository<Product>().UpdateAsync(product);
        //         }

        //     }
        //     else
        //     {
        //         Product product = new Product
        //         {
        //             Name = model.Name,
        //             // Code = model.Code,
        //             // Tag = model.Tag,
        //             // CategoryId = model.CategoryId,
        //             // BrandId = model.BrandId,
        //             // UnitId = model.UnitId,
        //             Description = model.Description,
        //             Price = model.Price,
        //             // Discount = model.Discount,
        //             AddedDate = DateTime.Now,
        //             ModifiedDate = DateTime.Now
        //         };
        //         await _unitOfWork.Repository<Product>().InsertAsync(product);
        //         if (model.Images.Count()>0)
        //         {
        //             await UploadProductImages(model.Images, product.Name, product.Id);
        //         }

        //         if (model.Manual!=null)
        //         {
        //             await UploadProductManual(model.Manual,product.Id,product.Name);
        //         }
        //     }
        //     return RedirectToAction(nameof(Index));
        // }


        // private async Task UploadProductImages(IList<IFormFile> list,string productName,int productId)
        // {
        //     foreach (var file in list)
        //     {
        //         if (file != null && (file.ContentType == "image/png" || file.ContentType == "image/jpg" || file.ContentType == "image/jpeg"))
        //         {
        //             var productImage=new ProductImage
        //             {
        //                 AddedDate = DateTime.Now,
        //                 ModifiedDate = DateTime.Now,
        //                 ProductId = productId,
        //                 Title = productName
        //             };
        //             var uploads = Path.Combine(_hosingEnv.WebRootPath, "uploads/ProductImages");
        //             var fileName = Path.Combine(uploads, productName + "_" + Guid.NewGuid().ToString().Substring(0, Guid.NewGuid().ToString().IndexOf("-", StringComparison.Ordinal)) + ".png");
        //             //Model
        //             productImage.ImagePath = Path.GetFileName(fileName);

        //             await _unitOfWork.Repository<ProductImage>().InsertAsync(productImage);

        //             using (var fileStream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create))
        //             {
        //                 await file.CopyToAsync(fileStream);
        //             }
        //         }
        //     }
        // }

        // public async Task UploadProductManual(IFormFile file,int productId,string productName)
        // {
        //     var pManual=new ProductManual
        //     {
        //         AddedDate = DateTime.Now,
        //         ModifiedDate = DateTime.Now,
        //         ProductId = productId
        //     };

        //     if (file != null && file.ContentType == "application/pdf")
        //     {

        //         var uploads = Path.Combine(_hosingEnv.WebRootPath, "uploads/ProductManuals");
        //         var fileName = Path.Combine(uploads, productName + "_" + productId + ".pdf");
        //         pManual.ManualName = Path.GetFileName(fileName);
        //         await _unitOfWork.Repository<ProductManual>().InsertAsync(pManual);
        //         using (var fileStream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create))
        //         {
        //             await file.CopyToAsync(fileStream);
        //         }

        //         //await model.ImageFile.CopyToAsync(new FileStream(fileName,FileMode.Create));
        //     }
        // }

        // [HttpGet]
        // public async Task<IActionResult> DeleteProduct(int id)
        // {
        //     Product product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

        //     return PartialView("_DeleteProduct", product?.Name);
        // }

        // [HttpPost]
        // public async Task<IActionResult> DeleteProduct(int id, IFormCollection form)
        // {
        //     Product product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        //     if (product != null)
        //     {
        //         await _unitOfWork.Repository<Product>().DeleteAsync(product);
        //     }
        //     return RedirectToAction(nameof(Index));
        // }

        // public IActionResult ListImageView(int id)
        // {
        //     ProductImageListByProduct productImage=new ProductImageListByProduct();
        //     //productImage.ProuctImages = GetProdutcsImages(id);
        //     productImage.Path = productImage.ProuctImages.Max(x => x.ImagePath);

        //     //return PartialView("_ShowImageByProduct", productImage);
        //     return View();
        // }

        // public PartialViewResult GetProdutcsImages(int id)
        // {
        //     List<ProductImageListViewModel> productImageList = new List<ProductImageListViewModel>();
        //     ViewBag.productName =  _unitOfWork.Repository<Product>().GetById(id).Name;
        //      _unitOfWork.Repository<ProductImage>().GetAll().Where(x => x.ProductId == id).ToList().ForEach(x =>
        //     {
        //         ProductImageListViewModel pImage = new ProductImageListViewModel
        //         {
        //             Id = x.Id,
        //             ProductId = x.ProductId,
        //             ImagePath = x.ImagePath,
        //             Title = x.Title,
        //             ProductName =  _unitOfWork.Repository<Product>().GetById(id).Name
        //         };
        //         productImageList.Add(pImage);
        //     });
        //     return PartialView("_ShowImageByProduct", productImageList);

        // }
    }
}