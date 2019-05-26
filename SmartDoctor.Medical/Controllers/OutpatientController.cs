﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SmartDoctor.Data.Consts;
using SmartDoctor.Data.JsonModels;
using SmartDoctor.Medical.Core;
using System;
using System.Threading.Tasks;

namespace SmartDoctor.Medical.Controllers
{
    public class OutpatientController : Controller
    {
        private readonly IOutpatientRepository _outpatientRepository;
        public OutpatientController(IOutpatientRepository outpatientRepository)
        {
            _outpatientRepository = outpatientRepository;
        }

        [HttpGet(Scope.GetOutpatients)]
        public async Task<IActionResult> GetOutpatients()
        {
            try
            {
                return Json(
                      new
                      {
                          Success = true,
                          Data = JsonConvert.SerializeObject(await _outpatientRepository.GetAllOutPatients())
                      });
            }
            catch (Exception exception)
            {
                return Json(new { Success = false, exception.Message });
            }
        }

        [HttpGet(Scope.GetOutpatientById)]
        public async Task<IActionResult> GetOutpatientById(long outpatientCardId)
        {
            try
            {
                return Json(
                      new
                      {
                          Success = true,
                          Data = JsonConvert.SerializeObject(await _outpatientRepository.GetOutpatientById(outpatientCardId))
                      });
            }
            catch (Exception exception)
            {
                return Json(new { Success = false, exception.Message });
            }
        }

        [HttpPost(Scope.ChangeCardStatus)]
        public async Task<IActionResult> ChangeCardStatus([FromBody] CardStatusModel model)
        {
            try
            {
                await _outpatientRepository.ChangeStatus(model);
                return Json(
                      new
                      {
                          Success = true,
                          Data = "ok"
                      });
            }
            catch (Exception exception)
            {
                return Json(new { Success = false, exception.Message });
            }
        }
    }
}