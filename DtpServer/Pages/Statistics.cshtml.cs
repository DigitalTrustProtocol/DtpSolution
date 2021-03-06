﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DtpCore.Repository;
using DtpGraphCore.Model;

namespace DtpServer.Pages
{
    public class StatisticsModel : PageModel
    {

        public readonly TrustDBContext DB;
        public readonly GraphModel Graph;

        public StatisticsModel(TrustDBContext context, GraphModel graphModel)
        {
            DB = context;
            Graph = graphModel;
        }

        public void OnGet()
        {


        }

        /// <summary>
        /// Formats from bytes to KB,MB,GB,TB 
        /// </summary>
        /// <param name="number">Bytes to format</param>
        /// <returns></returns>
        public string AutoSize(long number)
        {
            double tmp = number;
            string suffix = " B ";
            if (tmp > 1024) { tmp = tmp / 1024; suffix = " KB"; }
            if (tmp > 1024) { tmp = tmp / 1024; suffix = " MB"; }
            if (tmp > 1024) { tmp = tmp / 1024; suffix = " GB"; }
            if (tmp > 1024) { tmp = tmp / 1024; suffix = " TB"; }
            return tmp.ToString("n") + suffix;
        }

    }
}