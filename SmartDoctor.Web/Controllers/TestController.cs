﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using SmartDoctor.Data.Consts;
using SmartDoctor.Data.ContextModels;
using SmartDoctor.Data.Enums;
using SmartDoctor.Data.Models;
using SmartDoctor.Helper;
using SmartDoctor.Web.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartDoctor.Web.Controllers
{
    public class TestController : Controller
    {
        private readonly IControllerRepository _controllerRepository;
        public TestController(IControllerRepository controllerRepository)
        {
            _controllerRepository = controllerRepository;
        }

        public async Task<IActionResult> Index()
        {
            var role = await _controllerRepository.InitRole(User);
            if (role == RoleTypes.Patient)
            {
                var userId = _controllerRepository.GetUserId(User);
                var testingResponse = JsonConvert.DeserializeObject<MksResponse>(
                   await RequestExecutor.ExecuteRequestAsync(
                       MicroservicesEnum.Testing, RequestUrl.CheckNotViewedAnswer, new Parameter[]{
                           new Parameter("userId", userId, ParameterType.RequestBody)
                           }));
                if (!testingResponse.Success)
                    throw new Exception(testingResponse.Data);
                var result = JsonConvert.DeserializeObject<bool>(testingResponse.Data);
                if (result)
                    return RedirectToAction("AlreadyHasCompleteTest", "Test");
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> AlreadyHasCompleteTest()
        {
            var userId = _controllerRepository.GetUserId(User);
            var testingResponse = JsonConvert.DeserializeObject<MksResponse>(
               await RequestExecutor.ExecuteRequestAsync(
                   MicroservicesEnum.Testing, RequestUrl.GetPreDiseaseId, new Parameter[]{
                           new Parameter("userId", userId, ParameterType.GetOrPost)
                       }));
            if (!testingResponse.Success)
                throw new Exception(testingResponse.Data);
            var diseaseId = JsonConvert.DeserializeObject<long>(testingResponse.Data);
            var medicalResponse = JsonConvert.DeserializeObject<MksResponse>(
               await RequestExecutor.ExecuteRequestAsync(
                   MicroservicesEnum.Medical, RequestUrl.GetDiseaseNameById, new Parameter[]{
                           new Parameter(
                               "diseaseId", diseaseId, ParameterType.GetOrPost)
                       }));
            if (!medicalResponse.Success)
                throw new Exception(medicalResponse.Data);
            ViewBag.PreDiagnos = medicalResponse.Data;
            return View();
        }

        public async Task<IActionResult> GetTest()
        {
            var testingResponse = JsonConvert.DeserializeObject<MksResponse>(
                await RequestExecutor.ExecuteRequestAsync(
                    MicroservicesEnum.Testing, RequestUrl.GetQuestions));
            var questions = new List<Questions>();
            if (!testingResponse.Success)
                throw new Exception(testingResponse.Data);
            else
                questions.AddRange(JsonConvert.DeserializeObject<IEnumerable<Questions>>(testingResponse.Data));
            return View(questions);
        }

        [HttpPost]
        public async Task<IActionResult> PassTheTest([FromBody]AnswerItem[] answers)
        {
            try
            {
                var userId = _controllerRepository.GetUserId(User);
                var testingResponse = JsonConvert.DeserializeObject<MksResponse>(
                   await RequestExecutor.ExecuteRequestAsync(
                       MicroservicesEnum.Testing, RequestUrl.PassTheTest, new Parameter[]{
                           new Parameter("model", JsonConvert.SerializeObject(new AnswerModel
                           {
                               UserId = userId,
                               Answers = answers
                           }), ParameterType.RequestBody)}));
                if (!testingResponse.Success)
                    throw new Exception(testingResponse.Data);
                var answerId = long.Parse(testingResponse.Data);
                var testingResponse2 = JsonConvert.DeserializeObject<MksResponse>(
                   await RequestExecutor.ExecuteRequestAsync(
                       MicroservicesEnum.Testing, RequestUrl.EvaluateAnswer, new Parameter[]{
                           new Parameter("id", answerId, ParameterType.RequestBody) }));
                if (!testingResponse2.Success)
                    throw new Exception(testingResponse2.Data);
                return Json(new { Result = true });
            }
            catch (Exception exception)
            {
                ModelState.AddModelError(nameof(exception), exception.ToString());
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> TryAgain()
        {
            try
            {
                var userId = _controllerRepository.GetUserId(User);
                var testingResponse = JsonConvert.DeserializeObject<MksResponse>(
                   await RequestExecutor.ExecuteRequestAsync(
                       MicroservicesEnum.Testing, RequestUrl.RemoveAnswer, new Parameter[]{
                           new Parameter("userId", userId, ParameterType.RequestBody)
                           }));
                if (!testingResponse.Success)
                    throw new Exception(testingResponse.Data);
                return Json(new { Result = true });
            }
            catch (Exception exception)
            {
                ModelState.AddModelError(nameof(exception), exception.ToString());
                throw;
            }
        }
    }
}