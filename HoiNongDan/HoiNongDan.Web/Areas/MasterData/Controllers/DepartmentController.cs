﻿using Microsoft.AspNetCore.Mvc;
using HoiNongDan.Models;
using HoiNongDan.DataAccess;
using HoiNongDan.Constant;
using Microsoft.AspNetCore.Authorization;
using HoiNongDan.Extensions;
using HoiNongDan.Resources;
using System.Collections.Generic;
using HoiNongDan.DataAccess.Repository;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HoiNongDan.Web.Areas.MasterData.Controllers
{
    [Area("MasterData")]
    [Authorize]
    public class DepartmentController : BaseController
    {
        public DepartmentController(AppDbContext context) :base(context)
        {
        }
        #region Index
        [HoiNongDanAuthorization]
        public IActionResult Index()
        {
            //CreateViewBag();
            return View("Index");
        }
        public IActionResult _Search(DepartmentSearchVM userSearch)
        {
            return ExecuteSearch(() => {
                var data = _context.Departments.AsQueryable();
                if (!String.IsNullOrEmpty(userSearch.Name))
                {
                    data = data.Where(it => it.Name.Contains(userSearch.Name));
                }
                if (userSearch.IdCoso !=null)
                {
                    data = data.Where(it => it.IDCoSo == userSearch.IdCoso);
                }
                if (userSearch.Actived != null)
                {
                    data = data.Where(it => it.Actived == userSearch.Actived);
                }
                var model = data.ToList().Select(it => new DepartmentVM
                {
                    Id = it.Id,
                    Code = it.Code,
                    Name = it.Name,
                    Description = it.Description,
                    Actived = it.Actived,
                    OrderIndex = it.OrderIndex
                });
                //account.userRoless = new  
                return PartialView(model);
            });
        }
        #endregion End Index
        [HoiNongDanAuthorization]
        [HttpGet]
        public IActionResult Upsert(Guid? id)
        {
            DepartmentVM departmentVM = new DepartmentVM();
            if (id != null)
            {
                var item = _context.Departments.SingleOrDefault(it => it.Id == id);
                departmentVM.Id = item.Id;
                departmentVM.Name = item.Name;
                departmentVM.Code = item.Code;
                //departmentVM.IdCoSo = item.IDCoSo;
                departmentVM.Actived = item.Actived;
                departmentVM.Description = item.Description;
                departmentVM.OrderIndex = item.OrderIndex;

            }
            //CreateViewBag(departmentVM.IdCoSo);
            return View(departmentVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Upsert(DepartmentVM obj)
        {
            return ExecuteContainer(() => {
                if (obj.Id == null)
                {
                    // insert 
                    Department insert = new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = obj.Name,
                        Code = obj.Code,
                        Description = obj.Description,
                        OrderIndex = obj.OrderIndex,
                        Actived = true,
                        //IDCoSo = obj.IdCoSo,
                        CreatedAccountId = Guid.NewGuid(),
                        CreatedTime = DateTime.Now

                    };
                    _context.Departments.Add(insert);
                    _context.SaveChanges();
                    return Json(new
                    {
                        Code = System.Net.HttpStatusCode.OK,
                        Success = true,
                        Data = string.Format(LanguageResource.Alert_Create_Success, LanguageResource.Department.ToLower())
                    });
                }
                else
                {
                    var departmentEdit = _context.Departments.SingleOrDefault(it => it.Id == obj.Id);
                    if (departmentEdit != null)
                    {
                        departmentEdit.Actived = obj.Actived == null ? true : obj.Actived.Value;
                        departmentEdit.Name = obj.Name;
                        departmentEdit.Code = obj.Code;
                        departmentEdit.OrderIndex = obj.OrderIndex;
                        //departmentEdit.IDCoSo = obj.IdCoSo;
                        departmentEdit.Description = obj.Description;
                        departmentEdit.LastModifiedAccountId = new Guid(CurrentUser.AccountId!);
                        departmentEdit.LastModifiedTime = DateTime.Now;

                        HistoryModelRepository history = new HistoryModelRepository(_context);
                        _context.Entry(departmentEdit).State = EntityState.Modified;
                        _context.SaveChanges();
                        return Json(new
                        {
                            Code = System.Net.HttpStatusCode.OK,
                            Success = true,
                            Data = string.Format(LanguageResource.Alert_Edit_Success, LanguageResource.Department.ToLower())
                        });
                    }
                    else {
                        return Json(new
                        {
                            Code = System.Net.HttpStatusCode.NotFound,
                            Success = false,
                            Data = "Không tìm thấy thông tin co ma " + obj.Id
                        }); ;
                    }
                    // Edit
                }
            });
            
        }
        #region Delete
        [HttpDelete]
        [HoiNongDanAuthorization]
        public JsonResult Delete(Guid id)
        {
            return ExecuteDelete(() =>
            {
                var del = _context.Departments.FirstOrDefault(p => p.Id == id);


                if (del != null)
                {
                    //_context.Entry(accountInRoleModels).State = EntityState.Deleted;
                    //_context.Entry(account).State = EntityState.Deleted;
                    _context.Remove(del);
                    _context.SaveChanges();

                    return Json(new
                    {
                        Code = System.Net.HttpStatusCode.OK,
                        Success = true,
                        Data = string.Format(LanguageResource.Alert_Delete_Success, LanguageResource.Department.ToLower())
                    });
                }
                else
                {
                    return Json(new
                    {
                        Code = System.Net.HttpStatusCode.NotModified,
                        Success = false,
                        Data = string.Format(LanguageResource.Alert_NotExist_Delete, LanguageResource.Department.ToLower())
                    });
                }
            });
        }
        #endregion Delete
        #region Helper
        //private void CreateViewBag(Guid? IdCoSo = null) {
        //    var MenuList = _context.CoSos.Where(it => it.Actived == true).OrderBy(p => p.OrderIndex).Select(it => new { IdCoSo = it.IdCoSo, TenCoSo = it.TenCoSo }).ToList();
        //    ViewBag.IdCoSo = new SelectList(MenuList, "IdCoSo", "TenCoSo", IdCoSo);
        //}
        #endregion Helper
    }
}
