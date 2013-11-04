using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sokvihittar.Controllers
{
    public class SearchController : ApiController
    {
        // GET /api/search/?text=bmv x5
        public string Get(string text)
        {
            // тут надо вызов твоей бибилиотеки вставить
            return "Search results for : " + text;
        }
    }
}